using SNEL.WalletPlatform.Application.Interfaces.Repositories.Base;
using SNEL.WalletPlatform.Domain.Webhooks;

namespace SNEL.WalletPlatform.Application.Webhooks.Interfaces;

public interface IAfrimonyWebhookEventRepository : IEntityRepository<AfrimonyWebhookEventEntity, Guid>
{
    Task<bool> AnyByMmTransactionIdAsync(string mmTransactionId, CancellationToken cancellationToken = default);
}
