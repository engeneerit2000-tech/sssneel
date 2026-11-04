using SNEL.WalletPlatform.Application.Interfaces.Repositories.Base;
using SNEL.WalletPlatform.Domain.Webhooks;

namespace SNEL.WalletPlatform.Application.Webhooks.Interfaces;

public interface IEasypayWebhookEventRepository : IEntityRepository<EasypayWebhookEventEntity, Guid>
{
    Task<bool> AnyByEasypayReferenceAsync(string ReferenceId, CancellationToken cancellationToken = default);
}

