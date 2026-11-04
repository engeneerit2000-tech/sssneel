using System.Text.Json.Serialization;

namespace SNEL.WalletPlatform.Application.Payments.Models.External.Afrimony;

public sealed class AfrimonyMerchantPayResponseModel
{
    [JsonPropertyName("txnStatus")]
    public string? TxnStatus { get; set; }

    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("serviceRequestId")]
    public string? ServiceRequestId { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("transactionId")]
    public string? TransactionId { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("serviceFlow")]
    public string? ServiceFlow { get; set; }

    [JsonPropertyName("errors")]
    public List<AfrimonyErrorModel>? Errors { get; set; }
}

public sealed class AfrimonyErrorModel
{
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}
