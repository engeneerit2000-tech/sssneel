namespace SNEL.WalletPlatform.Application.Invoices.Models.Output;

public sealed class InvoicePrintModel
{
    public string LanguageCode { get; init; } = "en";
    public string InvoiceNum { get; init; } = default!; //1
    public string InvoiceTitle { get; init; } = "FACTURE D'ENERGIE ELECTRIQUE BASSE TENSION";
    public DateTime InvoiceDate { get; init; }
    /// <summary>UTC date when the invoice PDF/HTML is generated (date only, no time).</summary>
    public string PrintDate { get; init; } = default!;
    public IReadOnlyList<CustomerOpenInvoicePrintModel> CustomerOpenInvoices { get; init; } = [];
    public string? InvoiceBarcodeBase64 { get; init; }
    public InvoicePrintLabels Labels => InvoicePrintLabels.For(LanguageCode);

    public string CustomerName { get; init; } = default!; //4

    public string? CenterDesignation { get; init; } //2
    public string? Pa { get; init; } //3
    public string? PointOfSaleName { get; init; } //6
    public string? CustomerTypeName { get; init; } //7

    public string? SubstationCode { get; init; } //9
    public string? TariffType { get; init; } //18

    public string? ClientPropertyNumber { get; init; } //5
    public string? ClientStreet { get; init; } //5
    public string? ClientCommune { get; init; } //5

    public decimal? MeterNumber { get; init; } //11
    public decimal? CalculationCoefficient { get; init; } //12
    public decimal? PreviousBillingIndex { get; init; } //13
    public decimal? CurrentBillingIndex { get; init; } //14

    public decimal? Consumption { get; init; } //15
    public decimal? DiscountCode { get; init; } //16
    public decimal? BilledConsumption { get; init; } //17

    public int? BillingYear { get; init; } //19
    public int? BillingMonth { get; init; } //19

    public decimal? AmountExcludingTaxFc { get; init; } //23
    public decimal? AmountExcludingTaxUs { get; init; } //23
    /// <summary>
    /// Amount excluding tax according to the currency: if the currency is CDF → use Fc, otherwise USD.
    /// </summary>
    public decimal? AmountExcludingTax =>
        string.Equals(InvoiceCurrencyCode, "CDF", StringComparison.OrdinalIgnoreCase)
            ? AmountExcludingTaxFc
            : AmountExcludingTaxUs;

    public decimal? ProvincialTax { get; init; } //24
    public decimal? Vat { get; init; } //25

    public decimal? AmountFc { get; init; } //26
    public string ?InvoiceCurrencyCode { get; init; }

    public decimal? SubtotalFc { get; init; }
    public decimal? TaxAmountFc { get; init; }
    public decimal? TotalAmountFc { get; init; }
    public decimal? TotalAmountUs { get; init; }

    /// <summary>
    /// Montant total selon la devise: CDF → Fc, sinon USD.
    /// </summary>
    public decimal? TotalAmount =>
        string.Equals(InvoiceCurrencyCode, "CDF", StringComparison.OrdinalIgnoreCase)
            ? TotalAmountFc
            : TotalAmountUs;

    public decimal CustomerOpenInvoicesTotalAmountOutstanding { get; init; }

    public IReadOnlyList<InvoiceLinePrintModel> Lines { get; init; } = [];

    // helper property
    public string InvoicePeriod =>
        BillingMonth.HasValue && BillingYear.HasValue
            ? $"{BillingMonth}/{BillingYear}"
            : string.Empty;
}

public sealed class InvoicePrintLabels
{
    private static readonly InvoicePrintLabels English = new()
    {
        DatePrinted = "Print Date",
        Center = "Center",
        Name = "Name",
        Address = "Address",
        CollectionPoint = "Collection Point",
        CustomerType = "Customer Type",
        Substation = "Substation",
        Route = "Route",
        Meter = "Meter",
        Coefficient = "Coefficient",
        PreviousIndex = "Previous Index",
        CurrentIndex = "Current Index",
        CalculatedKwh = "Calculated kWh",
        Discount = "Discount",
        BilledKwh = "Billed kWh",
        TariffCode = "Tariff Code",
        MeterReadingInterpretation = "Meter Reading Interpretation",
        BillingPeriod = "Billing Period",
        Value = "Value",
        PublicLighting = "Public Lighting 1%",
        InvoiceAmount = "Invoice Amount",
        UnpaidInvoices = "Unpaid Invoices Statement",
        DocumentNumber = "Document Number",
        Period = "Period",
        InitialAmount = "Initial Amount",
        UpdatedAmount = "Updated Amount",
        GrandTotal = "Grand Total",
        NoticeTitle = "NOTICE",
        NoticeBody = "Please mention the invoice number on the payment order when paying through the bank.",
        PaymentCoupon = "PAYMENT COUPON",
        ReservedForSnel = "(Reserved for SNEL)",
        CashDesk = "Cash Desk",
        InvoiceNumber = "Invoice Number",
        AmountToPay = "Amount Due",
        TotalToPay = "Total Due",
        DebitCreditNoteTitle = "DEBIT/CREDIT NOTE",
        ElectricityInvoiceTitle = "LOW VOLTAGE ELECTRICITY INVOICE"
    };

