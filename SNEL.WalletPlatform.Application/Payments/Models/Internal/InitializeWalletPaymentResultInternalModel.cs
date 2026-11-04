

using SNEL.WalletPlatform.Domain.Payments;

namespace SNEL.WalletPlatform.Application.Payments.Models.Internal;

public sealed record ExecuteWalletPaymentResultInternalModel (PaymentEntity Payment, PaymentOrderEntity PaymentOrder);
