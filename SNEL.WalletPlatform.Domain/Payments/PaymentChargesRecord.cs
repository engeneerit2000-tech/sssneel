namespace SNEL.WalletPlatform.Domain.Payments;

public record PaymentChargesRecord(
    decimal Amount,
    decimal Fees,
    decimal FeesTax,
    decimal Cost,
    decimal CostTax,
    decimal TotalAmount
);