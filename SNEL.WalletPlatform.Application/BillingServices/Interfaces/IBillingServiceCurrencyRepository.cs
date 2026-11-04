using SNEL.WalletPlatform.Application.Interfaces.Repositories.Base;
using SNEL.WalletPlatform.Domain.Billing;

namespace SNEL.WalletPlatform.Application.BillingServices.Interfaces;

public interface IBillingServiceCurrencyRepository : IEntityRepository<BillingServiceCurrencyEntity,long>
{
    Task<BillingServiceCurrencyEntity?> GetByIdWithBillingServiceAsync(long id);
    Task<bool> ExistsByBillingServiceAndCurrencyAsync(long billingServiceId, int currencyId);
    Task<List<BillingServiceCurrencyEntity>> GetActiveByBillingServiceAsync(long billingServiceId);
    Task<BillingServiceCurrencyEntity?> GetByBillingServiceAndCurrencyAsync(long billingServiceId, int currencyId);
}
