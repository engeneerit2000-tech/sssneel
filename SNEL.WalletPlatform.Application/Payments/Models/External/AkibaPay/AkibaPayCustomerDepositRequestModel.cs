using System.Text.Json.Serialization;

namespace SNEL.WalletPlatform.Application.Payments.Models.External.AkibaPay;

public sealed class AkibaPayCustomerDepositRequestModel
{
    [JsonPropertyName("merchantId")]
    public string MerchantId { get; init; } = string.Empty;

    [JsonPropertyName("merchantPass")]
    public string MerchantPass { get; init; } = string.Empty;

    /// <summary>MSISDN without leading '+'
    [JsonPropertyName("fromNumber")]
    public string FromNumber { get; init; } = string.Empty;

    /// <summary>Required by AkibaPay; send empty string for customer deposit.</summary>
    [JsonPropertyName("toNumber")]
    public string ToNumber { get; init; } = string.Empty;

    [JsonPropertyName("amount")]
    public decimal Amount { get; init; }

    [JsonPropertyName("currency")]
    public string Currency { get; init; } = string.Empty;

    [JsonPropertyName("callbackUrl")]
    public string CallbackUrl { get; init; } = string.Empty;

    [JsonPropertyName("merchantReference")]
    public string MerchantReference { get; init; } = string.Empty;

    [JsonPropertyName("hash")]
    public string Hash { get; init; } = string.Empty;
}

