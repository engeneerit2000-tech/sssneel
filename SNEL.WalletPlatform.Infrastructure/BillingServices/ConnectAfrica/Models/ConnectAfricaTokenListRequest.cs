namespace SNEL.WalletPlatform.Infrastructure.BillingServices.ConnectAfrica.Models;

public sealed record ConnectAfricaTokenListRequest(
    string Meter,
    int NumberOfTokens,
    int ListOption);
