using System.Text.Json.Serialization;
using SNEL.WalletPlatform.Application.Payments.Models.External.Orange;

namespace SNEL.WalletPlatform.Application.Webhooks.Models.Input;

public sealed class OrangeWebhookEventInputModel
{
    [JsonPropertyName("status")]
    public string? Status { get; init; }
    [JsonPropertyName("message")]
    public string? Message { get; init; }
    [JsonPropertyName("transactionData")]
    public OrangeTransactionDataModel? TransactionData { get; init; }
}
