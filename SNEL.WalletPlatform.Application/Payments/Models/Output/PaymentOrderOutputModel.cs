using SNEL.WalletPlatform.Domain.Payments;

namespace SNEL.WalletPlatform.Application.Payments.Models.Output;

public sealed record PaymentOrderOutputModel(
    Guid Id,
    Guid CustomerId,
    int ChannelId,
    string Description,
    PaymentOrderStatus Status,
    string Currency,
    decimal Amount,
    decimal? Fees,
    decimal? Tax,
    decimal Total,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);
