using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using SNEL.WalletPlatform.Application.BillingServices.Interfaces;

namespace SNEL.WalletPlatform.Infrastructure.Jobs;

[DisallowConcurrentExecution]
public sealed class ApplyCompletedBillPaymentsJob(
    IServiceScopeFactory scopeFactory,
    ILogger<ApplyCompletedBillPaymentsJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var sw = Stopwatch.StartNew();
        logger.LogInformation("ApplyCompletedBillPaymentsJob started.");

        try
        {
            using var scope = scopeFactory.CreateScope();
            var billingService = scope.ServiceProvider.GetRequiredService<IBillingServiceService>();
            var result = await billingService.ApplyCompletedPaidPaymentsAsync();
            if (!result.IsSuccess)
            {
                throw new InvalidOperationException(result.Message ?? "Bulk payment apply failed.");
            }

            sw.Stop();
            logger.LogInformation(
                "ApplyCompletedBillPaymentsJob completed successfully in {ElapsedMs} ms.",
                sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            sw.Stop();
            logger.LogError(ex, "ApplyCompletedBillPaymentsJob failed after {ElapsedMs} ms.", sw.ElapsedMilliseconds);
            throw;
        }
    }
}
