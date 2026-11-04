using Microsoft.EntityFrameworkCore;
using SNEL.WalletPlatform.Application.Lookups.Interfaces;
using SNEL.WalletPlatform.Domain.Lookups;
using SNEL.WalletPlatform.Infrastructure.Persistence.Repositories.Base;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Repositories;

public sealed class CenterRepository(ApplicationDbContext context)
    : LookupRepository<CenterEntity, int>(context), ICenterRepository
{
    public Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var trimmed = code.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return Task.FromResult(false);
        }

        var normalized = trimmed.ToLowerInvariant();
        return context.Centers.AnyAsync(
            x => x.Active && x.Code.ToLower() == normalized,
            cancellationToken);
    }
}
