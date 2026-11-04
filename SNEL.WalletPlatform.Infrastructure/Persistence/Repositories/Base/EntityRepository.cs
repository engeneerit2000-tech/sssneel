using SNEL.WalletPlatform.Application.Interfaces.Repositories.Base;
using SNEL.WalletPlatform.Domain.Common;
using SNEL.WalletPlatform.Infrastructure.Persistence;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Repositories.Base;

public abstract class EntityRepository<TEntity, TKey> : BaseRepository<TEntity, TKey>, IEntityRepository<TEntity, TKey>
    where TEntity : TrackedEntity<TKey>
    where TKey : notnull
{
    protected EntityRepository(ApplicationDbContext context) : base(context)
    {
    }
}