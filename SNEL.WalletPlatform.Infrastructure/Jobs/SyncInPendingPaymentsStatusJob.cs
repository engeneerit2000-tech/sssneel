using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using SNEL.WalletPlatform.Application.Payments.Interfaces;

namespace SNEL.WalletPlatform.Infrastructure.Jobs;

[DisallowConcurrentExecution]
public sealed class SyncInPendingPaymentsStatusJob(
    IServiceScopeFactory scopeFactory,
    ILogger<SyncInPendingPaymentsStatusJob> logger) : IJob
{
    private const int DefaultPageSize = 500;
    private const int DefaultMinimumAgeMinutes = 60;

    public async Task Execute(IJobExecutionContext context)
    {
        var sw = Stopwatch.StartNew();
        var pageSize = SyncInPendingPaymentsStatusJobData.ReadPageSize(context.MergedJobDataMap, DefaultPageSize);
        var minimumAgeMinutes = SyncInPendingPaymentsStatusJobData.ReadMinimumAgeMinutes(
            context.MergedJobDataMap,
            DefaultMinimumAgeMinutes);
        var cancellationToken = context.CancellationToken;

        logger.LogInformation(
            "SyncInPendingPaymentsStatusJob started (pageSize={PageSize}, minimumAgeMinutes={MinimumAgeMinutes}).",
            pageSize,
            minimumAgeMinutes);

        try
        {
            using var scope = scopeFactory.CreateScope();
            var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();

            var result = await paymentService.SyncInPendingPaymentsStatusWithPspAsync(
                pageSize,
                minimumAgeMinutes,
                cancellationToken);
            if (!result.IsSuccess)
            {
                throw new InvalidOperationException(
                    result.Message ?? "Bulk IN payment status sync failed.");
            }

            sw.Stop();
            logger.LogInformation(
                "SyncInPendingPaymentsStatusJob finished in {ElapsedMs} ms. ProcessedPayments={ProcessedPayments}.",
                sw.ElapsedMilliseconds,
                result.Value);
        }
        catch (Exception ex)
        {
            sw.Stop();
            logger.LogError(ex, "SyncInPendingPaymentsStatusJob failed after {ElapsedMs} ms.", sw.ElapsedMilliseconds);
            throw;
        }
    }
}
