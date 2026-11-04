using Microsoft.EntityFrameworkCore;
using SNEL.WalletPlatform.Application.BillingServices.Interfaces;
using SNEL.WalletPlatform.Domain.Billing;
using SNEL.WalletPlatform.Infrastructure.Persistence.Repositories.Base;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Repositories;

public sealed class BillingServiceCurrencyRepository(ApplicationDbContext context)
    : EntityRepository<BillingServiceCurrencyEntity,long>(context), IBillingServiceCurrencyRepository
{
    public Task<BillingServiceCurrencyEntity?> GetByIdWithBillingServiceAsync(long id)
    {
        return context.Set<BillingServiceCurrencyEntity>()
            .Include(x => x.BillingService)
                .ThenInclude(bs => bs.Biller)
                .ThenInclude(b => b.Partner)
            .Include(x => x.BillingService)
                .ThenInclude(bs => bs.Translations)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public Task<bool> ExistsByBillingServiceAndCurrencyAsync(long billingServiceId, int currencyId)
    {
        return context.Set<BillingServiceCurrencyEntity>()
            .AnyAsync(x => x.BillingServiceId == billingServiceId && x.CurrencyId == currencyId);
    }

    public Task<List<BillingServiceCurrencyEntity>> GetActiveByBillingServiceAsync(long billingServiceId)
    {
        return context.Set<BillingServiceCurrencyEntity>()
            .Where(x => x.BillingServiceId == billingServiceId && x.Active)
            .ToListAsync();
    }
    public Task<BillingServiceCurrencyEntity?> GetByBillingServiceAndCurrencyAsync(long billingServiceId, int currencyId)
    {
        return context.Set<BillingServiceCurrencyEntity>()
            .FirstOrDefaultAsync(x => x.BillingServiceId == billingServiceId && x.CurrencyId == currencyId);
    }
}
