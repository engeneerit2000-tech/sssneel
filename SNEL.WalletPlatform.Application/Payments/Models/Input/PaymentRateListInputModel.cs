using SNEL.WalletPlatform.Application.Common.Models.Input;
using System.ComponentModel.DataAnnotations;

namespace SNEL.WalletPlatform.Application.Payments.Models.Input;

public sealed class PaymentRateListInputModel : BaseQueryInputModel
{
    [Required]
    [Range(1, 2, ErrorMessage = "CurrencyId must be either 1 or 2")]
    public int CurrencyId { get; init; }
    public long? BillerId { get; set; } = null;
    public long? BillingServiceId { get; init; } = null;    
}
public sealed class PaymentRateExportInputModel
{
    [Required]
    [Range(1, 2, ErrorMessage = "CurrencyId must be either 1 or 2")]
    public int CurrencyId { get; init; }
    public long? BillerId { get; set; } = null;
    public long? BillingServiceId { get; init; } = null;
}