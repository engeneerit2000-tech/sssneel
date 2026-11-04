using Quartz;

namespace SNEL.WalletPlatform.Application.Jobs;

public static class PaymentStatusPollingJobConstants
{
    public static string JobName = "PollPaymentStatusJob";
    public static string JobGroup = "payments";
    public static string TriggerGroup = "payments";
    public static string PaymentIdKey = "PaymentId";
    /// <summary>Fallback when payment method <c>config.status_polling</c> is absent.</summary>
    public const int DefaultTotalAttempts = 10;

    public const int DefaultInitialDelaySeconds = 60;

    public const int DefaultIntervalSeconds = 15;

    public static JobKey JobKey => new(JobName, JobGroup);

    public static TriggerKey CreateTriggerKey(Guid paymentId) =>
        new TriggerKey($"{JobName}-trigger-{paymentId:N}", TriggerGroup);
}
