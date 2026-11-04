using System.ComponentModel.DataAnnotations;

namespace SNEL.WalletPlatform.Application.Auth.Models.Input;

public class GenerateRegistrationOtpInputModel
{
    [Required, MaxLength(30)]
    public string MobileNo { get; init; } = string.Empty;
}
