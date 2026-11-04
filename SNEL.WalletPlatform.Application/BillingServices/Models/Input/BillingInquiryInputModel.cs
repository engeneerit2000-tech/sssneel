using System.ComponentModel.DataAnnotations;

namespace SNEL.WalletPlatform.Application.BillingServices.Models.Input;

public sealed class BillingInquiryInputModel
{
    [Required]
    public Guid CustomerSubscriptionId { get; init; }

    public bool UnpaidOnly { get; init; } = true;
}
