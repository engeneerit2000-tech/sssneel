using System.Linq.Expressions;
using SNEL.WalletPlatform.Domain.Common;

namespace SNEL.WalletPlatform.Application.Interfaces.Repositories.Base;

public interface IEntityRepository<TEntity,TKey> : IBaseRepository<TEntity, TKey>
    where TEntity : TrackedEntity<TKey>
    where TKey : notnull
{
}
