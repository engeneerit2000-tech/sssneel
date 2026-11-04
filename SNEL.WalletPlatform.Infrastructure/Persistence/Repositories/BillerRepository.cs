using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SNEL.WalletPlatform.Application.Billers.Interfaces;
using SNEL.WalletPlatform.Domain.Billing;
using SNEL.WalletPlatform.Infrastructure.Persistence.Repositories.Base;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Repositories;

public sealed class BillerRepository(ApplicationDbContext context)
    : EntityRepository<BillerEntity, long>(context), IBillerRepository
{
    public async Task<IReadOnlyCollection<BillerEntity>> GetAllScopedByPartnerAsync(
        long? partnerId,
        CancellationToken cancellationToken = default)
    {
        var query = context.Billers
            .AsNoTracking()
            .Include(x => x.Partner)
            .AsQueryable();

        if (partnerId.HasValue)
        {
            query = query.Where(x => x.PartnerId == partnerId.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public Task<BillerEntity?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalized = code.Trim().ToLower();
        return context.Billers
            .Include(x => x.Partner)
            .FirstOrDefaultAsync(x => x.Code.ToLower() == normalized, cancellationToken);
    }

    public Task<bool> ExistsByCodeAsync(string code)
    {
        var normalized = code.Trim().ToLower();
        return context.Billers.AnyAsync(x => x.Code.ToLower() == normalized);
    }

    public Task<bool> ExistsByCodeExceptIdAsync(string code, long billerId, CancellationToken cancellationToken = default)
    {
        var normalized = code.Trim().ToLower();
        return context.Billers.AnyAsync(
            x => x.Id != billerId && x.Code.ToLower() == normalized,
            cancellationToken);
    }

    public override BillerEntity? GetById(long id)
    {
        return DbSet.Include(x => x.Partner).FirstOrDefault(x => x.Id == id);
    }

    public override async Task<BillerEntity?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await DbSet.Include(x => x.Partner).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public override async Task<List<BillerEntity>> GetByIdsAsync(
        IReadOnlyCollection<long> ids,
        CancellationToken cancellationToken = default)
    {
        if (ids is null)
            throw new ArgumentNullException(nameof(ids));

        if (ids.Count == 0)
            return new List<BillerEntity>();

        return await DbSet
            .Include(x => x.Partner)
            .Where(e => ids.Contains(e.Id))
            .ToListAsync(cancellationToken);
    }

    public override async Task<BillerEntity?> GetByIdWithLockAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await base.GetByIdWithLockAsync(id, cancellationToken);
        if (entity is not null)
            await Context.Entry(entity).Reference(e => e.Partner).LoadAsync(cancellationToken);
        return entity;
    }

    public override async Task<List<BillerEntity>> GetByIdsWithLockAsync(
        IEnumerable<long> ids,
        CancellationToken cancellationToken = default)
    {
        var list = await base.GetByIdsWithLockAsync(ids, cancellationToken);
        foreach (var entity in list)
            await Context.Entry(entity).Reference(e => e.Partner).LoadAsync(cancellationToken);
        return list;
    }

    public override IEnumerable<BillerEntity> GetAll()
    {
        return DbSet.AsNoTracking().Include(x => x.Partner).ToList();
    }

    public override async Task<IEnumerable<BillerEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking().Include(x => x.Partner).ToListAsync(cancellationToken);
    }

    public override IQueryable<BillerEntity> GetAllQueryable(Expression<Func<BillerEntity, bool>>? predicate = null)
    {
        IQueryable<BillerEntity> query = DbSet.Include(x => x.Partner);
        if (predicate is not null)
            query = query.Where(predicate);
        return query;
    }

    public override Task<IQueryable<BillerEntity>> GetAllQueryableAsync(
        Expression<Func<BillerEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IQueryable<BillerEntity> query = DbSet.Include(x => x.Partner);
        if (predicate is not null)
            query = query.Where(predicate);
        return Task.FromResult(query);
    }
}
