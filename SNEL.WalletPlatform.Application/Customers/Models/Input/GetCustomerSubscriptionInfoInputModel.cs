using System.ComponentModel.DataAnnotations;

namespace SNEL.WalletPlatform.Application.Customers.Models.Input;

public sealed class GetCustomerSubscriptionInfoInputModel
{    

    [MaxLength(50)]
    public string? CustomerNumber { get; init; }

    [Required]
    public string AreaCode { get; init; }
}