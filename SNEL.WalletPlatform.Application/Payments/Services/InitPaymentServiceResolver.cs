using SNEL.WalletPlatform.Application.Payments.Interfaces;
using SNEL.WalletPlatform.Domain.Payments;

namespace SNEL.WalletPlatform.Application.Payments.Services;

public sealed class InitPaymentServiceResolver(IEnumerable<IInitPaymentService> services)
    : IInitPaymentServiceResolver
{
    public IInitPaymentService? Resolve(PaymentInitModeEnum paymentInitMode)
    {
        return services.FirstOrDefault(x => x.CanHandle(paymentInitMode));
    }
}
