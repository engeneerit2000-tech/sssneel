using System.ComponentModel.DataAnnotations;

namespace SNEL.WalletPlatform.Application.BillingServices.Models.Input;

public sealed class GetCustomActionsInputModel
{
    [Required]
    public Guid CustomerSubscriptionId { get; init; }   
}
