namespace SNEL.WalletPlatform.Application.Invoicing.Models;

/// <summary>
/// Input for mapping an Oracle invoice row to InvoiceEntity (row + resolved currency and billing service).
/// </summary>
public sealed class OracleInvoiceMapInput
{
    public OracleInvoiceRowModel Row { get; init; } = null!;

    // Uses the integer CurrencyId from the lookup table.
    public int CurrencyId { get; init; }

    public long BillingServiceId { get; init; }
}

