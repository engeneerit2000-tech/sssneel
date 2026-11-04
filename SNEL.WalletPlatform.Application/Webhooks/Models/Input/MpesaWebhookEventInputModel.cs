using System.Text.Json.Serialization;

namespace SNEL.WalletPlatform.Application.Webhooks.Models.Input;

public sealed class MpesaWebhookEventInputModel
{
    [JsonPropertyName("input_OriginalConversationID")]
    public string? OriginalConversationId { get; init; }

    [JsonPropertyName("input_ThirdPartyConversationID")]
    public string? ThirdPartyConversationId { get; init; }

    [JsonPropertyName("input_TransactionID")]
    public string? TransactionId { get; init; }

    [JsonPropertyName("input_ResultCode")]
    public string? ResultCode { get; init; }

    [JsonPropertyName("input_ResultDesc")]
    public string? ResultDescription { get; init; }
}