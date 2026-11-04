namespace SNEL.WalletPlatform.Application.Payments.Models.Output;

/// <summary>
/// Payment with its Payment Order (PO) and Payment Order Items (POI).
/// </summary>
public sealed record PaymentWithOrderAndItemsOutputModel(
    Guid Id,
    Guid PaymentOrderId,
    string Description,
    decimal Amount,
    decimal? Tax,
    decimal Total,
    int Status,
    string? CurrencyCode,
    string? PaymentMethodCode,
    string? PaymentMethodName,
    string? ExternalRef,
    string? PaymentRef,
    bool CanComplete,
    bool CanApply,
    bool CanCancel,
    bool CanReconcile,
    DateTimeOffset CreatedAtUtc,
    PaymentOrderSummaryOutputModel? PaymentOrder,
    IReadOnlyList<PaymentOrderItemSummaryOutputModel> Items);

public sealed record TopupExcelOutputModel(
    string Description,
    string Status,
    string? PaymentProviderCode,
    string? PaymentProviderName,
    string? PaymentMethodCode,
    string? PaymentMethodName,
    string? ExternalRef,
    string? PaymentRef,
    decimal Amount,
    decimal TotalFees,
    decimal TotalFeesTax,
    decimal TotalCost,
    decimal TotalCostTax,
    decimal TotalCommission,
    decimal TotalCommissionTax,
    decimal TotalAmount,
    string? CurrencyCode,
    DateTimeOffset CreatedAtUtc);

public sealed record PaymentExcelOutputModel(
    string Description,
    string Status,
    string? ExternalRef,
    string? PaymentRef,
    decimal Amount,
    decimal TotalFees,
    decimal TotalFeesTax,
    decimal TotalCost,
    decimal TotalCostTax,
    decimal TotalCommission,
    decimal TotalCommissionTax,
    decimal TotalAmount,
    string? CurrencyCode,
    DateTimeOffset CreatedAtUtc);
