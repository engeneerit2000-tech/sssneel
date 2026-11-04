using System.ComponentModel.DataAnnotations;

namespace SNEL.WalletPlatform.Application.BillingServices.Models.Input;

public sealed record PayBillsSummaryInputModel
{
    [Required]
    public decimal Amount { get; init; }

    [Required]
    public List<Guid> InvoicesIds { get; init; }
}
