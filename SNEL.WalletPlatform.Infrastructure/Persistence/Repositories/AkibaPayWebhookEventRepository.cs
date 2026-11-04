using Microsoft.EntityFrameworkCore;
using SNEL.WalletPlatform.Application.Webhooks.Interfaces;
using SNEL.WalletPlatform.Domain.Webhooks;
using SNEL.WalletPlatform.Infrastructure.Persistence.Repositories.Base;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Repositories;

public sealed class AkibaPayWebhookEventRepository(ApplicationDbContext context)
    : EntityRepository<AkibaPayWebhookEventEntity, Guid>(context), IAkibaPayWebhookEventRepository
{
    public Task<bool> AnyByAkibaReferenceAsync(string akibaReference, CancellationToken cancellationToken = default) =>
        context.AkibaPayWebhookEvents.AnyAsync(
            x => x.AkibaReference == akibaReference,
            cancellationToken);
}

