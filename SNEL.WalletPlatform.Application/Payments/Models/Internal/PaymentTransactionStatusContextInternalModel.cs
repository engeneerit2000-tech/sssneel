using SNEL.WalletPlatform.Domain.Payments;

namespace SNEL.WalletPlatform.Application.Payments.Models.Internal;

public sealed record PaymentTransactionStatusContextInternalModel(
    Guid PaymentId,
    int CurrencyId,
    string PaymentTransactionReference,
    string PaymentExternalReference,
    PaymentMethodEntity PaymentMethod,
    decimal Amount,
    int? StatusPollingAttempt = null,
    int? StatusPollingTotalAttempts = null,
    int? NotFoundFailAfterAttempts = null);



