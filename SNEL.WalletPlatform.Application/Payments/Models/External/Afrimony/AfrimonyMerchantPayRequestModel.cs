using System.Text.Json.Serialization;

namespace SNEL.WalletPlatform.Application.Payments.Models.External.Afrimony;

public sealed class AfrimonyMerchantPayRequestModel
{
    [JsonPropertyName("bearerCode")]
    public string BearerCode { get; init; } = string.Empty;

    [JsonPropertyName("transactionAmount")]
    public decimal TransactionAmount { get; init; }

    [JsonPropertyName("currency")]
    public string Currency { get; init; } = string.Empty;

    [JsonPropertyName("externalReferenceId")]
    public string ExternalReferenceId { get; init; } = string.Empty;

    [JsonPropertyName("initiator")]
    public string Initiator { get; init; } = string.Empty;

    [JsonPropertyName("transactor")]
    public AfrimonyPartyModel Transactor { get; init; } = new();

    [JsonPropertyName("sender")]
    public AfrimonyPartyModel Sender { get; init; } = new();
}

public sealed class AfrimonyPartyModel
{
    [JsonPropertyName("idType")]
    public string IdType { get; init; } = "mobileNumber";

    [JsonPropertyName("idValue")]
    public string IdValue { get; init; } = string.Empty;

    [JsonPropertyName("productId")]
    public string ProductId { get; init; } = string.Empty;

    [JsonPropertyName("mpin")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Mpin { get; init; }

    [JsonPropertyName("tpin")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Tpin { get; init; }
}
