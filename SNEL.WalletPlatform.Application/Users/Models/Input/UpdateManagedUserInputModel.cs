using System.ComponentModel.DataAnnotations;
using SNEL.WalletPlatform.Domain.Users;

namespace SNEL.WalletPlatform.Application.Users.Models.Input;

public sealed class UpdateManagedUserInputModel
{
    [Required, MaxLength(100)]
    public string FirstName { get; init; } = string.Empty;

    [MaxLength(100)]
    public string? SecondName { get; init; }

    [MaxLength(100)]
    public string? ThirdName { get; init; }

    [Required, MaxLength(100)]
    public string LastName { get; init; } = string.Empty;

    public int? GenderId { get; init; }

    [Required, MinLength(3), MaxLength(30)]
    public string MobileNo { get; init; } = string.Empty;

    [EmailAddress, MaxLength(256)]
    public string? Email { get; init; }

    [MaxLength(50)]
    public string? NationalNo { get; init; }

    [Required]
    public long PartnerId { get; init; }

    public UserStatus Status { get; init; } = UserStatus.Active;

    [MaxLength(10)]
    public string Language { get; init; } = "en";
}
