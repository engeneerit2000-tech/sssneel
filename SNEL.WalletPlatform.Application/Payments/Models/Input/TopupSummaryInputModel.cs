using System.ComponentModel.DataAnnotations;

namespace SNEL.WalletPlatform.Application.Payments.Models.Input;

public sealed class TopupSummaryInputModel
{
    [Required, MaxLength(20)]
    public string CurrencyCode { get; init; } = string.Empty;

    [Required, Range(typeof(decimal), "0.01", "999999999999")]
    public decimal Amount { get; init; }

    [Required]
    public long PaymentMethodId { get; init; }
}
