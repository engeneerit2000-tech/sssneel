using System.ComponentModel.DataAnnotations;

namespace SNEL.WalletPlatform.Application.Users.Models.Input;

public sealed class CreateManagedUserInputModel
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

    [Required, MinLength(8), MaxLength(256)]
    public string Password { get; init; } = string.Empty;

    [Required]
    public long PartnerId { get; init; }

    [MaxLength(10)]
    public string Language { get; init; } = "en";
}
