namespace SNEL.WalletPlatform.Application.Payments.Models.Internal;

public sealed record SnelWalletConfigModel(
    SnelWalletConfirmationConfigModel? Confirmation);

public sealed record SnelWalletConfirmationConfigModel(
    SnelWalletConfirmationMessageConfigModel? Message);

public sealed record SnelWalletConfirmationMessageConfigModel(
    string? En,
    string? Fr);
