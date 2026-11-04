using System.ComponentModel.DataAnnotations;

namespace SNEL.WalletPlatform.Application.Payments.Models.Input;

public sealed class PaymentRateInputModel
{       
    [Required]
    public bool GroupByBiller { get; init; }
    [Required]
    public bool GroupByBillingService { get; init; }
    [Required]
    [Range(1, 2, ErrorMessage = "CurrencyId must be either 1 or 2")]
    public int CurrencyId { get; init; }
    public long? BillerId { get; init; } = null;
    public long? BillingServiceId { get; init; } = null;    
}
