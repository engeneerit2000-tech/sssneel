namespace SNEL.WalletPlatform.Application.Payments.Models.Output;

public record PaymentOrderItemSummaryOutputModel(
    Guid Id,
    int ItemType,
    decimal Amount,
    /// <summary>Direction-aware fees: IN → <see cref="TotalFees"/>; OUT → <see cref="TotalCost"/>.</summary>
    decimal Fees,
    /// <summary>Direction-aware tax: IN → <see cref="TotalFeesTax"/>; OUT → <see cref="TotalCostTax"/>.</summary>
    decimal Tax,
    decimal TotalFees,
    decimal TotalFeesTax,
    decimal TotalCost,
    decimal TotalCostTax,
    decimal TotalCommission,
    decimal TotalCommissionTax,
    decimal TotalAmount,
    int State,
    string? BillingRefValue,
    string? BillingRefForDisplay,
    string? BillingServiceName,
    string? BillingServiceCode,
    string ExternalRef,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? InvoiceDate,
    string? InvoiceRef
    );