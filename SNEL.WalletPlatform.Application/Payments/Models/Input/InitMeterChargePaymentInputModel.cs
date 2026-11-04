using System.ComponentModel.DataAnnotations;

namespace SNEL.WalletPlatform.Application.Payments.Models.Input;

public sealed record InitMeterChargePaymentInputModel
{
    [Required]
    [MaxLength(50)]
    public string MeterNumber { get; init; } = string.Empty;

    [Range(typeof(decimal), "0.01", "999999999999", ParseLimitsInInvariantCulture = true)]
    public decimal Amount { get; init; }

    [Required]
    [MaxLength(50)]
    public string BillingServiceCode { get; init; } = string.Empty;
    [MaxLength(50)]
    public string BillerCode { get; init; } = "SNEL";

    [MaxLength(50)]
    public string PaymentMethodCode { get; init; } = "WALLET";

    [MaxLength(50)]
    public string PaymentProviderCode { get; init; } = "SNEL_WALLET";
}
