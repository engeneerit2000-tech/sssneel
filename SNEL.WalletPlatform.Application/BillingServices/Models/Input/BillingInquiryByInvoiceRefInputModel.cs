using System.ComponentModel.DataAnnotations;

namespace SNEL.WalletPlatform.Application.BillingServices.Models.Input;

public sealed class BillingInquiryByInvoiceRefInputModel
{
    [Required]
    [MaxLength(150)]
    public string InvoiceRef { get; init; } = string.Empty;

    public bool UnpaidOnly { get; init; } = false;
}
