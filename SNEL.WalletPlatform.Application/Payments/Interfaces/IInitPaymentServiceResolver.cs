using SNEL.WalletPlatform.Domain.Payments;

namespace SNEL.WalletPlatform.Application.Payments.Interfaces;

public interface IInitPaymentServiceResolver
{
    IInitPaymentService? Resolve(PaymentInitModeEnum paymentInitMode);
}
