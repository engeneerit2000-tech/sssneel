using Microsoft.Extensions.Logging;
using Quartz;
using SNEL.WalletPlatform.Application.Interfaces.UnitOfWork;
using SNEL.WalletPlatform.Application.Jobs;
using SNEL.WalletPlatform.Application.Notifications.Interfaces;
using SNEL.WalletPlatform.Domain.Notifications;

namespace SNEL.WalletPlatform.Infrastructure.Notifications;

public sealed class CampaignNotificationScheduler(
    ISchedulerFactory schedulerFactory,
    IUnitOfWork unitOfWork,
    ILogger<CampaignNotificationScheduler> logger) : ICampaignNotificationScheduler
{
    public Task<bool> ScheduleAsync(
        Guid campaignId,
        DateTimeOffset startAtUtc,
        CancellationToken cancellationToken = default) =>
        ScheduleTriggerAsync(
            SendCampaignNotificationsJobConstants.CreateStartTriggerKey(campaignId),
            campaignId,
            startAtUtc,
            cancellationToken);

    public async Task<bool> RescheduleAsync(
        Guid campaignId,
        DateTimeOffset startAtUtc,
        CancellationToken cancellationToken = default)
    {
        await UnscheduleAsync(campaignId, cancellationToken);
        return await ScheduleAsync(campaignId, startAtUtc, cancellationToken);
    }

    public Task<bool> ScheduleContinuationAsync(
        Guid campaignId,
        DateTimeOffset startAtUtc,
        CancellationToken cancellationToken = default) =>
        ScheduleTriggerAsync(
            SendCampaignNotificationsJobConstants.CreateContinuationTriggerKey(campaignId),
            campaignId,
            startAtUtc,
            cancellationToken);

    public async Task UnscheduleAsync(Guid campaignId, CancellationToken cancellationToken = default)
    {
        var scheduler = await schedulerFactory.GetScheduler(cancellationToken);
        var triggerKeys = new[]
        {
            SendCampaignNotificationsJobConstants.CreateStartTriggerKey(campaignId),
            SendCampaignNotificationsJobConstants.CreateContinuationTriggerKey(campaignId)
        };

        foreach (var triggerKey in triggerKeys)
        {
            if (await scheduler.CheckExists(triggerKey, cancellationToken))
            {
                await scheduler.UnscheduleJob(triggerKey, cancellationToken);
                logger.LogInformation(
                    "Unscheduled campaign notification trigger {TriggerGroup}.{TriggerName}.",
                    triggerKey.Group,
                    triggerKey.Name);
            }
        }
    }

    private async Task<bool> ScheduleTriggerAsync(
        TriggerKey triggerKey,
        Guid campaignId,
        DateTimeOffset startAtUtc,
        CancellationToken cancellationToken)
    {
        if (!await CanScheduleCampaignAsync(campaignId, cancellationToken))
            return false;

        var scheduler = await schedulerFactory.GetScheduler(cancellationToken);
        if (!await scheduler.CheckExists(SendCampaignNotificationsJobConstants.JobKey, cancellationToken))
        {
            throw new InvalidOperationException(
                $"Quartz job {SendCampaignNotificationsJobConstants.JobKey} is not registered.");
        }

        if (await scheduler.CheckExists(triggerKey, cancellationToken))
            await scheduler.UnscheduleJob(triggerKey, cancellationToken);

        var startImmediately = startAtUtc <= DateTimeOffset.UtcNow;

        var triggerBuilder = TriggerBuilder.Create()
            .WithIdentity(triggerKey)
            .ForJob(SendCampaignNotificationsJobConstants.JobKey)
            .UsingJobData(SendCampaignNotificationsJobConstants.CampaignIdKey, campaignId.ToString())
            .WithSimpleSchedule(x => x
                .WithRepeatCount(0)
                .WithMisfireHandlingInstructionFireNow());

        if (startImmediately)
            triggerBuilder.StartNow();
        else
            triggerBuilder.StartAt(startAtUtc);

        var trigger = triggerBuilder.Build();

        await scheduler.ScheduleJob(trigger, cancellationToken);

        logger.LogInformation(
            startImmediately
                ? "Scheduled campaign notification trigger {TriggerGroup}.{TriggerName} for campaign {CampaignId} to start immediately (requested StartAtUtc {RequestedStartAtUtc} was in the past)."
                : "Scheduled campaign notification trigger {TriggerGroup}.{TriggerName} for campaign {CampaignId} at {StartAtUtc}.",
            triggerKey.Group,
            triggerKey.Name,
            campaignId,
            startImmediately ? startAtUtc : startAtUtc);

        return true;
    }

    private async Task<bool> CanScheduleCampaignAsync(Guid campaignId, CancellationToken cancellationToken)
    {
        var campaign = await unitOfWork.CampaignRepository.GetByIdAsync(campaignId, cancellationToken);
        if (campaign is null)
        {
            logger.LogWarning(
                "Skipped scheduling campaign {CampaignId} because it was not found.",
                campaignId);
            await UnscheduleAsync(campaignId, cancellationToken);
            return false;
        }

        if (!campaign.CanBeScheduled())
        {
            logger.LogWarning(
                "Skipped scheduling campaign {CampaignId} because status is {Status}.",
                campaignId,
                campaign.Status);
            await UnscheduleAsync(campaignId, cancellationToken);
            return false;
        }

        return true;
    }
}
