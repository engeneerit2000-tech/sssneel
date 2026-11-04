using SNEL.WalletPlatform.Application.Lookups.Models.Output;
using SNEL.WalletPlatform.Domain.Accounting;

namespace SNEL.WalletPlatform.Application.Accounting.Models.Output;

public sealed record JournalLineStatementSummary(decimal Credit, decimal Debite);
public sealed class JournalLineStatementOutputModel
{
    public JournalLineStatementOutputModel() { }

    public JournalLineStatementOutputModel(
        Guid journalId,
        string journalDescription,
        string journalSourceRef,
        int journalSourceTypeId,
        int journalLineTypeId,
        JournalLineTypeOutputModel journalLineType,
        Guid accountId,
        decimal debit,
        decimal credit,
        decimal balance,
        long? platformInvoiceId,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc,
        string? createdBy,
        string? updatedBy)
    {
        JournalId = journalId;
        JournalDescription = journalDescription;
        JournalSourceRef = journalSourceRef;
        JournalSourceTypeId = journalSourceTypeId;
        JournalLineTypeId = journalLineTypeId;
        JournalLineType = journalLineType;
        AccountId = accountId;
        Debit = debit;
        Credit = credit;
        Balance = balance;
        PlatformInvoiceId = platformInvoiceId;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
        CreatedBy = createdBy;
        UpdatedBy = updatedBy;
    }

    public Guid JournalId { get; private set; }
    public string JournalDescription { get; private set; }
    public string JournalSourceRef { get; set; }
    public int JournalSourceTypeId { get; set; }
    public int JournalLineTypeId { get; private set; }
    public JournalLineTypeOutputModel JournalLineType { get; private set; }
    public Guid AccountId { get; private set; }
    public decimal Debit { get; private set; }
    public decimal Credit { get; private set; }
    public decimal Balance { get; private set; }
    public long? PlatformInvoiceId { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}

public sealed class JournalLineStatementExcelModel
{
    public JournalLineStatementExcelModel() { }

    public JournalLineStatementExcelModel(
        string journalDescription,
        string journalSourceRef,
        string journalLineTypeName,
        decimal debit,
        decimal credit,
        decimal balance,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc,
        string? createdBy,
        string? updatedBy)
    {
        JournalDescription = journalDescription;
        JournalSourceRef = journalSourceRef;
        JournalLineTypeName = journalLineTypeName;
        Debit = debit;
        Credit = credit;
        Balance = balance;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
        CreatedBy = createdBy;
        UpdatedBy = updatedBy;
    }

    public string JournalDescription { get; private set; }
    public string JournalSourceRef { get; set; }
    public string JournalLineTypeName { get; set; }
    public decimal Debit { get; private set; }
    public decimal Credit { get; private set; }
    public decimal Balance { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}
