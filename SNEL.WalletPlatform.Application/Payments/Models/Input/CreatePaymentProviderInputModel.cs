namespace SNEL.WalletPlatform.Application.Payments.Models.Input;

public sealed class CreatePaymentProviderInputModel
{
    public string LegalName { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public bool PartnerActive { get; set; } = true;

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
}
