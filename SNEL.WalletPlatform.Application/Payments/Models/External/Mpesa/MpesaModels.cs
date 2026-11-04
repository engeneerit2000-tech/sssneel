using System.Text.Json.Serialization;

namespace SNEL.WalletPlatform.Application.Payments.Models.External.Mpesa;

public sealed class MpesaSessionResponseModel
{
    [JsonPropertyName("output_SessionID")]
    public string? SessionId { get; set; }

    [JsonPropertyName("output_ResponseCode")]
    public string? ResponseCode { get; set; }

    [JsonPropertyName("output_ResponseDesc")]
    public string? ResponseDescription { get; set; }
}

public sealed class MpesaC2bPaymentRequestModel
{
    [JsonPropertyName("input_Amount")]
    public string Amount { get; init; } = string.Empty;
    [JsonPropertyName("input_Country")]
    public string Country { get; init; } = string.Empty;
    [JsonPropertyName("input_Currency")]
    public string Currency { get; init; } = string.Empty;
    [JsonPropertyName("input_CustomerMSISDN")]
    public string CustomerMsisdn { get; init; } = string.Empty;
    [JsonPropertyName("input_ServiceProviderCode")]
    public string ServiceProviderCode { get; init; } = string.Empty;
    [JsonPropertyName("input_ThirdPartyConversationID")]
    public string ThirdPartyConversationId { get; init; } = string.Empty;
    [JsonPropertyName("input_TransactionReference")]
    public string TransactionReference { get; init; } = string.Empty;
    [JsonPropertyName("input_PurchasedItemsDesc")]
    public string PurchasedItemsDescription { get; init; } = "Money";
}

public sealed class MpesaC2bPaymentResponseModel
{
    [JsonPropertyName("output_TransactionID")]
    public string? TransactionId { get; set; }
    [JsonPropertyName("output_ConversationID")]
    public string? ConversationId { get; set; }
    [JsonPropertyName("output_ResponseCode")]
    public string? ResponseCode { get; set; }
    [JsonPropertyName("output_ResponseDesc")]
    public string? ResponseDescription { get; set; }
    [JsonPropertyName("output_ThirdPartyConversationID")]
    public string? ThirdPartyConversationId { get; set; }
}

public sealed class MpesaStatusResponseModel
{
    [JsonPropertyName("output_ResponseCode")]
    public string? ResponseCode { get; set; }
    [JsonPropertyName("output_ResponseDesc")]
    public string? ResponseDescription { get; set; }
    [JsonPropertyName("output_TransactionStatus")]
    public string? TransactionStatus { get; set; }
    [JsonPropertyName("output_TransactionID")]
    public string? TransactionId { get; set; }
    [JsonPropertyName("output_ConversationID")]
    public string? ConversationId { get; set; }
    [JsonPropertyName("output_ThirdPartyConversationID")]
    public string? ThirdPartyConversationId { get; set; }
}
