using Microsoft.EntityFrameworkCore;
using SNEL.WalletPlatform.Application.Webhooks.Interfaces;
using SNEL.WalletPlatform.Domain.Webhooks;
using SNEL.WalletPlatform.Infrastructure.Persistence.Repositories.Base;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Repositories;

public sealed class AfrimonyWebhookEventRepository(ApplicationDbContext context)
    : EntityRepository<AfrimonyWebhookEventEntity, Guid>(context), IAfrimonyWebhookEventRepository
{
    public Task<bool> AnyByMmTransactionIdAsync(string mmTransactionId, CancellationToken cancellationToken = default) =>
        context.AfrimonyWebhookEvents.AnyAsync(
            x => x.MmTransactionId == mmTransactionId,
            cancellationToken);
}
