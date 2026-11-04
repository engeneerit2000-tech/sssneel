using System.Linq.Expressions;
using SNEL.WalletPlatform.Domain.Common;

namespace SNEL.WalletPlatform.Application.Interfaces.Repositories.Base;

public interface IBaseRepository<TEntity,TKey> where TEntity : BaseEntity<TKey> where TKey:notnull
{
    TEntity? GetById(TKey id);
    Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);
    Task<List<TEntity>> GetByIdsAsync(IReadOnlyCollection<TKey> ids, CancellationToken cancellationToken = default);
    Task<TEntity?> GetByIdWithLockAsync(TKey id, CancellationToken cancellationToken = default);

    Task<List<TEntity>> GetByIdsWithLockAsync(IEnumerable<TKey> ids, CancellationToken cancellationToken = default);
    
    IEnumerable<TEntity> GetAll();
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    
    IQueryable<TEntity> GetAllQueryable(Expression<Func<TEntity, bool>>? predicate = null);
    Task<IQueryable<TEntity>> GetAllQueryableAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default);

    void Add(TEntity entity);
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    
    void AddRange(IEnumerable<TEntity> entities);
    Task AddRangeAsync(IEnumerable<TEntity> entities,CancellationToken cancellationToken = default);
    
    void Update(TEntity entity);
    void UpdateRange(IEnumerable<TEntity> entities);
    void Remove(TEntity entity);
    void RemoveRange(IEnumerable<TEntity> entities);


}
