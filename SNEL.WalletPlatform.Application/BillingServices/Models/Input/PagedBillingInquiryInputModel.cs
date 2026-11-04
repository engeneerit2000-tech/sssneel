using System.ComponentModel.DataAnnotations;
using SNEL.WalletPlatform.Application.Common.Models.Input;

namespace SNEL.WalletPlatform.Application.BillingServices.Models.Input;

public sealed class PagedBillingInquiryInputModel : BaseQueryInputModel
{
    [Required]
    public Guid CustomerSubscriptionId { get; set; }

    public bool UnpaidOnly { get; set; } = true;
}
