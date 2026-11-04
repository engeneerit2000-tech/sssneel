using SNEL.WalletPlatform.Domain.Payments;

namespace SNEL.WalletPlatform.Application.Payments.Models.Output;

public sealed record PaymentTxnOutputModel(
    Guid Id,
    Guid PaymentId,
    TransactionType Type,
    TransactionStatus Status,
    int CurrencyId,
    decimal Amount,
    string? ExternalRef,
    string? ReqBody,
    string? ResBody,
    string? ResCode,
    string? ResMessage,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);
