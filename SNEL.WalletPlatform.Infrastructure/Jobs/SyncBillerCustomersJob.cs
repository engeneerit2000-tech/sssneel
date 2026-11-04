using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using SNEL.WalletPlatform.Application.BillingServices.Interfaces;
using SNEL.WalletPlatform.Application.Invoicing.Interfaces;
using SNEL.WalletPlatform.Infrastructure.Persistence;
using System.Diagnostics;

namespace SNEL.WalletPlatform.Infrastructure.Jobs;

/// <summary>
/// Pulls biller customers from Oracle (GCO_RACCORDEMENTS) into staging, merges into <c>BillerCustomers</c>, and clears staging.
/// </summary>
[DisallowConcurrentExecution]
public sealed class SyncBillerCustomersJob : IJob
{
    private const int DefaultPageSize = 100_000;
    private const string DefaultBillerCode = "SNEL";
    private const string RegionsKey = "Regions";

    private readonly IBillerCustomerOraclePull _oraclePull;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SyncBillerCustomersJob> _logger;

    public SyncBillerCustomersJob(
        IBillerCustomerOraclePull oraclePull,
        IServiceScopeFactory scopeFactory,
        ILogger<SyncBillerCustomersJob> logger)
    {
        _oraclePull = oraclePull;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
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

        _logger.LogInformation(
            "SyncBillerCustomersJob started. JobKey={JobKey}, BillerCode={BillerCode}, PageSize={PageSize}",
            jobKey, billerCode, pageSize);

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var configService = scope.ServiceProvider.GetRequiredService<ISnelConfigService>();
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
            _logger.LogInformation("BillerCustomerStagings cleared before pull (JobKey={JobKey}).", jobKey);

            var areaCodesStr = context.MergedJobDataMap.ContainsKey(RegionsKey)
           ? (context.MergedJobDataMap.GetString(RegionsKey) ?? string.Empty).Trim()
           : string.Empty;
            var areaCodes = string.IsNullOrEmpty(areaCodesStr) ? areaCodesStr.Split(',') : [];
            var configs = await configService.GetAllRequiredConfigAsync(areaCodes, context.CancellationToken);

            foreach (var config in configs)
            {
                var pulled = await _oraclePull.PullAllToStagingAsync(
                    config.Key,
                    billerId,
                    jobKey,
                    pageSize,
                    dbContext,
                    context.CancellationToken);
                _logger.LogInformation(
                    "Biller customer pull staged {Pulled} rows from Oracle for BillerId={BillerId}.",
                    pulled, billerId);
            }
            var mergeService = scope.ServiceProvider.GetRequiredService<IBillerCustomerStagingMergeService>();
            var (inserted, updated) = await mergeService.MergeFromStagingAsync(billerId, jobKey, context.CancellationToken);
            _logger.LogInformation(
                "Biller customer merge: inserted={Inserted}, updated={Updated}, BillerId={BillerId}.",
                inserted, updated, billerId);

            await dbContext.Database.ExecuteSqlRawAsync(
                @"DELETE FROM ""BillerCustomerStagings"" WHERE ""JobKey"" = {0}",
                new object[] { jobKey },
                context.CancellationToken);
            _logger.LogInformation("BillerCustomerStagings cleared after merge (JobKey={JobKey}).", jobKey);

            sw.Stop();
            _logger.LogInformation(
                "SyncBillerCustomersJob completed in {ElapsedMs} ms. JobKey={JobKey}",
                sw.ElapsedMilliseconds, jobKey);
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "SyncBillerCustomersJob failed after {ElapsedMs} ms.", sw.ElapsedMilliseconds);
            throw;
        }
    }
}
