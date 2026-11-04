using SNEL.WalletPlatform.Domain.Common;

namespace SNEL.WalletPlatform.Domain.Billing;

/// <summary>
/// Staging table for raw Oracle pull-invoices query result (GCO_ENCOURS_FACTURES + payments).
/// </summary>
public sealed class InvoiceStagingEntity : TrackedEntity<Guid>
{
    private InvoiceStagingEntity() { }

    private static DateTime? AsUtc(DateTime? value)
    {
        if (!value.HasValue) return null;
        var dt = value.Value;
        return dt.Kind == DateTimeKind.Utc ? dt : DateTime.SpecifyKind(dt, DateTimeKind.Utc);
    }

    public long? OriginInvoiceId { get; private set; }
    public string? JobKey { get; private set; }
    public string? SourceSystem { get; private set; }
    public string? InvoiceNum { get; private set; }
    public DateTime? InvoiceDate { get; private set; }
    public DateTime? InvoiceUpdateDate { get; private set; }
    public string? RegionCode { get; private set; }
    public string? DistrictCode { get; private set; }
    public decimal? CustomerId { get; private set; }
    public string? CustomerNum { get; private set; }
    public string? CustomerName { get; private set; }
    public string? CustomerTypeCode { get; private set; }
    public string? CustomerTypeName { get; private set; }
    public string? TariffType { get; private set; }
    public decimal? BillingPeriod { get; private set; }
    public decimal? Consumption { get; private set; }
    public decimal? BilledConsumption { get; private set; }
    public string? InvoiceCurrencyCode { get; private set; }
    public decimal? AmountUsd { get; private set; }
    public decimal? AmountFc { get; private set; }
    public decimal? PaidUsd { get; private set; }
    public decimal? PaidFc { get; private set; }
    public DateTime? PaymentUpdateDate { get; private set; }
    public decimal? Vat { get; private set; }
    public decimal? ProvincialTax { get; private set; }
    public string? CenterDesignation { get; private set; }
    public string? Pa { get; private set; }
    public string? PointOfSaleName { get; private set; }
    public string? SubstationCode { get; private set; }
    public string? ClientPropertyNumber { get; private set; }
    public string? ClientStreet { get; private set; }
    public string? ClientCommune { get; private set; }
    public decimal? MeterNumber { get; private set; }
    public decimal? CalculationCoefficient { get; private set; }
    public decimal? PreviousBillingIndex { get; private set; }
    public decimal? CurrentBillingIndex { get; private set; }
    public decimal? DiscountCode { get; private set; }
    public decimal? AmountExcludingTaxUsd { get; private set; }
    public decimal? AmountExcludingTaxFc { get; private set; }
    public decimal? SubtotalUsd { get; private set; }
    public decimal? SubtotalFc { get; private set; }
    public int? BillingYear { get; private set; }
    public int? BillingMonth { get; private set; }
    public DateTime? EffectiveInvoiceUpdateDate => new[] { InvoiceUpdateDate, PaymentUpdateDate }.Max();

    public static InvoiceStagingEntity FromOracleRow(
        string jobKey,
        string sourceSystem,
        long? originInvoiceId,
        string? invoiceNum,
        DateTime? invoiceDate,
        DateTime? invoiceUpdateDate,
        string? regionCode,
        string? districtCode,
        decimal? customerId,
        string? customerNum,
        string? customerName,
        string? customerTypeCode,
        string? customerTypeName,
        string? tariffType,
        decimal? billingPeriod,
        decimal? consumption,
        decimal? billedConsumption,
        string? invoiceCurrencyCode,
        decimal? amountUsd,
        decimal? amountFc,
        decimal? paidUsd,
        decimal? paidFc,
        DateTime? paymentUpdateDate,
        decimal? vat,
        decimal? provincialTax,
        string? centerDesignation,
        string? pa,
        string? pointOfSaleName,
        string? substationCode,
        string? clientPropertyNumber,
        string? clientStreet,
        string? clientCommune,
        decimal? meterNumber,
        decimal? calculationCoefficient,
        decimal? previousBillingIndex,
        decimal? currentBillingIndex,
        decimal? discountCode,
        decimal? amountExcludingTaxUsd,
        decimal? amountExcludingTaxFc,
        decimal? subtotalUsd,
        decimal? subtotalFc,
        int? billingYear,
        int? billingMonth
        )
    {
        if (string.IsNullOrWhiteSpace(jobKey))
            throw new ArgumentException("jobKey must be non-empty.", nameof(jobKey));
        if (string.IsNullOrWhiteSpace(sourceSystem))
            throw new ArgumentException("sourceSystem must be non-empty.", nameof(sourceSystem));

        return new InvoiceStagingEntity
        {
            OriginInvoiceId = originInvoiceId,
            JobKey = jobKey.Trim(),
            SourceSystem = sourceSystem.Trim(),
            InvoiceNum = invoiceNum?.Trim(),
            InvoiceDate = AsUtc(invoiceDate),
            InvoiceUpdateDate = AsUtc(invoiceUpdateDate),
            RegionCode = regionCode,
            DistrictCode = districtCode,
            CustomerId = customerId,
            CustomerNum = customerNum?.Trim(),
            CustomerName = customerName?.Trim(),
            CustomerTypeCode = customerTypeCode?.Trim(),
            CustomerTypeName = customerTypeName?.Trim(),
            TariffType = tariffType?.Trim(),
            BillingPeriod = billingPeriod,
            Consumption = consumption,
            BilledConsumption = billedConsumption,
            InvoiceCurrencyCode = invoiceCurrencyCode?.Trim(),
            AmountUsd = amountUsd,
            AmountFc = amountFc,
            PaidUsd = paidUsd,
            PaidFc = paidFc,
            PaymentUpdateDate = AsUtc(paymentUpdateDate),
            Vat = vat,
            ProvincialTax = provincialTax,
            CenterDesignation = centerDesignation?.Trim(),
            Pa = pa?.Trim(),
            PointOfSaleName = pointOfSaleName?.Trim(),
            SubstationCode = substationCode?.Trim(),
            ClientPropertyNumber = clientPropertyNumber?.Trim(),
            ClientStreet = clientStreet?.Trim(),
            ClientCommune = clientCommune?.Trim(),
            MeterNumber = meterNumber,
            CalculationCoefficient = calculationCoefficient,
            PreviousBillingIndex = previousBillingIndex,
            CurrentBillingIndex = currentBillingIndex,
            DiscountCode = discountCode,
            AmountExcludingTaxUsd = amountExcludingTaxUsd,
            AmountExcludingTaxFc = amountExcludingTaxFc,
            SubtotalUsd = subtotalUsd,
            SubtotalFc = subtotalFc,
            BillingYear = billingYear,
            BillingMonth = billingMonth,
        };
    }
}
