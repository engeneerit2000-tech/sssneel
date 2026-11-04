using SNEL.WalletPlatform.Domain.Accounting.Enums;
using SNEL.WalletPlatform.Domain.Common;

namespace SNEL.WalletPlatform.Domain.Accounting;

public sealed class JournalLineEntity : TrackedEntity<Guid>
{
    private JournalLineEntity() { }

    public Guid JournalId { get; set; }
    public JournalEntity Journal { get; set; } = null!;
    public int JournalLineTypeId { get; set; }
    public JournalLineTypeEntity JournalLineType { get; set; } = null!;
    public Guid AccountId { get; set; }
    public AccountEntity Account { get; set; } = null!;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal Balance { get; set; }
    public int Order { get; private set; }

    public long? PlatformInvoiceId { get; set; }
    public PlatformInvoiceEntity? PlatformInvoice { get; set; }

    public static JournalLineEntity Create(
        JournalLineTypesEnum journalLineType,
        Guid accountId,
        bool isCredit,
        decimal amount,
        decimal balanceAfter,
        int order,
        PlatformInvoiceEntity? platformInvoice = null)
    {
        return new JournalLineEntity()
        {
            JournalLineTypeId = (int)journalLineType,
            AccountId = accountId,
            Debit = isCredit? 0 : amount,
            Credit = isCredit? amount : 0,
            Balance = balanceAfter,
            Order = order,
            PlatformInvoice = platformInvoice
        };
    }
}

