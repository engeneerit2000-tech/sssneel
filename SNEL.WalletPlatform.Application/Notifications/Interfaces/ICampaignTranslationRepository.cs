using SNEL.WalletPlatform.Application.Notifications.Models.Output;

namespace SNEL.WalletPlatform.Application.Notifications.Interfaces;

public interface ICampaignTranslationRepository
{
    Task<bool> CampaignExistsAsync(Guid campaignId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CampaignTranslationOutputModel>> GetAsync(
        Guid campaignId,
        CancellationToken cancellationToken = default);

    Task<(CampaignTranslationOutputModel Translation, bool WasAdded)> UpsertAsync(
        Guid campaignId,
        string languageCode,
        string name,
        string message,
        CancellationToken cancellationToken = default);
}
