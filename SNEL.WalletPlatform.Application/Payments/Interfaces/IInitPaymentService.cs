using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Payments.Models.Internal;
using SNEL.WalletPlatform.Application.Payments.Models.Output;
using SNEL.WalletPlatform.Domain.Payments;

namespace SNEL.WalletPlatform.Application.Payments.Interfaces;

public interface IInitPaymentService
{
    bool CanHandle(PaymentInitModeEnum paymentInitMode);

    Task<Result<PaymentNextActionOutputModel>> InitPayment(
        PaymentEntity payment,
        PspConnector connector);
}
