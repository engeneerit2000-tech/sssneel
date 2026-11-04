using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using SNEL.WalletPlatform.Application.Notifications.Interfaces;

namespace SNEL.WalletPlatform.Infrastructure.Jobs;

public sealed class SendCampaignNotificationsJob(
    IServiceScopeFactory scopeFactory,
    ILogger<SendCampaignNotificationsJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var sw = Stopwatch.StartNew();
        var ct = context.CancellationToken;
        var campaignId = SendCampaignNotificationsJobData.ReadCampaignId(context.MergedJobDataMap);

        logger.LogInformation("SendCampaignNotificationsJob started for campaign {CampaignId}.", campaignId);

        try
        {
            using var scope = scopeFactory.CreateScope();
            var dispatcher = scope.ServiceProvider.GetRequiredService<ICampaignNotificationDispatcher>();
            var scheduler = scope.ServiceProvider.GetRequiredService<ICampaignNotificationScheduler>();

            var result = await dispatcher.DispatchNextBatchAsync(campaignId, ct);
            if (result.HasMore && result.SleepBeforeNextBatch is { } sleep)
            {
                var scheduled = await scheduler.ScheduleContinuationAsync(
                    campaignId,
                    DateTimeOffset.UtcNow.Add(sleep),
                    ct);

                if (!scheduled)
                {
                    logger.LogInformation(
                        "Campaign {CampaignId} continuation was not scheduled because the campaign is no longer active.",
                        campaignId);
                }
            }
            else if (!result.HasMore)
            {
                await scheduler.UnscheduleAsync(campaignId, ct);
            }

            sw.Stop();
            logger.LogInformation(
                "SendCampaignNotificationsJob finished for campaign {CampaignId} in {ElapsedMs} ms. Processed={ProcessedCount}, HasMore={HasMore}.",
                campaignId,
                sw.ElapsedMilliseconds,
                result.ProcessedCount,
                result.HasMore);
        }
        catch (Exception ex)
        {
            sw.Stop();
            logger.LogError(
                ex,
                "SendCampaignNotificationsJob failed for campaign {CampaignId} after {ElapsedMs} ms.",
                campaignId,
                sw.ElapsedMilliseconds);
            throw;
        }
    }
}
