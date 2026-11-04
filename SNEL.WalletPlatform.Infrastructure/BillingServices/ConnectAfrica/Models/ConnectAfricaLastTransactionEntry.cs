namespace SNEL.WalletPlatform.Infrastructure.BillingServices.ConnectAfrica.Models;

public sealed record ConnectAfricaLastTransactionEntry(
    DateOnly? Date,
    decimal? Price,
    string? ReceiptNumber);