    private static readonly InvoicePrintLabels French = new()
    {
        DatePrinted = "Date d'édition",
        Center = "Centre",
        Name = "Nom",
        Address = "Adresse",
        CollectionPoint = "Point de Perception",
        CustomerType = "Type Client",
        Substation = "Cabine",
        Route = "Tournée",
        Meter = "Compteur",
        Coefficient = "Coefficient",
        PreviousIndex = "Index Précédent",
        CurrentIndex = "Index Actuel",
        CalculatedKwh = "Kwh Calculé",
        Discount = "Rabais",
        BilledKwh = "Kwh Facturé",
        TariffCode = "Code Tarif",
        MeterReadingInterpretation = "Interprétation Relevé",
        BillingPeriod = "Période Facturée",
        Value = "Valeur",
        PublicLighting = "Eclairage Public 1%",
        InvoiceAmount = "Montant de la Facture",
        UnpaidInvoices = "Relevé des factures impayées",
        DocumentNumber = "Numéro Document",
        Period = "Période",
        InitialAmount = "Montant Initial",
        UpdatedAmount = "Montant actualisé",
        GrandTotal = "Total Général",
        NoticeTitle = "AVIS",
        NoticeBody = "Veuillez mentionner le numéro de la facture sur l'OP en cas de paiement par la banque.",
        PaymentCoupon = "COUPON REGLEMENT",
        ReservedForSnel = "(Réservé à la SNEL)",
        CashDesk = "Caisse",
        InvoiceNumber = "Numéro Facture",
        AmountToPay = "Montant à payer",
        TotalToPay = "Total à payer",
        DebitCreditNoteTitle = "NOTE DE DEBIT CREDIT",
        ElectricityInvoiceTitle = "FACTURE D'ENERGIE ELECTRIQUE BASSE TENSION"
    };

    public string DatePrinted { get; init; } = default!;
    public string Center { get; init; } = default!;
    public string Name { get; init; } = default!;
    public string Address { get; init; } = default!;
    public string CollectionPoint { get; init; } = default!;
    public string CustomerType { get; init; } = default!;
    public string Substation { get; init; } = default!;
    public string Route { get; init; } = default!;
    public string Meter { get; init; } = default!;
    public string Coefficient { get; init; } = default!;
    public string PreviousIndex { get; init; } = default!;
    public string CurrentIndex { get; init; } = default!;
    public string CalculatedKwh { get; init; } = default!;
    public string Discount { get; init; } = default!;
    public string BilledKwh { get; init; } = default!;
    public string TariffCode { get; init; } = default!;
    public string MeterReadingInterpretation { get; init; } = default!;
    public string BillingPeriod { get; init; } = default!;
    public string Value { get; init; } = default!;
    public string PublicLighting { get; init; } = default!;
    public string InvoiceAmount { get; init; } = default!;
    public string UnpaidInvoices { get; init; } = default!;
    public string DocumentNumber { get; init; } = default!;
    public string Period { get; init; } = default!;
    public string InitialAmount { get; init; } = default!;
    public string UpdatedAmount { get; init; } = default!;
    public string GrandTotal { get; init; } = default!;
    public string NoticeTitle { get; init; } = default!;
    public string NoticeBody { get; init; } = default!;
    public string PaymentCoupon { get; init; } = default!;
    public string ReservedForSnel { get; init; } = default!;
    public string CashDesk { get; init; } = default!;
    public string InvoiceNumber { get; init; } = default!;
    public string AmountToPay { get; init; } = default!;
    public string TotalToPay { get; init; } = default!;
    public string DebitCreditNoteTitle { get; init; } = default!;
    public string ElectricityInvoiceTitle { get; init; } = default!;

    public static InvoicePrintLabels For(string? languageCode) =>
        string.Equals(languageCode, "fr", StringComparison.OrdinalIgnoreCase)
            ? French
            : English;
}

public sealed class CustomerOpenInvoicePrintModel
{
    public string InvoiceRef { get; init; } = default!;
    public DateTime IssueDate { get; init; }
    public string CurrencyAbbreviation { get; init; } = default!;
    public decimal AmountOutstanding { get; init; }
}

public sealed class InvoiceLinePrintModel
{
    public string Description { get; init; } = default!;
    public decimal Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal LineTotal { get; init; }
}