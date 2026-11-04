namespace SNEL.WalletPlatform.Infrastructure.BillingServices.ConnectAfrica.Models;

public sealed record ConnectAfricaTokenListEntry(
    string ReceiptNumber,
    decimal? Amount);
