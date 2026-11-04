using System.Text.Json.Serialization;

namespace SNEL.WalletPlatform.Application.Webhooks.Models.Input;

public sealed class EasypayWebhookEventInputModel
{
    [JsonPropertyName("referenceId")]
    public string ReferenceId { get; init; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; init; } = string.Empty;

    [JsonPropertyName("transactionId")]
    public string TransactionId { get; init; } = string.Empty;

    [JsonPropertyName("phone")]
    public string? Phone { get; init; }

    [JsonPropertyName("amount")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public decimal Amount { get; init; }

    [JsonPropertyName("currency")]
    public string Currency { get; init; } = string.Empty;

    [JsonPropertyName("date")]
    public string? Date { get; init; }

    [JsonPropertyName("telecomId")]
    public string? TelecomId { get; init; }

    [JsonPropertyName("provider")]
    public string Provider { get; init; } = string.Empty;

    [JsonPropertyName("type")]
    public string? Type { get; init; }

    [JsonPropertyName("reason")]
    public string? Reason { get; init; }

    [JsonPropertyName("errormsg")]
    public string? ErrorMessage { get; init; }
}
