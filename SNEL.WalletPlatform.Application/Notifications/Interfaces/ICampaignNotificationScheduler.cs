namespace SNEL.WalletPlatform.Application.Notifications.Interfaces;

public interface ICampaignNotificationScheduler
{
    Task<bool> ScheduleAsync(Guid campaignId, DateTimeOffset startAtUtc, CancellationToken cancellationToken = default);
    Task<bool> RescheduleAsync(Guid campaignId, DateTimeOffset startAtUtc, CancellationToken cancellationToken = default);
    Task<bool> ScheduleContinuationAsync(
        Guid campaignId,
        DateTimeOffset startAtUtc,
        CancellationToken cancellationToken = default);
    Task UnscheduleAsync(Guid campaignId, CancellationToken cancellationToken = default);
}
