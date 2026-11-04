using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using SNEL.WalletPlatform.Application.Interfaces.Repositories.Base;
using SNEL.WalletPlatform.Domain.Common;
using SNEL.WalletPlatform.Domain.Common.Base;

namespace SNEL.WalletPlatform.Application.Tests;

internal abstract class FakeLookupRepository<TEntity, TKey>(IEnumerable<TEntity> items) : ILookupRepository<TEntity, TKey>
    where TEntity : LookupEntity<TKey>
    where TKey : notnull
{
    protected readonly List<TEntity> Items = items.ToList();

    public TEntity? GetById(TKey id) => Items.FirstOrDefault(x => x.Id.Equals(id));
    public IEnumerable<TEntity> GetAll() => Items;
    public IQueryable<TEntity> GetAllQueryable(Expression<Func<TEntity, bool>>? predicate = null)
    {
        var query = Items.AsQueryable();
        return predicate is null ? query : query.Where(predicate);
    }

    public void Add(TEntity entity) => Items.Add(entity);
    public Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Items.Add(entity);
        return Task.CompletedTask;
    }

    public void AddRange(IEnumerable<TEntity> entities) => Items.AddRange(entities);
    public void Update(TEntity entity) { }
    public void UpdateRange(IEnumerable<TEntity> entities) { }
    public void Remove(TEntity entity) => Items.Remove(entity);
    public void RemoveRange(IEnumerable<TEntity> entities)
    {
        foreach (var entity in entities.ToArray())
        {
            Items.Remove(entity);
        }
    }

    public Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default) => Task.FromResult(Items.FirstOrDefault(x => x.Id.Equals(id)));
    public Task<TEntity?> GetByIdWithLockAsync(TKey id, CancellationToken cancellationToken = default) => Task.FromResult(Items.FirstOrDefault(x => x.Id.Equals(id)));
    public Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult<IEnumerable<TEntity>>(Items);
    public Task<IQueryable<TEntity>> GetAllQueryableAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        var query = Items.AsQueryable();
        return Task.FromResult(predicate is null ? query : query.Where(predicate));
    }
    public Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        Items.AddRange(entities);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<TEntity>> GetActiveAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<TEntity>>(Items.Where(x => x.Active).ToList());

    public Task<TEntity?> GetByCodeAsync(string code, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.FirstOrDefault(x => x.Code == code));

    public Task<List<TEntity>> GetByIdsAsync(IReadOnlyCollection<TKey> ids, CancellationToken cancellationToken = default) => Task.FromResult(Items.Where(x => ids.Contains(x.Id)).ToList());    

    public Task<List<TEntity>> GetByIdsWithLockAsync(IEnumerable<TKey> ids, CancellationToken cancellationToken = default) => Task.FromResult(Items.Where(x => ids.Contains(x.Id)).ToList());    
}
