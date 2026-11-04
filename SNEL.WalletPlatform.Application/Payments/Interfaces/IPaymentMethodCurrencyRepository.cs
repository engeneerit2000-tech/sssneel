using SNEL.WalletPlatform.Application.Interfaces.Repositories.Base;
using SNEL.WalletPlatform.Domain.Payments;

namespace SNEL.WalletPlatform.Application.Payments.Interfaces;

public interface IPaymentMethodCurrencyRepository : IEntityRepository<PaymentMethodCurrencyEntity,long>
{
    Task<PaymentMethodCurrencyEntity?> GetByPaymentMethodAndCurrencyAsync(long paymentMethodId, int currencyId);
}