using System.ComponentModel.DataAnnotations;

namespace SNEL.WalletPlatform.Application.BillingServices.Models.Input;

//TODO: must add billingservice code and biller code as invoices ids may be from different biller other than SNEL
//TODO: invoice ids must be string to support all cases for different billers.
public sealed record PayBillsInputModel
{
    [Required]
    public decimal Amount { get; init; }

    public List<Guid>? InvoicesIds { get; init; }

    public List<string>? InvoiceRefs { get; init; }

    [MaxLength(50)]
    public string? PaymentMethodCode { get; init; }

    [MaxLength(50),Required]
    public string? PaymentServiceProviderCode { get; init; }

    [MaxLength(20)]
    public string? ChannelCode { get; init; }
}
