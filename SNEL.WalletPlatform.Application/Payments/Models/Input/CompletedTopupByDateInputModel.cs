using System.ComponentModel.DataAnnotations;

namespace SNEL.WalletPlatform.Application.Payments.Models.Input;

public sealed class CompletedTopupByDateInputModel
{
    [Required]
    public string CurrencyCode { get; init; } = string.Empty;

    [Required]
    public DateTimeOffset FromDate { get; init; }

    [Required]
    public DateTimeOffset ToDate { get; init; }

    public long? PaymentProviderId { get; set; }
}
