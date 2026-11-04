using SNEL.WalletPlatform.Domain.Accounting.Enums;
using SNEL.WalletPlatform.Domain.Common;

namespace SNEL.WalletPlatform.Domain.Accounting;

public sealed class JournalEntity : TrackedEntity<Guid>
{
    private JournalEntity() { }
    public int JournalTypeId { get; set; }
    public JournalTypeEntity JournalType { get; set; } = null!;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int SourceTypeId { get; set; }
    public string? SourceRef { get; set; }
    public DateTimeOffset SourceTimestamp { get; set; }
    public DateTimeOffset JournalTimestamp { get; set; }
    public int Order { get; set; }
    public ICollection<JournalLineEntity> Lines { get; set; } = new List<JournalLineEntity>();

    public static JournalEntity Create(
        JournalTypesEnum journalType,
        string description,
        decimal amount,
        JournalSourceTypesEnum sourceType,
        string? sourceRef,
        DateTimeOffset sourceTimestamp,
        DateTimeOffset journalTimestamp,
        int order)
    {
        return new JournalEntity()
        {
            JournalTypeId = (int)journalType,
            Amount = amount,
            SourceTypeId = (int)sourceType,
            SourceRef = string.IsNullOrWhiteSpace(sourceRef) ? null : sourceRef.Trim(),
            SourceTimestamp = sourceTimestamp,
            JournalTimestamp = journalTimestamp,
            Description = description?.Trim() ?? throw new ArgumentNullException(nameof(description)),
            Lines = new List<JournalLineEntity>(),
            Order = order
        };
    }
    public JournalEntity AddLine(
        JournalLineTypesEnum journalLineType,
        Guid accountId,
        bool isCredit,
        decimal amount,
        decimal balanceAfter,
        int order,
        PlatformInvoiceEntity? platformInvoice = null)
    {
        Lines.Add(JournalLineEntity.Create(
            journalLineType,
            accountId,
            isCredit,amount,balanceAfter,order,
            platformInvoice));
        return this;
    }
    
}