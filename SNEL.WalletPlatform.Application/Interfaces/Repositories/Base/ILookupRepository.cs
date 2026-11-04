using SNEL.WalletPlatform.Domain.Common;
using SNEL.WalletPlatform.Domain.Common.Base;

namespace SNEL.WalletPlatform.Application.Interfaces.Repositories.Base;

public interface ILookupRepository<TEntity, TKey> : IBaseRepository<TEntity, TKey>
    where TEntity : LookupEntity<TKey>
    where TKey : notnull
{
    Task<IReadOnlyList<TEntity>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<TEntity?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
}