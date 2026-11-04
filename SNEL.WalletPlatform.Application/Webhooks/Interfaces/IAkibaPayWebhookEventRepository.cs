using SNEL.WalletPlatform.Application.Interfaces.Repositories.Base;
using SNEL.WalletPlatform.Domain.Webhooks;

namespace SNEL.WalletPlatform.Application.Webhooks.Interfaces;

public interface IAkibaPayWebhookEventRepository : IEntityRepository<AkibaPayWebhookEventEntity, Guid>
{
    Task<bool> AnyByAkibaReferenceAsync(string akibaReference, CancellationToken cancellationToken = default);
}

