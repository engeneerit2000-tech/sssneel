namespace SNEL.WalletPlatform.Application.Accounting.Models.Output;

public class CreateJournalForTopupInputModel
{
    public Guid PaymentId { get; set; }
    public decimal Amount { get; set; }
    public Guid CustomerAccountId { get; set; }
    public Guid PspAccountId { get; set; }
}