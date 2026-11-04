using SNEL.WalletPlatform.Application.Interfaces.Repositories.Base;
using SNEL.WalletPlatform.Domain.Payments;

namespace SNEL.WalletPlatform.Application.Payments.Interfaces;

public interface IPaymentProviderRouterRepository : IBaseRepository<PaymentProviderRouterEntity,int>
{
    Task<PaymentProviderRouterEntity> GetByPrefix(string countryCode, string prefix, CancellationToken cancellationToken = default);
    Task<bool> IsPhoneMatchingProvider(string countryCode, string prefix, string paymentProviderCode, CancellationToken cancellationToken = default);
}
