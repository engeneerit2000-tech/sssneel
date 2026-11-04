using SNEL.WalletPlatform.Domain.Common;
using SNEL.WalletPlatform.Domain.Common.Base;

namespace SNEL.WalletPlatform.Application.Interfaces.Repositories.Base;

public interface ITranslatableLookupRepository<TEntity, TKey> : IBaseRepository<TEntity, TKey>
    where TEntity : LookupEntity<TKey>
    where TKey : notnull
{
    Task<IReadOnlyList<TEntity>> GetActiveAsync(string LanguageCode,CancellationToken cancellationToken = default);
    Task<TEntity?> GetByCodeAsync(string LanguageCode,string code, CancellationToken cancellationToken = default);
}