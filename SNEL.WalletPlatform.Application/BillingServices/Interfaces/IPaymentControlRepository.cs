using SNEL.WalletPlatform.Application.Interfaces.Repositories.Base;
using SNEL.WalletPlatform.Domain.Billing;

namespace SNEL.WalletPlatform.Application.BillingServices.Interfaces;

public interface IPaymentControlRepository : IEntityRepository<PaymentControlEntity, Guid>
{
    Task<List<PaymentControlEntity>> GetByBillingServiceAndCurrencyAndBillingRefAsync(
        long billingServiceId,
        int currencyId,
        string billingRef,
        CancellationToken cancellationToken = default);
}
