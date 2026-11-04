namespace SNEL.WalletPlatform.Application.Payments.Models.Output;

/// <summary>
/// Payment with its Payment Order (PO) and Payment Order Items (POI).
/// </summary>
public sealed record PaymentOutputModel(
    Guid Id,
    Guid PaymentOrderId,
    decimal Amount,
    decimal? Tax,
    decimal Total,
    int Status,
    string? CurrencyCode,
    string? PaymentMethodCode,
    string? PaymentMethodName,
    string? PaymentProviderCode,
    string? PaymentProviderName,
    string? ExternalRef,
    string? PaymentRef,
    DateTimeOffset CreatedAtUtc);

public sealed record PaymentExcelModel(
    decimal Amount,
    decimal? Tax,
    decimal Total,
    int Status,
    string? CurrencyCode,
    string? PaymentMethodCode,
    string? PaymentMethodName,
    string? PaymentProviderCode,
    string? PaymentProviderName,
    string? ExternalRef,
    string? PaymentRef,
    DateTimeOffset CreatedAtUtc);
