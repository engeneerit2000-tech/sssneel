using SNEL.WalletPlatform.Domain.Common;
using SNEL.WalletPlatform.Domain.Lookups;
using SNEL.WalletPlatform.Domain.Partners;

namespace SNEL.WalletPlatform.Domain.Accounting;

/// <summary>
/// Platform-issued invoice for fee/tax/commission components linked to payment journal lines.
/// <see cref="DescriptionTemplateKey"/> is a localization resource name; <see cref="DescriptionParametersJson"/> is a JSON string array for format arguments.
/// </summary>
public sealed class PlatformInvoiceEntity : TrackedEntity<long>
{
    private PlatformInvoiceEntity() { }

    public long PartnerId { get; private set; }
    public PartnerEntity Partner { get; private set; } = null!;

    public int CurrencyId { get; private set; }
    public CurrencyEntity Currency { get; private set; } = null!;

    /// <summary>Localization key for <c>IStringLocalizer</c> (evaluated using the requested culture at read time).</summary>
    public string DescriptionTemplateKey { get; private set; } = string.Empty;

    /// <summary>JSON-serialized <c>string[]</c> of template parameters.</summary>
    public string? DescriptionParametersJson { get; private set; }

    public DateTimeOffset InvoiceDate { get; private set; }

    public decimal Amount { get; private set; }
    public decimal Tax { get; private set; }
    public decimal Total { get; private set; }

    public static PlatformInvoiceEntity Create(
        long partnerId,
        int currencyId,
        string descriptionTemplateKey,
        string? descriptionParametersJson,
        DateTimeOffset invoiceDate,
        decimal amount,
        decimal tax,
        decimal total)
    {
        return new PlatformInvoiceEntity
        {
            PartnerId = partnerId,
            CurrencyId = currencyId,
            DescriptionTemplateKey = descriptionTemplateKey.Trim(),
            DescriptionParametersJson = string.IsNullOrWhiteSpace(descriptionParametersJson) ? null : descriptionParametersJson.Trim(),
            InvoiceDate = invoiceDate,
            Amount = amount,
            Tax = tax,
            Total = total
        };
    }
}
