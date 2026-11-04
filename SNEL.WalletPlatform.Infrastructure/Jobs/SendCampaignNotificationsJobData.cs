using Quartz;
using SNEL.WalletPlatform.Application.Jobs;

namespace SNEL.WalletPlatform.Infrastructure.Jobs;

/// <summary>
/// With Quartz JDBC store <c>useProperties = true</c>, JobDataMap values must be strings when persisted.
/// </summary>
internal static class SendCampaignNotificationsJobData
{
    internal static Guid ReadCampaignId(JobDataMap map)
    {
        if (!map.TryGetString(SendCampaignNotificationsJobConstants.CampaignIdKey, out var raw)
            || !Guid.TryParse(raw, out var campaignId))
        {
            throw new InvalidOperationException("CampaignId is missing or invalid in job data.");
        }

        return campaignId;
    }

    internal static JobDataMap CreateMap(Guid campaignId)
    {
        return new JobDataMap
        {
            { SendCampaignNotificationsJobConstants.CampaignIdKey, campaignId.ToString() }
        };
    }
}
