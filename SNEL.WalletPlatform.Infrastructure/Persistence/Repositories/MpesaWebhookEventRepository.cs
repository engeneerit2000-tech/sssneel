using SNEL.WalletPlatform.Application.Webhooks.Interfaces;
using SNEL.WalletPlatform.Domain.Webhooks;
using SNEL.WalletPlatform.Infrastructure.Persistence.Repositories.Base;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Repositories;

public sealed class MpesaWebhookEventRepository(ApplicationDbContext context)
    : EntityRepository<MpesaWebhookEventEntity, Guid>(context), IMpesaWebhookEventRepository;
