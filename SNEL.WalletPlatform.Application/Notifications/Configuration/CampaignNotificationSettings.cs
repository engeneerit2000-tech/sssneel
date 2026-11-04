namespace SNEL.WalletPlatform.Application.Notifications.Configuration;

public sealed class CampaignNotificationSettings
{
    public const string SectionName = "CampaignNotifications";

    public int BatchSize { get; set; } = 500;

    /// <summary>
    /// Sleep between batches is calculated as <c>BatchSize * SleepMillisecondsPerRecipient</c>,
    /// clamped by min/max settings.
    /// </summary>
    public int SleepMillisecondsPerRecipient { get; set; } = 5;

    public int MinSleepBetweenBatchesMs { get; set; } = 100;

    public int MaxSleepBetweenBatchesMs { get; set; } = 60_000;
}
