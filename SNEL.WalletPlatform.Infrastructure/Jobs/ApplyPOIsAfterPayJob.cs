using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using SNEL.WalletPlatform.Application.BillingServices.Interfaces;
using SNEL.WalletPlatform.Application.Jobs;
using SNEL.WalletPlatform.Application.Payments.Interfaces;
using SNEL.WalletPlatform.Domain.Payments;

namespace SNEL.WalletPlatform.Infrastructure.Jobs;

//TODO: Shake, make sure that deleting triggers of cron-job (thoe ones configured from appsettings) ar not deleiting paymentStatus job triggers.
public sealed class ApplyPOIsAfterPayJob(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<ApplyPOIsAfterPayJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var paymentIdRaw = context.MergedJobDataMap.GetString(ApplyPOIsAfterPayJobConstants.PaymentIdKey);
        if (!Guid.TryParse(paymentIdRaw, out var paymentId) || paymentId == Guid.Empty)
        {
            logger.LogWarning("ApplyPOIsAfterPayJob skipped because PaymentId job data is missing or invalid.");
            return;
        }
        using var scope = serviceScopeFactory.CreateScope();
        var billingService = scope.ServiceProvider.GetRequiredService<IBillingServiceService>();
        var applyResult = await billingService.ApplyPOIsAfterPay(paymentId);
        if (!applyResult.IsSuccess)
        {
            logger.LogError($"Failed to apply POIs after payment: {applyResult.Message} with inner error:  {applyResult.Message}");
        }

        logger.LogInformation(
            $"ApplyPOIsAfterPayJob applied POIs for payment {paymentId}.",
            paymentId);
    }
}
