using System.Text.Json.Serialization;

namespace SNEL.WalletPlatform.Application.Webhooks.Models.Input;

public sealed class AkibaPayWebhookEventInputModel
{
    [JsonPropertyName("AkibaReference")]
    public string AkibaReference { get; init; } = string.Empty;

    [JsonPropertyName("MerchantReference")]
    public string MerchantReference { get; init; } = string.Empty;

    [JsonPropertyName("TransactionStatus")]
    public string TransactionStatus { get; init; } = string.Empty;

    [JsonPropertyName("ToNumberTelecomReference")]
    public string? ToNumberTelecomReference { get; init; }

    [JsonPropertyName("FromNumberTelecomReference")]
    public string? FromNumberTelecomReference { get; init; }

    [JsonPropertyName("Amount")]
    public decimal Amount { get; init; }

    [JsonPropertyName("Currency")]
    public string Currency { get; init; } = string.Empty;

    [JsonPropertyName("ToNumber")]
    public string? ToNumber { get; init; }

    [JsonPropertyName("FromNumber")]
    public string? FromNumber { get; init; }

    [JsonPropertyName("CreatedAt")]
    public DateTimeOffset? CreatedAt { get; init; }

    [JsonPropertyName("UpdatedAt")]
    public DateTimeOffset? UpdatedAt { get; init; }

    [JsonPropertyName("CallbackGeneratedAt")]
    public DateTimeOffset? CallbackGeneratedAt { get; init; }
}

