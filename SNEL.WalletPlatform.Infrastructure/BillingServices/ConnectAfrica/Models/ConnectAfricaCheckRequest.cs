namespace SNEL.WalletPlatform.Infrastructure.BillingServices.ConnectAfrica.Models;

public sealed record ConnectAfricaCheckRequest(
    string Meter,
    decimal Amount);
