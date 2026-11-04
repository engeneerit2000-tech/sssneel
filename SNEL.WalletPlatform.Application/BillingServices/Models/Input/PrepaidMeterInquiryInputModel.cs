using System.ComponentModel.DataAnnotations;

namespace SNEL.WalletPlatform.Application.BillingServices.Models.Input;

public sealed class PrepaidMeterInquiryInputModel
{
    [Required]
    [MaxLength(50)]
    public string MeterNumber { get; init; } = string.Empty;
}
