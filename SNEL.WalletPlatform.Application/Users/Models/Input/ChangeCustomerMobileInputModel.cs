using System.ComponentModel.DataAnnotations;

namespace SNEL.WalletPlatform.Application.Users.Models.Input;

public sealed class ChangeCustomerMobileInputModel
{
    [Required, MaxLength(30)]
    public string MobileNo { get; init; } = string.Empty;
}
