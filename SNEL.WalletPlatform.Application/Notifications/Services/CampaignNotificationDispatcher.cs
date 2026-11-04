using Medallion.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SNEL.WalletPlatform.Application.Common.Enums;
using SNEL.WalletPlatform.Application.Interfaces.UnitOfWork;
using SNEL.WalletPlatform.Application.Jobs.BatchProcessing;
using SNEL.WalletPlatform.Application.Notification.Enums;
using SNEL.WalletPlatform.Application.Notification.Models.Internal;
using SNEL.WalletPlatform.Application.Notification.Services;
using SNEL.WalletPlatform.Application.Notifications.Configuration;
using SNEL.WalletPlatform.Application.Notifications.Interfaces;
using SNEL.WalletPlatform.Domain.Customers;
using SNEL.WalletPlatform.Domain.Notifications;
using SNEL.WalletPlatform.Domain.Users;

namespace SNEL.WalletPlatform.Application.Notifications.Services;

public sealed class CampaignNotificationDispatcher(
    IUnitOfWork unitOfWork,
    NotificationService notificationService,
    IBatchSleepCalculator batchSleepCalculator,
    IDistributedLockProvider distributedLockProvider,
    ICampaignNotificationScheduler campaignNotificationScheduler,
    ICampaignAudienceResolver campaignAudienceResolver,
    IOptions<CampaignNotificationSettings> settings,
    ILogger<CampaignNotificationDispatcher> logger) : ICampaignNotificationDispatcher
{
    public async Task<CampaignBatchDispatchResult> DispatchNextBatchAsync(
        Guid campaignId,
        CancellationToken cancellationToken = default)
    {
        await using var handle = await distributedLockProvider.AcquireLockAsync(
            $"campaign-notification:{campaignId:N}",
            cancellationToken: cancellationToken);

        var campaign = await unitOfWork.CampaignRepository.GetByIdWithTranslationsAsync(campaignId, cancellationToken);
        if (campaign is null)
        {
            logger.LogWarning("Campaign {CampaignId} was not found while dispatching notifications.", campaignId);
            return new CampaignBatchDispatchResult(false, 0, null);
        }

        if (campaign.Status is CampaignStatus.Completed or CampaignStatus.Cancelled)
        {
            logger.LogInformation(
                "Campaign {CampaignId} dispatch skipped because status is {Status}.",
                campaignId,
                campaign.Status);
            await campaignNotificationScheduler.UnscheduleAsync(campaignId, cancellationToken);
            return new CampaignBatchDispatchResult(false, 0, null);
        }

        if (campaign.Translations.Count == 0)
        {
            logger.LogWarning("Campaign {CampaignId} has no translations. Marking as completed.", campaignId);
            campaign.MarkCompleted();
            unitOfWork.CampaignRepository.Update(campaign);
            await unitOfWork.CompleteAsync();
            await campaignNotificationScheduler.UnscheduleAsync(campaignId, cancellationToken);
            return new CampaignBatchDispatchResult(false, 0, null);
        }

        campaign.BeginDispatch();

        var audienceResolver = campaignAudienceResolver.Resolve(campaign.AudienceGroupCode);
        if (audienceResolver is null)
        {
            logger.LogError(
                "Campaign {CampaignId} has unsupported audience group '{AudienceGroupCode}'. Marking as completed.",
                campaignId,
                campaign.AudienceGroupCode);
            campaign.MarkCompleted();
            unitOfWork.CampaignRepository.Update(campaign);
            await unitOfWork.CompleteAsync();
            await campaignNotificationScheduler.UnscheduleAsync(campaignId, cancellationToken);
            return new CampaignBatchDispatchResult(false, 0, null);
        }

        var batchSize = settings.Value.BatchSize;
        var batch = await audienceResolver.GetActiveBatchAsync(
            campaign.LastProcessedCustomerId,
            batchSize,
            cancellationToken);

        if (batch.SourceCount == 0)
        {
            campaign.MarkCompleted();
            unitOfWork.CampaignRepository.Update(campaign);
            await unitOfWork.CompleteAsync();
            await campaignNotificationScheduler.UnscheduleAsync(campaignId, cancellationToken);

            logger.LogInformation(
                "Campaign {CampaignId} completed. No more recipients for audience group '{AudienceGroupCode}'.",
                campaignId,
                campaign.AudienceGroupCode);
            return new CampaignBatchDispatchResult(false, 0, null);
        }

        var hasMore = batch.SourceCount == batchSize;

        if (batch.Users.Count == 0)
        {
            if (batch.ProgressCursorId.HasValue)
            {
                campaign.RecordBatchProgress(batch.ProgressCursorId.Value);
            }

            if (!hasMore)
            {
                campaign.MarkCompleted();
            }

            unitOfWork.CampaignRepository.Update(campaign);
            await unitOfWork.CompleteAsync();

            logger.LogInformation(
                "Campaign {CampaignId} skipped audience batch with no notifiable users. SourceCount={SourceCount}, HasMore={HasMore}.",
                campaignId,
                batch.SourceCount,
                hasMore);

            return new CampaignBatchDispatchResult(
                hasMore,
                0,
                hasMore ? CalculateSleep(batch.SourceCount) : null);
        }

        var requestedChannels = new List<NotificationChannel> { NotificationChannel.Push };
        var enabledChannels = notificationService.GetTargettedChannels(requestedChannels);
        if (enabledChannels.Count == 0)
        {
            logger.LogWarning(
                "Campaign {CampaignId} batch skipped because push notifications are disabled.",
                campaignId);
            return new CampaignBatchDispatchResult(hasMore, 0, null);
        }

        var notification = BuildNotificationModel(campaign, batch.Users, enabledChannels);
        var sent = await notificationService.SendNotificationsAsync([notification]);

        // Always advance so one failed batch cannot stall the campaign.
        if (batch.ProgressCursorId.HasValue)
        {
            campaign.RecordBatchProgress(batch.ProgressCursorId.Value);
        }

        if (!hasMore)
        {
            campaign.MarkCompleted();
        }

        unitOfWork.CampaignRepository.Update(campaign);
        await unitOfWork.CompleteAsync();

        if (!sent)
        {
            logger.LogError(
                "Campaign {CampaignId} failed to send batch for {UserCount} users after cursor {ProgressCursorId}. Continuing to next batch.",
                campaignId,
                batch.Users.Count,
                batch.ProgressCursorId);
            return new CampaignBatchDispatchResult(
                hasMore,
                0,
                hasMore ? CalculateSleep(batch.SourceCount) : null);
        }

        logger.LogInformation(
            "Campaign {CampaignId} dispatched notifications to {UserCount} users for audience '{AudienceGroupCode}'. HasMore={HasMore}, Status={Status}.",
            campaignId,
            batch.Users.Count,
            campaign.AudienceGroupCode,
            hasMore,
            campaign.Status);

        return new CampaignBatchDispatchResult(
            hasMore,
            batch.Users.Count,
            hasMore ? CalculateSleep(batch.SourceCount) : null);
    }

    private TimeSpan CalculateSleep(int batchCount) =>
        TimeSpan.FromMilliseconds(
            batchSleepCalculator.CalculateSleepMilliseconds(
                batchCount,
                settings.Value.SleepMillisecondsPerRecipient,
                settings.Value.MinSleepBetweenBatchesMs,
                settings.Value.MaxSleepBetweenBatchesMs));

    private static NotificationDetailsModel BuildNotificationModel(
        CampaignEntity campaign,
        IReadOnlyList<UserEntity> users,
        List<NotificationChannel> enabledChannels)
    {
        var parameters = campaign.Translations
            .OrderBy(t => t.LanguageCode)
            .Select(translation => new ParameterDataModel
            {
                Language = MapLanguage(translation.LanguageCode),
                Params = [translation.Name.Trim(), translation.Message]
            })
            .ToList();

        return new NotificationDetailsModel
        {
            IsTransactional = false,
            NotificationCode = NotificationCodeEnum.CAMPAIGN_ANNOUNCEMENT,
            NotificationType = NotificationTypeEnum.CAMPAIGN,
            Params = parameters,
            SenderId = null,
            ReferenceType = null,
            ReferenceId = campaign.Id.ToString(),
            OverrideReceiverChannelsPreferences = false,
            RequestedChannels = enabledChannels,
            Receivers = users.ToList(),
            SendingAt = DateTime.UtcNow,
            SaveInAppNotification = true
        };
    }

    private static LanguageEnum MapLanguage(string languageCode) =>
        languageCode.Trim().ToLowerInvariant() switch
        {
            "fr" => LanguageEnum.FR,
            _ => LanguageEnum.EN
        };
}
