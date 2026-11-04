using System.ComponentModel.DataAnnotations;

namespace SNEL.WalletPlatform.Application.Auth.Models.Input;

public class GenerateMobileNoOtpInputModel
{
    [Required, MaxLength(30)]
    public string NewMobileNo { get; init; } = string.Empty;
}
