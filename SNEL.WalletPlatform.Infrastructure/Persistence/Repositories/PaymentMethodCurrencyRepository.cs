using Microsoft.EntityFrameworkCore;
using SNEL.WalletPlatform.Application.Payments.Interfaces;
using SNEL.WalletPlatform.Domain.Payments;
using SNEL.WalletPlatform.Infrastructure.Persistence.Repositories.Base;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Repositories;

public sealed class PaymentMethodCurrencyRepository(ApplicationDbContext context)
    : EntityRepository<PaymentMethodCurrencyEntity,long>(context), IPaymentMethodCurrencyRepository
{
    public Task<PaymentMethodCurrencyEntity?> GetByPaymentMethodAndCurrencyAsync(long paymentMethodId, int currencyId)
    {
        return context.Set<PaymentMethodCurrencyEntity>()
            .FirstOrDefaultAsync(
                x => 
                    x.PaymentMethodId == paymentMethodId && 
                    x.CurrencyId == currencyId
                    );
    }
}
