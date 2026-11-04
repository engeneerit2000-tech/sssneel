using Medallion.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Quartz;
using SNEL.WalletPlatform.Application;
using SNEL.WalletPlatform.Application.Billers.Interfaces;
using SNEL.WalletPlatform.Application.BillingServices.Interfaces;
using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Common.Enums;
using SNEL.WalletPlatform.Application.Common.Localization;
using SNEL.WalletPlatform.Application.Interfaces.UnitOfWork;
using SNEL.WalletPlatform.Application.Invoicing.Interfaces;
using SNEL.WalletPlatform.Infrastructure.Persistence;
using System.Diagnostics;

namespace SNEL.WalletPlatform.Infrastructure.Jobs;

[DisallowConcurrentExecution]
public sealed class OpenSnelDailyJournalJob(
    IServiceScopeFactory scopeFactory,
    ILogger<OpenSnelDailyJournalJob> logger,
    IBillerCustomerOraclePull oraclePull,
    IInvoiceRetrieval invoiceRetrieval) : IJob
{
    private const string DefaultAreaCode = "06";
    private const int DefaultPageSize = 100_000;
    private const string DefaultBillerCode = "SNEL";

    private const string RegisterNotificationOnPullOptionKey = "RegisterNotificationOnPull";
    private const string NotificationTriggerTimeOptionKey = "NotificationTriggerTime";
    private const string RegionsKey = "Regions";

    public async Task Execute(IJobExecutionContext context)
    {
        var sw = Stopwatch.StartNew();
        logger.LogInformation("OpenSnelDailyJournalJob started.");

        try
        {
            using var scope = scopeFactory.CreateScope();
            var configService = scope.ServiceProvider.GetRequiredService<ISnelConfigService>();            
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var journalService = scope.ServiceProvider.GetRequiredService<ISnelOracleJournalService>();
            var billingServiceService = scope.ServiceProvider.GetRequiredService<IBillingServiceService>();
            var distributedLockProvider = scope.ServiceProvider.GetRequiredService<IDistributedLockProvider>();            

            var lockKey = $"{LockNames.SNEL_DAILY_JOB}";
            await using var locker = await distributedLockProvider.TryAcquireLockAsync(lockKey, TimeSpan.FromMinutes(10));
            if (locker is null)
            {
                logger.LogError($"Failed to open SNEL journal, It's locked");
            }
                        
            //Waiting for all payments to complete
            Thread.Sleep(TimeSpan.FromMinutes(6));
            var result = await billingServiceService.ApplyCompletedPaidPaymentsForBillingServiceAsync("SNEL");
            //var result = await billingServiceService.ApplyCompletedPaidPaymentsAsync();

            var areaCodesStr = context.MergedJobDataMap.ContainsKey(RegionsKey)
           ? (context.MergedJobDataMap.GetString(RegionsKey) ?? string.Empty).Trim()
           : string.Empty;
            var areaCodes = string.IsNullOrEmpty(areaCodesStr) ? areaCodesStr.Split(',') : [];
            var configs = await configService.GetAllRequiredConfigAsync(areaCodes, context.CancellationToken);

            foreach (var config in configs) {
                var journal = await journalService.ClosePreviousAndOpenDailyJournalAsync(
                config.Key,
                DateTime.UtcNow,
                config.Value.PosCode,
                context.CancellationToken);
                var allocation = await journalService.GetCurrentAllocationAsync(
                    config.Key,
                    DateTime.UtcNow,
                    config.Value.PosCode,
                    context.CancellationToken);
                // Update the journal config locally
                await configService.SetJournalAsync(
                    journal.NumJournal,
                    allocation.NextItemNumber,
                    config.Key,
                    context.CancellationToken);

                locker?.Dispose();

                await SyncBillerCustomers(config.Key, context);
                await SyncInvoices(config.Key, context);

                logger.LogInformation(
                "OpenSnelDailyJournalJob completed for POS {PosCode} with journal {NumJournal} on {JournalDate:yyyy-MM-dd} in {ElapsedMs} ms.",
                journal.PosCode,
                journal.NumJournal,
                journal.DatJournal,
                sw.ElapsedMilliseconds);
            }

            

            sw.Stop();            
        }
        catch (Exception ex)
        {
            sw.Stop();
            logger.LogError(ex, "OpenSnelDailyJournalJob failed after {ElapsedMs} ms.", sw.ElapsedMilliseconds);
            throw;
        }
    }

    public async Task SyncBillerCustomers(string areaCode, IJobExecutionContext context)
    {
        var sw = Stopwatch.StartNew();
        var jobKey = context.JobDetail.Key.ToString();

        var billerCode = context.MergedJobDataMap.ContainsKey("BillerCode")
            ? (context.MergedJobDataMap.GetString("BillerCode") ?? DefaultBillerCode).Trim()
            : DefaultBillerCode;

        var pageSize = context.MergedJobDataMap.ContainsKey("PageSize")
            ? context.MergedJobDataMap.GetInt("PageSize")
            : 0;
        if (pageSize <= 0)
            pageSize = DefaultPageSize;

        logger.LogInformation(
            "SyncBillerCustomersJob started. JobKey={JobKey}, BillerCode={BillerCode}, PageSize={PageSize}",
            jobKey, billerCode, pageSize);

        try
        {
            using var scope = scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var normalized = billerCode.ToLowerInvariant();
            var billerId = await dbContext.Billers
                .AsNoTracking()
                .Where(b => b.Code.ToLower() == normalized)
                .Select(b => b.Id)
                .FirstOrDefaultAsync(context.CancellationToken);

            if (billerId == 0)
            {
                throw new InvalidOperationException(
                    $"Biller not found for biller customer sync. Ensure a Biller exists with Code='{billerCode}'.");
            }

            await dbContext.Database.ExecuteSqlRawAsync(
                @"DELETE FROM ""BillerCustomerStagings"" WHERE ""JobKey"" = {0}",
                new object[] { jobKey },
                context.CancellationToken);
            logger.LogInformation("BillerCustomerStagings cleared before pull (JobKey={JobKey}).", jobKey);

            var pulled = await oraclePull.PullAllToStagingAsync(
                areaCode,
                billerId,
                jobKey,
                pageSize,
                dbContext,
                context.CancellationToken);
            logger.LogInformation(
                "Biller customer pull staged {Pulled} rows from Oracle for BillerId={BillerId}.",
                pulled, billerId);

            var mergeService = scope.ServiceProvider.GetRequiredService<IBillerCustomerStagingMergeService>();
            var (inserted, updated) = await mergeService.MergeFromStagingAsync(billerId, jobKey, context.CancellationToken);
            logger.LogInformation(
                "Biller customer merge: inserted={Inserted}, updated={Updated}, BillerId={BillerId}.",
                inserted, updated, billerId);

            await dbContext.Database.ExecuteSqlRawAsync(
                @"DELETE FROM ""BillerCustomerStagings"" WHERE ""JobKey"" = {0}",
                new object[] { jobKey },
                context.CancellationToken);
            logger.LogInformation("BillerCustomerStagings cleared after merge (JobKey={JobKey}).", jobKey);

            sw.Stop();
            logger.LogInformation(
                "SyncBillerCustomersJob completed in {ElapsedMs} ms. JobKey={JobKey}",
                sw.ElapsedMilliseconds, jobKey);
        }
        catch (Exception ex)
        {
            sw.Stop();
            logger.LogError(ex, "SyncBillerCustomersJob failed after {ElapsedMs} ms.", sw.ElapsedMilliseconds);            
        }
    }

    public async Task SyncInvoices(string areaCode, IJobExecutionContext context)
    {
        var sw = Stopwatch.StartNew();
        var jobKey = context.JobDetail.Key.ToString();
        var sourceSystem = context.MergedJobDataMap.ContainsKey("SourceSystem")
            ? (context.MergedJobDataMap.GetString("SourceSystem") ?? "")
            : "OracleSnel";
        logger.LogInformation(
            "SyncGcoEnCoursFacturesJob started (pull-invoices query). JobKey={JobKey}, SourceSystem={SourceSystem}",
            jobKey, sourceSystem);

        try
        {
            var pageSize = context.MergedJobDataMap.ContainsKey("PageSize")
                ? context.MergedJobDataMap.GetInt("PageSize")
                : 0;
            if (pageSize <= 0)
                pageSize = DefaultPageSize;

            DateTime startTs;
            DateTime endTs;
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                if (await dbContext.Invoices.AnyAsync())
                {
                    var maxUpdate = await dbContext.Invoices.MaxAsync(i => i.UpdatedAtUtc.DateTime);
                    startTs = maxUpdate.Date;
                }
                else
                {
                    startTs = DateTime.UtcNow.Date.AddDays(-1);
                }
            }

            endTs = DateTime.UtcNow.Date;

            if (context.MergedJobDataMap.ContainsKey("StartTs") && context.MergedJobDataMap.ContainsKey("EndTs"))
            {
                var startTsStr = context.MergedJobDataMap.GetString("StartTs");
                var endTsStr = context.MergedJobDataMap.GetString("EndTs");
                if (!string.IsNullOrEmpty(startTsStr) && !string.IsNullOrEmpty(endTsStr)
                                                      && DateTime.TryParse(startTsStr, null,
                                                          System.Globalization.DateTimeStyles.RoundtripKind,
                                                          out var parsedStart)
                                                      && DateTime.TryParse(endTsStr, null,
                                                          System.Globalization.DateTimeStyles.RoundtripKind,
                                                          out var parsedEnd))
                {
                    startTs = parsedStart.Kind == DateTimeKind.Unspecified
                        ? DateTime.SpecifyKind(parsedStart, DateTimeKind.Utc)
                        : parsedStart.ToUniversalTime();
                    endTs = parsedEnd.Kind == DateTimeKind.Unspecified
                        ? DateTime.SpecifyKind(parsedEnd, DateTimeKind.Utc)
                        : parsedEnd.ToUniversalTime();
                }
            }

            logger.LogInformation(
                "Pull-invoices Started: PageSize={PageSize}, start={Start:O}, end={End:O}, command timeout = 5 min.",
                pageSize, startTs, endTs);

            var totalCount = 0L;
            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // Start with a clean staging table for this job/source (delete before pull)
                await dbContext.Database.ExecuteSqlRawAsync(
                        "DELETE FROM \"InvoiceStagings\" WHERE \"JobKey\" = {0} AND \"SourceSystem\" = {1}",
                        new object[] { jobKey, sourceSystem },
                        context.CancellationToken);
                logger.LogInformation(
                    "Staging cleared for JobKey={JobKey}, SourceSystem={SourceSystem} before pull.",
                    jobKey, sourceSystem);

                await invoiceRetrieval.GetAllAsync(
                    areaCode, startTs, endTs, pageSize, jobKey, sourceSystem, dbContext, context.CancellationToken);
                logger.LogInformation(
                    "Pull-invoices Staged: PageSize={PageSize}, start={Start:O}, end={End:O}, command timeout = 5 min.",
                    pageSize, startTs, endTs);
                var syncService = scope.ServiceProvider.GetRequiredService<IInvoiceStagingSyncService>();
                var registerNotificationOnPull = context.MergedJobDataMap.ContainsKey(RegisterNotificationOnPullOptionKey) &&
                                                 context.MergedJobDataMap.GetBoolean(RegisterNotificationOnPullOptionKey);
                var notificationTriggerTime = ResolveNotificationTriggerTime(context);
                await syncService.SyncInvoicesFromStagingAsync(
                    jobKey,
                    sourceSystem,
                    registerNotificationOnPull,
                    notificationTriggerTime);

                sw.Stop();
                logger.LogInformation(
                    "Pull-invoices Completed: Read {ReadCount} from Oracle in {ElapsedMs} ms.",
                    totalCount, sw.ElapsedMilliseconds);
            }
        }
        catch (Exception ex)
        {
            sw.Stop();
            logger.LogError(ex, "SyncGcoEnCoursFacturesJob failed after {ElapsedMs} ms.", sw.ElapsedMilliseconds);            
        }
    }

    private static TimeSpan? ResolveNotificationTriggerTime(IJobExecutionContext context)
    {
        if (!context.MergedJobDataMap.ContainsKey(NotificationTriggerTimeOptionKey))
            return null;

        var value = context.MergedJobDataMap.GetString(NotificationTriggerTimeOptionKey);
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return TimeSpan.TryParse(value, out var parsed) ? parsed : null;
    }
}
