using SNEL.WalletPlatform.Application.Notifications.Interfaces;

namespace SNEL.WalletPlatform.Infrastructure.Notifications;

public sealed class CampaignAudienceResolverRegistry(IEnumerable<CampaignAudienceResolver> resolvers)
    : ICampaignAudienceResolver
{
    private readonly IReadOnlyList<CampaignAudienceResolver> _resolvers =
        resolvers
            .OrderBy(x => x.AudienceGroupCode, StringComparer.OrdinalIgnoreCase)
            .ToList();

    public CampaignAudienceResolver? Resolve(string audienceGroupCode)
    {
        if (string.IsNullOrWhiteSpace(audienceGroupCode))
        {
            return null;
        }

        var normalized = audienceGroupCode.Trim();
        return _resolvers.FirstOrDefault(x =>
            string.Equals(x.AudienceGroupCode, normalized, StringComparison.OrdinalIgnoreCase));
    }

    public IReadOnlyList<CampaignAudienceResolver> GetAll() => _resolvers;
}
