namespace SNEL.WalletPlatform.Infrastructure.BillingServices.ConnectAfrica.Models;

public sealed record ConnectAfricaAckResponse(
    bool IsSuccess,
    string RequestBody,
    string? ResponseBody,
    string ResponseCode);
