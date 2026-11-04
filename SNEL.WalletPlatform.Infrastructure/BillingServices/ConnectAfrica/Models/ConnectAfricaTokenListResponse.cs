namespace SNEL.WalletPlatform.Infrastructure.BillingServices.ConnectAfrica.Models;

public sealed record ConnectAfricaTokenListResponse(
    bool IsSuccess,
    int? NumberOfTokens,
    IReadOnlyList<ConnectAfricaTokenListEntry> Tokens,
    string RequestBody,
    string ResponseBody,
    string? ResponseCode);
