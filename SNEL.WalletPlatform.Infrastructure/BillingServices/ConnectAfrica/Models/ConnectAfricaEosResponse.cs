namespace SNEL.WalletPlatform.Infrastructure.BillingServices.ConnectAfrica.Models;

public sealed record ConnectAfricaEosResponse(
    bool IsSuccess,
    string? SupplyAuthorityName,
    string? UnitNumber,
    string? Date,
    string? Time,
    string? ShiftNumber,
    string? TotalAmount,
    string? OperatorInfo,
    string? ReservedValue,
    string RequestBody,
    string ResponseBody,
    string? ResponseCode);
