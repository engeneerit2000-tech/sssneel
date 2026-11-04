namespace SNEL.WalletPlatform.Application.Accounting.Models.Output;

public sealed class PlatformInvoicePrintModel
{
    public long PlatformInvoiceId { get; init; }
    public string? AcceptLanguage { get; init; }
    public DateTimeOffset InvoiceDate { get; init; }

    /// <summary>Invoice calendar date only (<c>dd/MM/yyyy</c>, UTC).</summary>
    public string InvoiceDateDisplay { get; init; } = string.Empty;

    /// <summary>Customer organization display name resolved from invoice parameters.</summary>
    public string CustomerDisplayName { get; init; } = string.Empty;

    /// <summary>Biller / PSP display name when present in invoice parameters.</summary>
    public string? CounterpartyPartnerDisplayName { get; init; }

    /// <summary>Partner on the invoice record (<see cref="PlatformInvoiceEntity.PartnerId"/>).</summary>
    public string InvoicedPartnerDisplayName { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public decimal Amount { get; init; }
    public decimal Tax { get; init; }
    public decimal Total { get; init; }
    public string Currency { get; init; } = string.Empty;

    public string? InvoiceBarcodeBase64 { get; init; }
}
