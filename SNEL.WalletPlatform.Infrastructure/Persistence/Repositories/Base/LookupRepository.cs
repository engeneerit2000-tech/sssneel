using Microsoft.EntityFrameworkCore;
using SNEL.WalletPlatform.Application.Interfaces.Repositories.Base;
using SNEL.WalletPlatform.Domain.Common;
using SNEL.WalletPlatform.Domain.Common.Base;
using SNEL.WalletPlatform.Infrastructure.Persistence;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Repositories.Base;

public abstract class LookupRepository<TEntity, TKey> : BaseRepository<TEntity, TKey>, ILookupRepository<TEntity, TKey>
    where TEntity : LookupEntity<TKey>
    where TKey : notnull
{
    protected LookupRepository(ApplicationDbContext context) : base(context)
    {
    }

    public virtual async Task<IReadOnlyList<TEntity>> GetActiveAsync(
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(x => x.Active)
            .ToListAsync(cancellationToken);
    }
    public virtual async Task<TEntity?> GetByCodeAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Code == code, cancellationToken);
    }
}