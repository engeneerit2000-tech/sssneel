using System.Text.Json.Serialization;

namespace SNEL.WalletPlatform.Application.Payments.Models.External.Easypay;

public sealed class EasypayCustomerDepositRequestModel
{
    [JsonPropertyName("referenceId")]
    public string ReferenceId { get; init; } = string.Empty;

    [JsonPropertyName("phone")]
    public string Phone { get; init; } = string.Empty;

    /// <summary>ISO currency abbreviation.</summary>
    [JsonPropertyName("currency")]
    public string Currency { get; init; } = string.Empty;    

    [JsonPropertyName("amount")]
    public decimal Amount { get; init; }    
}

