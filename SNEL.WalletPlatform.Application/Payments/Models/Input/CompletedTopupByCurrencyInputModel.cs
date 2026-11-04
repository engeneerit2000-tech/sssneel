using System.ComponentModel.DataAnnotations;

namespace SNEL.WalletPlatform.Application.Payments.Models.Input;

public sealed class CompletedTopupByCurrencyInputModel
{
    public bool GroupByProvider { get; init; } = true;

    public long? PaymentProviderId { get; set; }

    [Required]
    public string CurrencyCode { get; init; } = string.Empty;
}
