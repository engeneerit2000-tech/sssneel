namespace SNEL.WalletPlatform.Application.Billers.Models.Input;

public sealed class AddBillerInputModel
{
    public string LegalName { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public bool Active { get; set; } = true;
    public bool ApplyAfterPay { get; set; }

    public string BillerCode { get; set; } = string.Empty;
    public string BillerName { get; set; } = string.Empty;

}
