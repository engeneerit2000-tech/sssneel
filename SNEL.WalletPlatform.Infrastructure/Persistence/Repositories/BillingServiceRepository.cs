using Microsoft.EntityFrameworkCore;
using SNEL.WalletPlatform.Application.BillingServices.Interfaces;
using SNEL.WalletPlatform.Domain.Billing;
using SNEL.WalletPlatform.Infrastructure.Persistence.Repositories.Base;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Repositories;

public sealed class BillingServiceRepository(ApplicationDbContext context)
    : EntityRepository<BillingServiceEntity, long>(context), IBillingServiceRepository
{
    public Task<BillingServiceEntity?> GetByIdWithRelationsAsync(long id)
    {
        return context.BillingServices
            .Include(x => x.Biller)
            .ThenInclude(b => b.Partner)
            .Include(x => x.BillingCategory)
            .Include(x => x.ServiceType)
            .Include(x=>x.Currencies)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<IReadOnlyCollection<BillingServiceEntity>> GetAllWithRelationsAsync(CancellationToken cancellationToken = default)
    {
        return await context.BillingServices
            .AsNoTracking()
            .Include(x => x.Biller)
            .ThenInclude(b => b.Partner)
            .Include(x => x.BillingCategory)
            .Include(x => x.ServiceType)
            .Include(x => x.Currencies)
            .Include(x => x.Translations)
            .OrderBy(x => x.Biller.Code)
            .ThenBy(x => x.Code)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<BillingServiceEntity>> GetAllWithRelationsScopedByBillerPartnerAsync(
        long? billerPartnerId,
        bool? isActive = true,
        CancellationToken cancellationToken = default)
    {
        var query = context.BillingServices
            .AsNoTracking()
            .Include(x => x.Biller)
            .ThenInclude(b => b.Partner)
            .Include(x => x.BillingCategory)
            .Include(x => x.ServiceType)
            .Include(x => x.Currencies)
            .Include(x => x.Translations)
            .AsQueryable();

        if (billerPartnerId.HasValue)
        {
            query = query.Where(x => x.Biller.PartnerId == billerPartnerId.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(x => x.Active == isActive.Value);
        }

        return await query
            .OrderBy(x => x.Biller.Code)
            .ThenBy(x => x.Code)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsByCodeAsync(long billerId, string code)
    {
        var normalized = code.Trim().ToLower();

        return context.BillingServices.AnyAsync(x =>
            x.BillerId == billerId &&
            x.Code.ToLower() == normalized);
    }

    public Task<bool> ExistsByCodeExceptIdAsync(long billerId, string code, long id)
    {
        var normalized = code.Trim().ToLower();

        return context.BillingServices.AnyAsync(x =>
            x.BillerId == billerId &&
            x.Id != id &&
            x.Code.ToLower() == normalized);
    }

    public Task<bool> HasInvoicesAsync(long billingServiceId, CancellationToken cancellationToken = default)
    {
        return context.Invoices.AnyAsync(x => x.BillingServiceId == billingServiceId, cancellationToken);
    }

    public Task<bool> HasCustomerSubscriptionsAsync(long billingServiceId, CancellationToken cancellationToken = default)
    {
        return context.CustomerSubscriptions.AnyAsync(x => x.BillingServiceId == billingServiceId, cancellationToken);
    }

    public Task<BillingServiceEntity?> GetByBillerCodeAndBillingServiceCodeWithCurrenciesAsync(string billerCode, string billingServiceCode)
    {
        var normalizedBillerCode = billerCode.Trim().ToLower();
        var normalizedServiceCode = billingServiceCode.Trim().ToLower();

        return context.BillingServices
            .Include(x => x.Biller)
            .ThenInclude(x => x.Partner)
            .Include(x => x.ServiceType)
            .Include(x => x.Currencies)
            .FirstOrDefaultAsync(x =>
                x.Biller.Code.ToLower() == normalizedBillerCode &&
                x.Code.ToLower() == normalizedServiceCode);
    }
}
