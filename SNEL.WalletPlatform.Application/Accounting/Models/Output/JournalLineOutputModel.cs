using SNEL.WalletPlatform.Application.Lookups.Models.Output;
using SNEL.WalletPlatform.Domain.Accounting;

namespace SNEL.WalletPlatform.Application.Accounting.Models.Output;

public sealed class JournalLineOutputModel
{
    public JournalLineOutputModel() { }

    public JournalLineOutputModel(
        Guid journalId,
        int journalLineTypeId,
        JournalLineTypeOutputModel journalLineType,
        Guid accountId,
        decimal debit,
        decimal credit,
        decimal balance,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc,
        string? createdBy,
        string? updatedBy)
    {
        JournalId = journalId;
        JournalLineTypeId = journalLineTypeId;
        JournalLineType = journalLineType;
        AccountId = accountId;
        Debit = debit;
        Credit = credit;
        Balance = balance;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
        CreatedBy = createdBy;
        UpdatedBy = updatedBy;
    }

    public Guid JournalId { get; private set; }
    public int JournalLineTypeId { get; private set; }
    public JournalLineTypeOutputModel JournalLineType { get; private set; }
    public Guid AccountId { get; private set; }
    public long? PlatformInvoiceId { get; set; }
    public PlatformInvoiceOutputModel? PlatformInvoice { get; set; }
    public decimal Debit { get; private set; }
    public decimal Credit { get; private set; }
    public decimal Balance { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}

public sealed class PlatformInvoiceOutputModel
{
    public long Id { get; init; }
    public long PartnerId { get; init; }
    public int CurrencyId { get; init; }
    public string DescriptionTemplateKey { get; init; } = string.Empty;
    public string? DescriptionParametersJson { get; init; }
    public string? Description { get; set; }
    public DateTimeOffset InvoiceDate { get; init; }
    public decimal Amount { get; init; }
    public decimal Tax { get; init; }
    public decimal Total { get; init; }

    /// <summary>Derived from DescriptionParametersJson for payment/top-up invoices (POI id).</summary>
    public Guid? PaymentOrderItemId { get; set; }

    public long? CustomerPartnerId { get; set; }
    public Guid? CustomerId { get; set; }
    public string? CustomerDisplayName { get; set; }

    /// <summary>Biller or PSP partner id from parameters, where applicable.</summary>
    public long? SecondaryPartnerId { get; set; }
    public string? SecondaryPartnerDisplayName { get; set; }
}
