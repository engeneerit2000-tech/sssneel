using System.ComponentModel.DataAnnotations;
using SNEL.WalletPlatform.Domain.Customers;

namespace SNEL.WalletPlatform.Application.Customers.Models.Input;

public sealed class UpdateCustomerStatusInputModel
{
    [Required]
    public CustomerStatus Status { get; init; }
}
