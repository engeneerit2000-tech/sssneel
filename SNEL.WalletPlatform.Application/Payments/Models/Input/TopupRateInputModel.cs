using System.ComponentModel.DataAnnotations;

namespace SNEL.WalletPlatform.Application.Payments.Models.Input;

public sealed class TopupRateInputModel
{       
    [Required]
    public bool GroupByProvider { get; init; }
    [Required]
    public bool GroupByMethod { get; init; }
    [Required]
    [Range(1, 2, ErrorMessage = "CurrencyId must be either 1 or 2")]
    public int CurrencyId { get; init; }
    public long? CustomerPartnerId { get; init; } = null;
    public long? PaymentProviderId { get; set; } = null;
    public long? PaymentMethodId { get; init; } = null;


}
