namespace SNEL.WalletPlatform.Application.Payments.Models.External.AkibaPay;

public sealed class AkibaPayLoginResponseModel
{
    public AkibaPayLoginUserModel? User { get; set; }

    public string? Token { get; set; }

    public string? RefreshToken { get; set; }
}

public sealed class AkibaPayLoginUserModel
{
    public string? Muid { get; set; }

    public string? MerchantId { get; set; }

    public string? Username { get; set; }
}
