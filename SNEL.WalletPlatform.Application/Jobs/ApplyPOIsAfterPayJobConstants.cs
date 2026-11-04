using Quartz;

namespace SNEL.WalletPlatform.Application.Jobs;

public static class ApplyPOIsAfterPayJobConstants
{
    public static string JobName = "ApplyPOIsAfterPayJob";
    public static string JobGroup = "POI";
    public static string TriggerGroup = "POI";
    public static string PaymentIdKey = "PaymentId";
    public static int TotalAttempts = 10;
    public static int InitialDelaySeconds = 60;
    public static int IntervalSeconds = 30;
    public static double SaftyMargin = 1.2;

    public static JobKey JobKey => new(JobName, JobGroup);

    public static TriggerKey CreateTriggerKey(Guid paymentId) =>
        new TriggerKey($"{JobName}-trigger-{paymentId:N}", TriggerGroup);
}
