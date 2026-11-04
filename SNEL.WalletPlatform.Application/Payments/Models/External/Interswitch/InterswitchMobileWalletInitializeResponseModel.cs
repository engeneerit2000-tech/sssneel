using System.Text.Json.Serialization;

namespace SNEL.WalletPlatform.Application.Payments.Models.External.Interswitch;

/// <summary>
/// Parsed subset of Interswitch mobile-wallet initialize response (fields may vary by environment).
/// </summary>
public sealed class InterswitchMobileWalletInitializeResponseModel
{
    [JsonPropertyName("responseCode")]
    public string? ResponseCode { get; init; }

    [JsonPropertyName("responseMessage")]
    public string? ResponseMessage { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }
}
