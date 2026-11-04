using SNEL.WalletPlatform.Application.Common.Models.Input;
using System.ComponentModel.DataAnnotations;

namespace SNEL.WalletPlatform.Application.Invoices.Models.Input;

public sealed class InvoicesQueryInputModel : BaseQueryInputModel
{
    [Required]
    public Guid CustomerSubscriptionId { get; set; }
}