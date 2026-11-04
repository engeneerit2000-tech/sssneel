using System.Text.Json.Serialization;

namespace SNEL.WalletPlatform.Application.Payments.Models.External.Easypay;

public sealed class EasypayCollectionResponseModel
{
    [JsonPropertyName("success")]
    public int Success { get; init; }

    [JsonPropertyName("errormsg")]
    public string? ErrorMessage { get; init; }

    [JsonPropertyName("errorcode")]
    public int? ErrorCode { get; init; }

    [JsonPropertyName("data")]
    public EasypayCollectionResponseDataModel? Data { get; init; }
}

public sealed class EasypayCollectionResponseDataModel
{
    [JsonPropertyName("transactionId")]
    public string? TransactionId { get; init; }

    [JsonPropertyName("referenceId")]
    public string? ReferenceId { get; init; }

    [JsonPropertyName("status")]
    public string? Status { get; init; }

    [JsonPropertyName("amount")]
    public decimal Amount { get; init; }

    [JsonPropertyName("currency")]
    public string? Currency { get; init; }

    [JsonPropertyName("provider")]
    public string? Provider { get; init; }

    [JsonPropertyName("paymentUrl")]
    public string? PaymentUrl { get; init; }

    [JsonPropertyName("expiresAt")]
    public DateTimeOffset? ExpiresAt { get; init; }
}
