using SNEL.WalletPlatform.Application.Interfaces.Repositories.Base;
using SNEL.WalletPlatform.Domain.Notifications;

namespace SNEL.WalletPlatform.Application.Notifications.Interfaces;

public interface ICampaignRepository : IEntityRepository<CampaignEntity, Guid>
{
    Task<IReadOnlyCollection<CampaignEntity>> GetAllWithTranslationsAsync(CancellationToken cancellationToken = default);
    Task<CampaignEntity?> GetByIdWithTranslationsAsync(Guid id, CancellationToken cancellationToken = default);
}
