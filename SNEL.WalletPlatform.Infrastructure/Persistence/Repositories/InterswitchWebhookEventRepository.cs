using Microsoft.EntityFrameworkCore;
using SNEL.WalletPlatform.Application.Webhooks.Interfaces;
using SNEL.WalletPlatform.Domain.Webhooks;
using SNEL.WalletPlatform.Infrastructure.Persistence.Repositories.Base;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Repositories;

public sealed class InterswitchWebhookEventRepository(ApplicationDbContext context)
    : EntityRepository<InterswitchWebhookEventEntity,Guid>(context), IInterswitchWebhookEventRepository
{
    public Task<bool> AnyByUuidAsync(string Uuid)
    {
        return context.InterswitchWebhookEvents.AnyAsync(x => x.Uuid == Uuid);
    }
}
