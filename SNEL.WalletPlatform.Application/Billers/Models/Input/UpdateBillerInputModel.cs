namespace SNEL.WalletPlatform.Application.Billers.Models.Input;

public sealed class UpdateBillerInputModel
{
    public string BillerCode { get; set; } = string.Empty;
    public string BillerName { get; set; } = string.Empty;
    public bool ApplyAfterPay { get; set; }
    public string LegalName { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public bool Active { get; set; } = true;
}
