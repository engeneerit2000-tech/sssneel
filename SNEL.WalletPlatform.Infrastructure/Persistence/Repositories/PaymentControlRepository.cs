using Microsoft.EntityFrameworkCore;
using SNEL.WalletPlatform.Application.BillingServices.Interfaces;
using SNEL.WalletPlatform.Domain.Billing;
using SNEL.WalletPlatform.Infrastructure.Persistence.Repositories.Base;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Repositories;

public sealed class PaymentControlRepository(ApplicationDbContext context)
    : EntityRepository<PaymentControlEntity, Guid>(context), IPaymentControlRepository
{
    public Task<List<PaymentControlEntity>> GetByBillingServiceAndCurrencyAndBillingRefAsync(
        long billingServiceId,
        int currencyId,
        string billingRef,
        CancellationToken cancellationToken = default)
    {
        return Context.PaymentControls
            .Where(x =>
            x.BillingServiceId == billingServiceId &&
            x.CurrencyId == currencyId &&
            x.BillingRef == billingRef
            )
            .ToListAsync(cancellationToken);
    }
}
