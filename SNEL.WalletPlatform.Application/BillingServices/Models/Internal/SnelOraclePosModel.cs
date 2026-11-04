namespace SNEL.WalletPlatform.Application.BillingServices.Models.Internal;

public sealed record SnelOraclePosModel(
    decimal PosId,
    string PosCode,
    string CenterCode,
    string Description);
