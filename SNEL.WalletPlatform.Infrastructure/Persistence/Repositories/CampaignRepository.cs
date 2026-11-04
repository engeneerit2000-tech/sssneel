using Microsoft.EntityFrameworkCore;
using SNEL.WalletPlatform.Application.Notifications.Interfaces;
using SNEL.WalletPlatform.Domain.Notifications;
using SNEL.WalletPlatform.Infrastructure.Persistence.Repositories.Base;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Repositories;

public sealed class CampaignRepository(ApplicationDbContext context)
    : EntityRepository<CampaignEntity, Guid>(context), ICampaignRepository
{
    public async Task<IReadOnlyCollection<CampaignEntity>> GetAllWithTranslationsAsync(
        CancellationToken cancellationToken = default)
    {
        return await context.Campaigns
            .AsNoTracking()
            .Include(x => x.Translations)
            .OrderByDescending(x => x.StartAtUtc)
            .ThenByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public Task<CampaignEntity?> GetByIdWithTranslationsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return context.Campaigns
            .Include(x => x.Translations)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public override async Task<CampaignEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await GetByIdWithTranslationsAsync(id, cancellationToken);
    }
}
