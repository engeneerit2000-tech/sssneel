using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using SNEL.WalletPlatform.Application.Interfaces.Repositories.Base;
using SNEL.WalletPlatform.Domain.Common;
using SNEL.WalletPlatform.Infrastructure.Persistence;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Repositories.Base;

public abstract class BaseRepository<TEntity, TKey> : IBaseRepository<TEntity, TKey>
    where TEntity : BaseEntity<TKey>
    where TKey : notnull
{
    protected readonly ApplicationDbContext Context;
    protected readonly DbSet<TEntity> DbSet;

    protected BaseRepository(ApplicationDbContext context)
    {
        Context = context;
        DbSet = context.Set<TEntity>();
    }

    public virtual TEntity? GetById(TKey id)
    {
        return DbSet.Find(id);
    }

    public virtual async Task<TEntity?> GetByIdAsync(
        TKey id,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    public virtual async Task<List<TEntity>> GetByIdsAsync( IReadOnlyCollection<TKey> ids, CancellationToken cancellationToken = default)
    {
        if (ids is null)
            throw new ArgumentNullException(nameof(ids));

        if (ids.Count == 0)
            return new List<TEntity>();

        return await DbSet
            .Where(e => ids.Contains(EF.Property<TKey>(e, "Id")))
            .ToListAsync(cancellationToken);
    }

    public virtual async Task<TEntity?> GetByIdWithLockAsync(
        TKey id,
        CancellationToken cancellationToken = default)
    {
        var entityType = Context.Model.FindEntityType(typeof(TEntity))
                         ?? throw new InvalidOperationException($"Entity type {typeof(TEntity).Name} is not mapped.");

        var primaryKey = entityType.FindPrimaryKey()
                         ?? throw new InvalidOperationException($"Entity type {typeof(TEntity).Name} does not have a primary key.");

        if (primaryKey.Properties.Count != 1)
            throw new NotSupportedException(
                $"GetByIdWithLockAsync supports only single-column primary keys. " +
                $"{typeof(TEntity).Name} has a composite key.");

        if (Context.Database.CurrentTransaction is null)
            throw new InvalidOperationException(
                "GetByIdWithLockAsync requires an active transaction. " +
                "Start a transaction before calling this method.");

        var keyProperty = primaryKey.Properties[0];

        var tableName = entityType.GetTableName()
                        ?? throw new InvalidOperationException($"Entity type {typeof(TEntity).Name} is not mapped to a table.");

        var schema = entityType.GetSchema();

        var storeObject = StoreObjectIdentifier.Table(tableName, schema);
        var keyColumnName = keyProperty.GetColumnName(storeObject)
                            ?? throw new InvalidOperationException($"Could not resolve column name for key property {keyProperty.Name}.");

        var sqlHelper = Context.GetService<ISqlGenerationHelper>();

        var delimitedTable = string.IsNullOrWhiteSpace(schema)
            ? sqlHelper.DelimitIdentifier(tableName)
            : sqlHelper.DelimitIdentifier(tableName, schema);

        var delimitedKeyColumn = sqlHelper.DelimitIdentifier(keyColumnName);

        var providerName = Context.Database.ProviderName ?? string.Empty;

        string sql = providerName switch
        {
            // PostgreSQL / Oracle
            var p when p.Contains("Npgsql", StringComparison.OrdinalIgnoreCase)
                => $"SELECT * FROM {delimitedTable} WHERE {delimitedKeyColumn} = @p0 FOR UPDATE",

            var p when p.Contains("Oracle", StringComparison.OrdinalIgnoreCase)
                => $"SELECT * FROM {delimitedTable} WHERE {delimitedKeyColumn} = @p0 FOR UPDATE",

            // SQL Server
            var p when p.Contains("SqlServer", StringComparison.OrdinalIgnoreCase)
                => $"SELECT * FROM {delimitedTable} WITH (UPDLOCK, ROWLOCK) WHERE {delimitedKeyColumn} = @p0",

            // MySQL / MariaDB
            var p when p.Contains("MySql", StringComparison.OrdinalIgnoreCase)
                => $"SELECT * FROM {delimitedTable} WHERE {delimitedKeyColumn} = @p0 FOR UPDATE",

            var p when p.Contains("MariaDb", StringComparison.OrdinalIgnoreCase)
                => $"SELECT * FROM {delimitedTable} WHERE {delimitedKeyColumn} = @p0 FOR UPDATE",

            _ => throw new NotSupportedException(
                $"Provider '{providerName}' is not supported for GetByIdWithLockAsync.")
        };

        return await DbSet
            .FromSqlRaw(sql, id!)
            .AsTracking()
            .SingleOrDefaultAsync(cancellationToken);
    }

    public virtual async Task<List<TEntity>> GetByIdsWithLockAsync(
        IEnumerable<TKey> ids,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (ids is null)
            throw new ArgumentNullException(nameof(ids));

        var idList = ids
            .Where(x => x is not null)
            .Distinct()
            .ToList();

        if (idList.Count == 0)
            return [];

        var entityType = Context.Model.FindEntityType(typeof(TEntity))
                         ?? throw new InvalidOperationException(
                             $"Entity type {typeof(TEntity).Name} is not mapped.");

        var primaryKey = entityType.FindPrimaryKey()
                         ?? throw new InvalidOperationException(
                             $"Entity type {typeof(TEntity).Name} does not have a primary key.");

        if (primaryKey.Properties.Count != 1)
            throw new NotSupportedException(
                $"GetByIdsWithLockAsync supports only single-column primary keys. " +
                $"{typeof(TEntity).Name} has a composite key.");

        if (Context.Database.CurrentTransaction is null)
            throw new InvalidOperationException(
                "GetByIdsWithLockAsync requires an active transaction. " +
                "Start a transaction before calling this method.");

        var keyProperty = primaryKey.Properties[0];

        var tableName = entityType.GetTableName()
                        ?? throw new InvalidOperationException(
                            $"Entity type {typeof(TEntity).Name} is not mapped to a table.");

        var schema = entityType.GetSchema();

        var storeObject = StoreObjectIdentifier.Table(tableName, schema);

        var keyColumnName = keyProperty.GetColumnName(storeObject)
                            ?? throw new InvalidOperationException(
                                $"Could not resolve column name for key property {keyProperty.Name}.");

        var sqlHelper = Context.GetService<ISqlGenerationHelper>();

        var delimitedTable = string.IsNullOrWhiteSpace(schema)
            ? sqlHelper.DelimitIdentifier(tableName)
            : sqlHelper.DelimitIdentifier(tableName, schema);

        var delimitedKeyColumn = sqlHelper.DelimitIdentifier(keyColumnName);

        // Build: IN ({0}, {1}, {2}, ...)
        var placeholders = string.Join(", ", idList.Select((_, index) => $"{{{index}}}"));

        var providerName = Context.Database.ProviderName ?? string.Empty;

        string sql = providerName switch
        {
            // PostgreSQL / Oracle / MySQL / MariaDB
            var p when p.Contains("Npgsql", StringComparison.OrdinalIgnoreCase)
                => $"SELECT * FROM {delimitedTable} " +
                   $"WHERE {delimitedKeyColumn} IN ({placeholders}) " +
                   $"ORDER BY {delimitedKeyColumn} FOR UPDATE",

            var p when p.Contains("Oracle", StringComparison.OrdinalIgnoreCase)
                => $"SELECT * FROM {delimitedTable} " +
                   $"WHERE {delimitedKeyColumn} IN ({placeholders}) " +
                   $"ORDER BY {delimitedKeyColumn} FOR UPDATE",

            var p when p.Contains("MySql", StringComparison.OrdinalIgnoreCase)
                => $"SELECT * FROM {delimitedTable} " +
                   $"WHERE {delimitedKeyColumn} IN ({placeholders}) " +
                   $"ORDER BY {delimitedKeyColumn} FOR UPDATE",

            var p when p.Contains("MariaDb", StringComparison.OrdinalIgnoreCase)
                => $"SELECT * FROM {delimitedTable} " +
                   $"WHERE {delimitedKeyColumn} IN ({placeholders}) " +
                   $"ORDER BY {delimitedKeyColumn} FOR UPDATE",

            // SQL Server
            var p when p.Contains("SqlServer", StringComparison.OrdinalIgnoreCase)
                => $"SELECT * FROM {delimitedTable} WITH (UPDLOCK, ROWLOCK) " +
                   $"WHERE {delimitedKeyColumn} IN ({placeholders}) " +
                   $"ORDER BY {delimitedKeyColumn}",

            _ => throw new NotSupportedException(
                $"Provider '{providerName}' is not supported for GetByIdsWithLockAsync.")
        };

        return await DbSet
            .FromSqlRaw(sql, idList.Cast<object>().ToArray())
            .AsTracking()
            .ToListAsync(cancellationToken);
    }

    public virtual IEnumerable<TEntity> GetAll()
    {
        return DbSet.AsNoTracking().ToList();
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public virtual IQueryable<TEntity> GetAllQueryable(
        Expression<Func<TEntity, bool>>? predicate = null)
    {
        IQueryable<TEntity> query = DbSet.AsQueryable();

        if (predicate is not null)
        {
            query = query.Where(predicate);
        }

        return query;
    }

    public virtual Task<IQueryable<TEntity>> GetAllQueryableAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IQueryable<TEntity> query = DbSet.AsQueryable();

        if (predicate is not null)
        {
            query = query.Where(predicate);
        }

        return Task.FromResult(query);
    }

    public virtual void Add(TEntity entity)
    {
        DbSet.Add(entity);
    }

    public virtual async Task AddAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
    }

    public virtual void AddRange(IEnumerable<TEntity> entities)
    {
        DbSet.AddRange(entities);
    }

    public virtual async Task AddRangeAsync(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default)
    {
        await DbSet.AddRangeAsync(entities, cancellationToken);
    }

    public virtual void Update(TEntity entity)
    {
        DbSet.Update(entity);
    }
    public virtual void UpdateRange(IEnumerable<TEntity> entities)
    {
        DbSet.UpdateRange(entities);
    }

    public virtual void Remove(TEntity entity)
    {
        DbSet.Remove(entity);
    }

    public void RemoveRange(IEnumerable<TEntity> entities)
    {
        DbSet.RemoveRange(entities);
    }

    protected static IOrderedQueryable<TEntity> ApplyOrdering(
        IQueryable<TEntity> query,
        string? sortBy,
        bool isAscending,
        IReadOnlyDictionary<string, Expression<Func<TEntity, object>>> allowedOrderBy,
        string defaultSortBy)
    {
        if (allowedOrderBy.Count == 0)
        {
            throw new InvalidOperationException(
                $"No ordering fields were configured for entity '{typeof(TEntity).Name}'.");
        }

        var key = string.IsNullOrWhiteSpace(sortBy) ? defaultSortBy : sortBy.Trim();

        if (!allowedOrderBy.TryGetValue(key, out var selector))
        {
            if (!allowedOrderBy.TryGetValue(defaultSortBy, out selector))
            {
                throw new InvalidOperationException(
                    $"Default sort key '{defaultSortBy}' was not configured for entity '{typeof(TEntity).Name}'.");
            }
        }

        return isAscending
            ? query.OrderBy(selector)
            : query.OrderByDescending(selector);
    }
}