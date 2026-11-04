using System.ComponentModel.DataAnnotations;

namespace SNEL.WalletPlatform.Application.Auth.Models.Input;

public class ChangeUserPasswordInputModel
{
    [Required, MinLength(8), MaxLength(256)]
    public required string NewPassword { get; set; }

    [Required, MinLength(8), MaxLength(256)]
    public required string OldPassword { get; set; }
}
