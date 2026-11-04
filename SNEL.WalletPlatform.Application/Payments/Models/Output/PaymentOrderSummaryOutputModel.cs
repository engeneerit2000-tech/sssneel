namespace SNEL.WalletPlatform.Application.Payments.Models.Output;

public sealed record PaymentOrderSummaryOutputModel(
    Guid Id,
    string Description,
    int Status,
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