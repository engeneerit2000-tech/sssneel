namespace SNEL.WalletPlatform.Infrastructure.BillingServices.ConnectAfrica.Models;

public sealed record ConnectAfricaLastTransactionsResponse(
    IReadOnlyList<ConnectAfricaLastTransactionEntry> Transactions,
    string RequestBody,
    string ResponseBody);
