namespace SNEL.WalletPlatform.Infrastructure.BillingServices.ConnectAfrica.Models;

public sealed record ConnectAfricaCheckResponse(
    bool IsSuccess,
    decimal? MinimumAllowedChargeValue,
    string? CustomerName,
    string Message);
