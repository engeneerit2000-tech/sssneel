using System.ComponentModel.DataAnnotations;

namespace SNEL.WalletPlatform.Application.Customers.Models.Input;

public sealed class UpdateCustomerInputModel
{
    [Required, MaxLength(100)]
    public string FirstName { get; init; } = string.Empty;

    [MaxLength(100)]
    public string? SecondName { get; init; }

    [MaxLength(100)]
    public string? ThirdName { get; init; }

    [Required, MaxLength(100)]
    public string LastName { get; init; } = string.Empty;
    
    [Required]
    public int GenderId { get; init; }

    [MaxLength(30)]
    public string? MobileNo { get; init; }

    [MaxLength(50)]
    public string? NationalNo { get; init; }

    [EmailAddress, MaxLength(256)]
    public string? Email { get; init; }

    public int? AreaId { get; init; }

    [MaxLength(500)]
    public string? Address { get; init; }

    [MaxLength(10)]
    public string Language { get; init; } = "en";

    public IReadOnlyCollection<int>? NotificationChannels { get; init; }
}
