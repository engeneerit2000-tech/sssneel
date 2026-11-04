using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using SNEL.WalletPlatform.Application.BillingServices.Interfaces;
using SNEL.WalletPlatform.Application.BillingServices.Services;
using SNEL.WalletPlatform.Application.Invoicing;
using SNEL.WalletPlatform.Application.Invoicing.Interfaces;
using SNEL.WalletPlatform.Domain.Billing;
using SNEL.WalletPlatform.Infrastructure.Invoicing;
using SNEL.WalletPlatform.Infrastructure.Persistence;
using System.Diagnostics;

namespace SNEL.WalletPlatform.Infrastructure.Jobs;

/// <summary>
/// Quartz job that pulls invoice data from Oracle using the pull-invoices query (GCO_ENCOURS_FACTURES + payments).
/// Processes page-by-page to avoid loading a huge number of rows into memory. Options (e.g. PageSize) can be passed via config.
/// After writing all pages to InvoiceStagings, calls SyncInvoicesFromStagingAsync once (writes InvoiceSecondaryStagings + clears staging).
/// </summary>
[DisallowConcurrentExecution]
public class SyncGcoEnCoursFacturesJob : IJob
{
    private const int DefaultPageSize = 100000;
    private const string RegisterNotificationOnPullOptionKey = "RegisterNotificationOnPull";
    private const string NotificationTriggerTimeOptionKey = "NotificationTriggerTime";
    private const string RegionsKey = "Regions";

    private readonly IInvoiceRetrieval invoiceRetrieval;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IMapper _mapper;
    private readonly ISnelConfigService _configService;
    private readonly ILogger<SyncGcoEnCoursFacturesJob> _logger;

    public SyncGcoEnCoursFacturesJob(
        IInvoiceRetrieval invoiceRetrieval,
        IServiceScopeFactory scopeFactory,
        IMapper mapper,
        ISnelConfigService configService,
        ILogger<SyncGcoEnCoursFacturesJob> logger)
    {
        this.invoiceRetrieval = invoiceRetrieval;
        _scopeFactory = scopeFactory;
        _mapper = mapper;
        _logger = logger;
        _configService = configService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var sw = Stopwatch.StartNew();
        var jobKey = context.JobDetail.Key.ToString();
        var sourceSystem = context.MergedJobDataMap.ContainsKey("SourceSystem")
            ? (context.MergedJobDataMap.GetString("SourceSystem") ?? "")
            : "OracleSnel";
        _logger.LogInformation(
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
            using (var scope = _scopeFactory.CreateScope())
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

            _logger.LogInformation(
                "Pull-invoices Started: PageSize={PageSize}, start={Start:O}, end={End:O}, command timeout = 5 min.",
                pageSize, startTs, endTs);

            var totalCount = 0L;
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // Start with a clean staging table for this job/source (delete before pull)
                await dbContext.Database.ExecuteSqlRawAsync(
                        "DELETE FROM \"InvoiceStagings\" WHERE \"JobKey\" = {0} AND \"SourceSystem\" = {1}",
                        new object[] { jobKey, sourceSystem },
                        context.CancellationToken);
                _logger.LogInformation(
                    "Staging cleared for JobKey={JobKey}, SourceSystem={SourceSystem} before pull.",
                    jobKey, sourceSystem);
                var areaCodesStr = context.MergedJobDataMap.ContainsKey(RegionsKey)
           ? (context.MergedJobDataMap.GetString(RegionsKey) ?? string.Empty).Trim()
           : string.Empty;
                var areaCodes = string.IsNullOrEmpty(areaCodesStr) ? [] : areaCodesStr.Split(',');
                var configs = await _configService.GetAllRequiredConfigAsync(areaCodes, context.CancellationToken);
                foreach (var config in configs)
                {
                    await invoiceRetrieval.GetAllAsync(
                    config.Key, startTs, endTs, pageSize, jobKey, sourceSystem, dbContext, context.CancellationToken);
                }
                _logger.LogInformation(
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
                _logger.LogInformation(
                    "Pull-invoices Completed: Read {ReadCount} from Oracle in {ElapsedMs} ms.",
                    totalCount, sw.ElapsedMilliseconds);
            }
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "SyncGcoEnCoursFacturesJob failed after {ElapsedMs} ms.", sw.ElapsedMilliseconds);
            throw;
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
