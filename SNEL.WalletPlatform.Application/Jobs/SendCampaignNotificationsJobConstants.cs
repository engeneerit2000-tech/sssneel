namespace SNEL.WalletPlatform.Application.Jobs;

using Quartz;

public static class SendCampaignNotificationsJobConstants
{
    public const string JobName = "SendCampaignNotificationsJob";
    public const string JobGroup = "campaigns";
    public const string TriggerGroup = "campaigns";
    public const string CampaignIdKey = "CampaignId";

    public static JobKey JobKey => new(JobName, JobGroup);

    public static TriggerKey CreateStartTriggerKey(Guid campaignId) =>
        new($"{JobName}-start-{campaignId:N}", TriggerGroup);

    public static TriggerKey CreateContinuationTriggerKey(Guid campaignId) =>
        new($"{JobName}-continue-{campaignId:N}", TriggerGroup);
}
