using System.ComponentModel.DataAnnotations;

namespace SNEL.WalletPlatform.Application.Auth.Models.Input;

public class VerifyOtpInputModel
{
    [Required]
    public required Guid OtpChallengeId { get; set; }

    [Required]
    public required string OtpCode { get; set; }
}
