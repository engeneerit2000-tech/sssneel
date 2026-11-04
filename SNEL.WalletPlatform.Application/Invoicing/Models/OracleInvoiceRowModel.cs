namespace SNEL.WalletPlatform.Application.Invoicing.Models;

/// <summary>
/// Application-layer model of one row from the Oracle pull-invoices query (Oracle table representation).
/// </summary>
public sealed class OracleInvoiceRowModel
{
    public long? InvoiceId { get; init; }
    public string? InvoiceNum { get; init; }
    public DateTime? InvoiceDate { get; init; }
    public DateTime? InvoiceUpdateDate { get; init; }
    public string? RegionCode { get; init; }
    public string? DistrictCode { get; init; }
    public decimal? CustomerId { get; init; }
    public string? CustomerNum { get; init; }
    public string? CustomerName { get; init; }
    public string? CustomerTypeCode { get; init; }
    public string? CustomerTypeName { get; init; }
    public string? TariffType { get; init; }
    public decimal? BillingPeriod { get; init; }
    public decimal? Consumption { get; init; }
    public decimal? BilledConsumption { get; init; }
    public string? InvoiceCurrencyCode { get; init; }
    public decimal? AmountUsd { get; init; }
    public decimal? AmountFc { get; init; }
    public decimal? PaidUsd { get; init; }
    public decimal? PaidFc { get; init; }
    public DateTime? PaymentUpdateDate { get; init; }
    public decimal? Vat { get; init; }
    public decimal? ProvincialTax { get; init; }
    public string? CenterDesignation { get; init; }
    public string? Pa { get; init; }
    public string? PointOfSaleName { get; init; }
    public string? SubstationCode { get; init; }
    public string? ClientPropertyNumber { get; init; }
    public string? ClientStreet { get; init; }
    public string? ClientCommune { get; init; }
    public decimal? MeterNumber { get; init; }
    public decimal? CalculationCoefficient { get; init; }
    public decimal? PreviousBillingIndex { get; init; }
    public decimal? CurrentBillingIndex { get; init; }
    public decimal? DiscountCode { get; init; }
    public decimal? AmountExcludingTaxUsd { get; init; }
    public decimal? AmountExcludingTaxFc { get; init; }
    public decimal? SubtotalUsd { get; init; }
    public decimal? SubtotalFc { get; init; }
    public decimal? TaxAmountUsd => (Vat ?? 0) + (ProvincialTax ?? 0);
    public decimal? TaxAmountFc => (Vat ?? 0) + (ProvincialTax ?? 0);
    public decimal? TotalAmountUsd => AmountUsd;
    public decimal? TotalAmountFc => AmountFc;
    public int? BillingYear { get; init; }
    public int? BillingMonth { get; init; }
    public DateTime? EffectiveInvoiceUpdateDate => new[] { InvoiceUpdateDate, PaymentUpdateDate }.Max();

    public string ToInvoiceInfoJson() => InvoiceInfoModel.FromOracleRow(this).ToJson();
}
