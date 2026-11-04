namespace SNEL.WalletPlatform.Domain.Billing;

public record BillingChargesRecord(
    decimal Amount,
    decimal Fees,
    decimal FeesTax,
    decimal Cost,
    decimal CostTax,
    decimal Commission,
    decimal CommissionTax,
    decimal TotalAmount
    );