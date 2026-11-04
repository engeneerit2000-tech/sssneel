namespace SNEL.WalletPlatform.Application.Payments.Models.Output;

public sealed class PaymentPrintModel
{
    public Guid PaymentId { get; init; }
    public string PaymentRef { get; init; } = default!;
    public string ReceiptDate { get; init; } = default!;
    public string ReceiptTime { get; init; } = default!;
    public string FooterWarningFr { get; init; } = "le vol d'électricité est un délit grave";
    public string FooterWarningEn { get; init; } = "Electricity theft is a serious crime";
    public IReadOnlyList<PaymentSnelTicketModel> Tickets { get; init; } = [];
    public string? AcceptLanguage { get; init; }
    public string? ExternalReference { get; init; }

    /// <summary>
    /// When <see cref="ExternalReference"/> is a JSON object (e.g. PSP metadata), printable lines with friendly labels; otherwise one line equals the raw value.
    /// </summary>
    public IReadOnlyList<string> ExternalReferencePrintLines { get; init; } = [];

    /// <summary>
    /// True when <see cref="ExternalReference"/> was successfully parsed as a JSON object for printing (receipt omits the grouped "external reference" label).
    /// </summary>
    public bool ExternalReferenceIsJsonObject { get; init; }
    public decimal? Tax { get; init; }
    public string? PspReference { get; init; }
    public bool ShowTaxAndFees { get; init; } = true;
    public DateTime CreatedAtUtc { get; init; }

    public string Status { get; init; } = default!;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = default!;

    public string? PaymentMethodCode { get; init; }
    public string? PaymentProviderCode { get; init; }
    public string? PaymentProviderName { get; init; }

    public IReadOnlyList<PaymentPrintItemModel> Items { get; init; } = [];

    public bool ShowWalletHeader { get; init; } = true;
}

public sealed class PaymentPrintItemModel
{
    /// <summary>
    /// Item type (Topup, PayBill, MeterCharge).
    /// </summary>
    public string ItemType { get; init; } = default!;

    public decimal Amount { get; init; }

    /// <summary>
    /// Direction-aware fees (IN: total fees charged; OUT: total cost component), aligned with payment receipt logic.
    /// </summary>
    public decimal Fees { get; init; }

    /// <summary>
    /// Direction-aware tax (IN: fees tax; OUT: cost tax).
    /// </summary>
    public decimal Tax { get; init; }

    /// <summary>
    /// Line total (amount plus fees/taxes per item entity).
    /// </summary>
    public decimal Total { get; init; }

    /// <summary>
    /// Unified reference for the item:
    ///  - PayBill: invoice reference
    ///  - MeterCharge: meter number (billing ref value)
    ///  - Topup: null (top-ups are identified by payment ref)
    /// </summary>
    public string? ItemRef { get; init; }

    /// <summary>
    /// Unified item date:
    ///  - PayBill: invoice issue date
    ///  - MeterCharge / Topup: payment order item creation date
    /// </summary>
    public DateTime ItemDateUtc { get; init; }

    public string? BillingServiceCode { get; init; }
    public string? BillingServiceName { get; init; }

    /// <summary>
    /// Billing reference value from the payment order item
    /// (e.g. account/subscription number for PayBill, meter number for MeterCharge).
    /// </summary>
    public string? BillingRef { get; init; }
}

public sealed class PaymentSnelTicketModel
{
    public bool IsPostPaid { get; init; }
    public bool IsPrePaid { get; init; }
    public bool ShowPrepaidTokenSection { get; init; }
    public bool ShowPrepaidTechnicalCodes { get; init; }
    public bool ShowTotalUnits { get; init; }

    public string? ReceiptNumber { get; init; }
    public string? ReceiptDate { get; init; }
    public string? ReceiptTime { get; init; }
    public string? TaxNumber { get; init; }
    public string? Duplicate { get; init; }
    public string? MeterNumber { get; init; }
    public string? CustomerName { get; init; }
    public string? PaNumber { get; init; }
    public string? PointOfConnection { get; init; }
    public string ServiceTitle { get; init; } = default!;
    public string? TransactionNumber { get; init; }

    public IReadOnlyList<PrepaidTokenPrintModel> PrepaidTokens { get; init; } = [];
    public string? Sgc { get; init; }
    public string? TaxIndex { get; init; }
    public string? Krn { get; init; }

    public string AmountCollected { get; init; } = default!;
    public string Currency { get; init; } = default!;
    public string? TotalUnitsKwh { get; init; }
    public string TotalValueTtc { get; init; } = default!;

    public IReadOnlyList<PaymentTariffLinePrintModel> TariffLines { get; init; } = [];

    public string? TariffCategory { get; init; }
    public string? TariffRatePerKwh { get; init; }
    public string? AmountExcludingTax { get; init; }
    public string? Vat { get; init; }
    public string? VatAmount { get; init; }
    public string? VatRate { get; init; }
    public string? Tep { get; init; }
    public string? ProvincialTax { get; init; }
    public string? ProvincialTaxAmount { get; init; }
    public string? ProvincialTaxRate { get; init; }

    public IReadOnlyList<PaymentLevyLinePrintModel> LevyLines { get; init; } = [];

    public string? PreTaxDebts1 { get; init; }

    public string? Cashpower { get; init; }

    public string? Arrears { get; init; }
    public string AdvanceOnWorks { get; init; } = "0,00";
    public string Debts1 { get; init; } = "0,00";
    public string TotalDeductions { get; init; } = "0,00";

    public string? Operator { get; init; }
    public string? InvoiceRef { get; init; }
}

public sealed class PrepaidTokenPrintModel
{
    public int Index { get; init; }
    public string Value { get; init; } = default!;
    public IReadOnlyList<string> DisplayLines { get; init; } = [];
}

public sealed class PaymentLevyLinePrintModel
{
    public string Label { get; init; } = default!;
    public string Amount { get; init; } = default!;
}

public sealed class PaymentTariffLinePrintModel
{
    public string AmountExcludingTax { get; init; } = default!;
    public string? RatePerKwh { get; init; }
    public string? Category { get; init; }
}
