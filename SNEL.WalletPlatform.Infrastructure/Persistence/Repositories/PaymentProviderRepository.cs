using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SNEL.WalletPlatform.Application.Payments.Interfaces;
using SNEL.WalletPlatform.Domain.Payments;
using SNEL.WalletPlatform.Infrastructure.Persistence.Repositories.Base;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Repositories;

public sealed class PaymentProviderRepository(ApplicationDbContext context)
    : EntityRepository<PaymentProviderEntity, long>(context), IPaymentProviderRepository
{
    public async Task<IReadOnlyCollection<PaymentProviderEntity>> GetAllScopedByPartnerAsync(
        long? partnerId,
        CancellationToken cancellationToken = default)
    {
        var query = context.PaymentProviders
            .AsNoTracking()
            .Include(x => x.Partner)
            .AsQueryable();

        if (partnerId.HasValue)
        {
            query = query.Where(x => x.PartnerId == partnerId.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public Task<PaymentProviderEntity> GetByPartnerId(long partnerId, CancellationToken cancellationToken = default)
    {
        return Context.Set<PaymentProviderEntity>().FirstOrDefaultAsync(x => x.PartnerId == partnerId, cancellationToken)!;
    }
    public Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalized = code.Trim().ToUpper();
        return Context.Set<PaymentProviderEntity>().AnyAsync(x => x.Code == normalized, cancellationToken);
    }

    public Task<bool> ExistsByCodeExceptIdAsync(string code, long paymentProviderId, CancellationToken cancellationToken = default)
    {
        var normalized = code.Trim().ToUpper();
        return Context.Set<PaymentProviderEntity>().AnyAsync(
            x => x.Id != paymentProviderId && x.Code == normalized,
            cancellationToken);
    }

    public Task<bool> ExistsByPartnerIdAsync(long partnerId, CancellationToken cancellationToken = default)
    {
        return Context.Set<PaymentProviderEntity>().AnyAsync(x => x.PartnerId == partnerId, cancellationToken);
    }

    public override PaymentProviderEntity? GetById(long id)
    {
        return DbSet.Include(x => x.Partner).FirstOrDefault(x => x.Id == id);
    }

    public override async Task<PaymentProviderEntity?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await DbSet.Include(x => x.Partner).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public override async Task<List<PaymentProviderEntity>> GetByIdsAsync(
        IReadOnlyCollection<long> ids,
        CancellationToken cancellationToken = default)
    {
        if (ids is null)
            throw new ArgumentNullException(nameof(ids));

        if (ids.Count == 0)
            return new List<PaymentProviderEntity>();

        return await DbSet
            .Include(x => x.Partner)
            .Where(e => ids.Contains(e.Id))
            .ToListAsync(cancellationToken);
    }

    public override async Task<PaymentProviderEntity?> GetByIdWithLockAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await base.GetByIdWithLockAsync(id, cancellationToken);
        if (entity is not null)
            await Context.Entry(entity).Reference(e => e.Partner).LoadAsync(cancellationToken);
        return entity;
    }

    public override async Task<List<PaymentProviderEntity>> GetByIdsWithLockAsync(
        IEnumerable<long> ids,
        CancellationToken cancellationToken = default)
    {
        var list = await base.GetByIdsWithLockAsync(ids, cancellationToken);
        foreach (var entity in list)
            await Context.Entry(entity).Reference(e => e.Partner).LoadAsync(cancellationToken);
        return list;
    }

    public override IEnumerable<PaymentProviderEntity> GetAll()
    {
        return DbSet.AsNoTracking().Include(x => x.Partner).ToList();
    }

    public override async Task<IEnumerable<PaymentProviderEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking().Include(x => x.Partner).ToListAsync(cancellationToken);
    }

    public override IQueryable<PaymentProviderEntity> GetAllQueryable(Expression<Func<PaymentProviderEntity, bool>>? predicate = null)
    {
        IQueryable<PaymentProviderEntity> query = DbSet.Include(x => x.Partner);
        if (predicate is not null)
            query = query.Where(predicate);
        return query;
    }

    public override Task<IQueryable<PaymentProviderEntity>> GetAllQueryableAsync(
        Expression<Func<PaymentProviderEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IQueryable<PaymentProviderEntity> query = DbSet.Include(x => x.Partner);
        if (predicate is not null)
            query = query.Where(predicate);
        return Task.FromResult(query);
    }
}
