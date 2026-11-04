using System.Text.Json.Serialization;

namespace SNEL.WalletPlatform.Application.Lookups.Models;

public sealed class DefaultPaymentMethodCodesModel
{
    [JsonPropertyName("paymentServiceProviderCode")]
    public string? PaymentServiceProviderCode { get; init; }

    [JsonPropertyName("paymentMethodCode")]
    public string? PaymentMethodCode { get; init; }

    [JsonPropertyName("requirePhoneNumber")]
    public string? RequirePhoneNumber { get; init; }
}
