using System.ComponentModel.DataAnnotations;

namespace SNEL.WalletPlatform.Application.Users.Models.Input;

public sealed class ChangeManagedUserPasswordInputModel
{
    [Required, MinLength(8), MaxLength(256)]
    public string NewPassword { get; init; } = string.Empty;
}
