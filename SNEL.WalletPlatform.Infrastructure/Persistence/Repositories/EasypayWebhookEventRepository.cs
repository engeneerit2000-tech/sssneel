using Microsoft.EntityFrameworkCore;
using SNEL.WalletPlatform.Application.Webhooks.Interfaces;
using SNEL.WalletPlatform.Domain.Webhooks;
using SNEL.WalletPlatform.Infrastructure.Persistence.Repositories.Base;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Repositories;

public sealed class EasypayWebhookEventRepository(ApplicationDbContext context)
    : EntityRepository<EasypayWebhookEventEntity, Guid>(context), IEasypayWebhookEventRepository
{
    public Task<bool> AnyByEasypayReferenceAsync(string ReferenceId, CancellationToken cancellationToken = default) =>
        context.EasypayWebhookEvents.AnyAsync(
            x => x.ReferenceId == ReferenceId,
            cancellationToken);
}

