namespace SNEL.WalletPlatform.Application.Notifications.Interfaces;

public sealed record CampaignBatchDispatchResult(
    bool HasMore,
    int ProcessedCount,
    TimeSpan? SleepBeforeNextBatch);

public interface ICampaignNotificationDispatcher
{
    Task<CampaignBatchDispatchResult> DispatchNextBatchAsync(
        Guid campaignId,
        CancellationToken cancellationToken = default);
}
