using System.ComponentModel.DataAnnotations;

namespace SNEL.WalletPlatform.Application.Payments.Models.Input;

public sealed class UpdatePaymentProviderInputModel
{
    [Required, MaxLength(50)]
    public required string Code { get; init; }

    [Required]
    public required string Name { get; init; }

    [Required]
    public required string LegalName { get; init; }
    public string? DisplayName { get; init; }
    public bool PartnerActive { get; init; } = true;
}
