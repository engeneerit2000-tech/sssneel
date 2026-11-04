using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using SNEL.WalletPlatform.Application.Jobs;
using SNEL.WalletPlatform.Application.Payments.Interfaces;

namespace SNEL.WalletPlatform.Infrastructure.Jobs;

//TODO: Shake, make sure that deleting triggers of cron-job (thoe ones configured from appsettings) ar not deleiting paymentStatus job triggers.
public sealed class PollPaymentStatusJob(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<PollPaymentStatusJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var paymentIdRaw = context.MergedJobDataMap.GetString(PaymentStatusPollingJobConstants.PaymentIdKey);
        if (!Guid.TryParse(paymentIdRaw, out var paymentId) || paymentId == Guid.Empty)
        {
            logger.LogWarning("PollPaymentStatusJob skipped because PaymentId job data is missing or invalid.");
            return;
        }

        int? statusPollingAttempt = null;
        if (context.Trigger is ISimpleTrigger simpleTrigger)
        {
            statusPollingAttempt = simpleTrigger.TimesTriggered + 1;
        }

        using var scope = serviceScopeFactory.CreateScope();
        var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();
        var paymentStatusResult = await paymentService.SyncPaymentStatusWithPsp(paymentId, statusPollingAttempt);
        if (!paymentStatusResult.IsSuccess)
        {
            logger.LogError($"Failed to get payment status for payment '{paymentId}': {paymentStatusResult.Message} with inner error:  {paymentStatusResult.Message}");
        }

        logger.LogInformation(
            "PollPaymentStatusJob fetched status {PaymentStatus} for payment {PaymentId}.",
            paymentStatusResult.Value,
            paymentId);
    }
}
