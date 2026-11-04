using SNEL.WalletPlatform.Application.Interfaces.Repositories.Base;
using SNEL.WalletPlatform.Domain.Webhooks;

namespace SNEL.WalletPlatform.Application.Webhooks.Interfaces;

public interface IOrangeWebhookEventRepository : IEntityRepository<OrangeWebhookEventEntity, Guid>
{
}
