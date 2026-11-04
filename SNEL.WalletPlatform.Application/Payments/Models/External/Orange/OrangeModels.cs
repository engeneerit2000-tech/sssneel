using System.Text.Json;
using System.Text.Json.Serialization;

namespace SNEL.WalletPlatform.Application.Payments.Models.External.Orange;

public sealed class OrangeTokenResponseModel
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; init; }
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; init; }
    [JsonPropertyName("token_type")]
    public string? TokenType { get; init; }
    [JsonPropertyName("scope")]
    public string? Scope { get; init; }
}

public sealed class OrangeWithdrawRequestModel
{
    [JsonPropertyName("peerId")]
    public string PeerId { get; init; } = string.Empty;
    [JsonPropertyName("peerIdType")]
    public string PeerIdType { get; init; } = "msisdn";
    [JsonPropertyName("amount")]
    public decimal Amount { get; init; }
    [JsonPropertyName("currency")]
    public string Currency { get; init; } = string.Empty;
    [JsonPropertyName("transactionId")]
    public string TransactionId { get; init; } = string.Empty;
}

public sealed class OrangeTransactionResponseModel
{
    [JsonPropertyName("status")]
    public string? Status { get; init; }
    [JsonPropertyName("message")]
    public string? Message { get; init; }
    [JsonPropertyName("transactionData")]
    public OrangeTransactionDataModel? TransactionData { get; init; }
}

public sealed class OrangeTransactionDataModel
{
    [JsonPropertyName("transactionId")]
    public string? TransactionId { get; init; }
    [JsonPropertyName("type")]
    public string? Type { get; init; }
    [JsonPropertyName("txnId")]
    public string? TxnId { get; init; }
    [JsonPropertyName("peerId")]
    public string? PeerId { get; init; }
    [JsonPropertyName("peerIdType")]
    public string? PeerIdType { get; init; }
    [JsonPropertyName("amount")]
    public decimal Amount { get; init; }
    [JsonPropertyName("currency")]
    public string? Currency { get; init; }
    [JsonPropertyName("posId")]
    public string? PosId { get; init; }
    [JsonPropertyName("executionDate")]
    public DateTimeOffset? ExecutionDate { get; init; }
    [JsonPropertyName("creationDate")]
    public DateTimeOffset? CreationDate { get; init; }
    [JsonPropertyName("country")]
    public string? Country { get; init; }
}

public sealed class OrangeErrorResponseModel
{
    [JsonPropertyName("code")]
    public JsonElement? Code { get; init; }
    [JsonPropertyName("message")]
    public string? Message { get; init; }
}
