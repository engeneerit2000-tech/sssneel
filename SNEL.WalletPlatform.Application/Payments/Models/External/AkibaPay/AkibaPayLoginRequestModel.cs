namespace SNEL.WalletPlatform.Application.Payments.Models.External.AkibaPay;

public sealed class AkibaPayLoginRequestModel
{
    public string Login { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}
