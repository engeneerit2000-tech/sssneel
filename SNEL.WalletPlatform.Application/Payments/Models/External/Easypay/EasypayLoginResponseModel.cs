namespace SNEL.WalletPlatform.Application.Payments.Models.External.Easypay;

public sealed class EasypayLoginResponseModel
{
    public int? Success { get; set; }
    public EasypayLoginResponseDataModel? Data { get; set; }    
}

public sealed class EasypayLoginResponseDataModel
{    
    public string? Token { get; set; }
}
