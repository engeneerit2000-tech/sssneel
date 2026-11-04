using System.ComponentModel.DataAnnotations;

namespace SNEL.WalletPlatform.Application.Auth.Models.Input;

public class CompleteForgotPasswordInputModel
{
    [Required]
    public required Guid OtpChallengeId { get; set; }

    [Required, MinLength(8), MaxLength(256)]
    public required string NewPassword { get; set; }
}
