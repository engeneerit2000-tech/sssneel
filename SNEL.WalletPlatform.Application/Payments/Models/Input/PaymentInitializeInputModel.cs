using SNEL.WalletPlatform.Domain.Payments;
using System.ComponentModel.DataAnnotations;

namespace SNEL.WalletPlatform.Application.Payments.Models.Input;

public sealed class PaymentInitializeInputModel
{
    [Required, MaxLength(20)]
    public string CurrencyCode { get; init; } = string.Empty;

    [Range(typeof(decimal), "0.01", "999999999999", ParseLimitsInInvariantCulture = true)]
    public decimal Amount { get; init; }

    /// <summary>
    /// Optional. When omitted together with <see cref="PaymentMethodCode"/>, the platform default payment method is used.
    /// </summary>
    [MaxLength(50)]
    public string? PaymentServiceProviderCode { get; init; }

    /// <summary>
    /// Optional. When omitted together with <see cref="PaymentServiceProviderCode"/>, the platform default payment method is used.
    /// </summary>
    [MaxLength(50)]
    public string? PaymentMethodCode { get; init; }

    /// <summary>
    /// Optional. Payer MSISDN for AkibaPay customer deposit when using init-topup. International format with leading <c>+</c>.
    /// </summary>
    [RegularExpression(@"^(\+243\d{9}|0{11}\d{1})$", ErrorMessage = "FromNumber must be in the format +243XXXXXXXXX (9 digits after +243) or 00000000000X (12 digits total)")]
    [MaxLength(13)]
    public string? FromNumber { get; init; }

    /// <summary>Optional customer email used to pre-fill an EasyPay card checkout.</summary>
    [EmailAddress]
    [MaxLength(254)]
    public string? Email { get; init; }

    /// <summary>Optional description displayed on the EasyPay card checkout.</summary>
    [MaxLength(120)]
    public string? Description { get; init; }

    /// <summary>Optional public HTTPS URL to which EasyPay returns the customer after checkout.</summary>
    [Url]
    [MaxLength(2048)]
    public string? ReturnUrl { get; init; }

    /// <summary>When true, asks EasyPay to email the hosted card-payment link to <see cref="Email"/>.</summary>
    public bool SendPaymentLinkByEmail { get; init; }

    public PaymentTypeEnum? PaymentType { get; init; } = null;
}
