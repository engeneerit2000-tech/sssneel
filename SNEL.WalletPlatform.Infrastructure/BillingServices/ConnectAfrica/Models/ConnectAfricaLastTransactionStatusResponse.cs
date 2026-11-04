namespace SNEL.WalletPlatform.Infrastructure.BillingServices.ConnectAfrica.Models;

public sealed record ConnectAfricaLastTransactionStatusResponse(
    string? TransactionId,
    string? Status,
    string RequestBody,
    string ResponseBody);
