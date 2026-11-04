using SNEL.WalletPlatform.Domain.Payments;

namespace SNEL.WalletPlatform.Application.Payments.Models.Output;

public sealed record PaymentOrderItemOutputModel(
    Guid Id,
    Guid PaymentOrderId,
    int ChannelId,
    OrderItemType ItemType,
    long? BillingServiceId,
    Guid? CustomerSubscriptionId,
    string? BillingRefValue,
    string? BillingRefForDisplay,
    Guid? InvoiceId,
    Guid WalletAccountId,
    decimal Amount,
    decimal? Fees,
    decimal? Tax,
    decimal Total,
    PaymentOrderItemStatus Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    PaymentOrderOutputModel PaymentOrder);

public sealed record PaymentOrderItemExcelModel(
    int ChannelId,
    OrderItemType ItemType,
    long? BillingServiceId,
    string? BillingRefValue,
    string? BillingRefForDisplay,
    decimal Amount,
    decimal? Fees,
    decimal? Tax,
    decimal Total,
    PaymentOrderItemStatus Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    string Description);
