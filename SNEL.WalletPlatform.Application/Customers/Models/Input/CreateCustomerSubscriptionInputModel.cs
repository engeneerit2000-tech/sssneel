using System.ComponentModel.DataAnnotations;

namespace SNEL.WalletPlatform.Application.Customers.Models.Input;

public sealed record CreateCustomerSubscriptionInputModel
{
    [Required]
    public long BillingServiceId { get; init; }

    public string? AreaCode { get; init; }

    [Required]
    public required string SubscriptionNumber { get; init; }

    [Required, StringLength(200, MinimumLength = 1)]
    public required string NickName { get; init; }
}
