namespace SNEL.WalletPlatform.Infrastructure.BillingServices.ConnectAfrica.Models;

public sealed record ConnectAfricaVendRequest(
    string Meter,
    decimal Amount,
    string CustomerPhone);
