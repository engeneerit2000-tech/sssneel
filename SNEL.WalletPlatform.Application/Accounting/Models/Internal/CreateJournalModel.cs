using SNEL.WalletPlatform.Domain.Accounting;
using SNEL.WalletPlatform.Domain.Accounting.Enums;

namespace SNEL.WalletPlatform.Application.Accounting.Models.Internal;

public class CreateJournalModel
{
    public JournalTypesEnum JournalType { get; set; }
    public JournalSourceTypesEnum SourceType { get; set; }
    public string? SourceRef { get; set; }
    public DateTimeOffset SourceDate { get; set; }
    public DateTimeOffset JournalPostingDate { get; set; }
    
    public bool FirstAccountCredit { get; set; }
    public required Guid FirstAccountId { get; set; }
    public JournalLineTypesEnum FirstJournalLineType { get; set; }
    
    public bool SecondAccountCredit { get; set; }
    public required Guid SecondAccountId { get; set; }
    
    public JournalLineTypesEnum SecondJournalLineType { get; set; }
    
    public decimal Amount { get; set; }
    public string DescriptionTemplate { get; set; } = string.Empty;
    public int Order { get; set; }

    public PlatformInvoiceEntity? PlatformInvoice { get; set; }
}