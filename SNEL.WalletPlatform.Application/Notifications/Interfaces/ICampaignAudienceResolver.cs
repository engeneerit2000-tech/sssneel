namespace SNEL.WalletPlatform.Application.Notifications.Interfaces;

public interface ICampaignAudienceResolver
{
    CampaignAudienceResolver? Resolve(string audienceGroupCode);

    IReadOnlyList<CampaignAudienceResolver> GetAll();
}
