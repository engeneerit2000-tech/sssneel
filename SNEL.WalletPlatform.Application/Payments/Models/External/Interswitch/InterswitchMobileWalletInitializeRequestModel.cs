using System.Text.Json.Serialization;

namespace SNEL.WalletPlatform.Application.Payments.Models.External.Interswitch;

/// <summary>
/// Request body for Interswitch collections mobile-wallet initialize (POST .../collections/api/v2/mobile-wallet/initialize).
/// </summary>
public sealed class InterswitchMobileWalletInitializeRequestModel
{
    [JsonPropertyName("order_id")]
    public string OrderId { get; init; } = string.Empty;

    [JsonPropertyName("order_currency")]
    public string OrderCurrency { get; init; } = string.Empty;

    [JsonPropertyName("customer_mobile_number")]
    public string CustomerMobileNumber { get; init; } = string.Empty;

    [JsonPropertyName("mobile_wallet_provider")]
    public string MobileWalletProvider { get; init; } = string.Empty;

    [JsonPropertyName("pay_item_id")]
    public string PayItemId { get; init; } = string.Empty;

    [JsonPropertyName("merchant_code")]
    public string MerchantCode { get; init; } = string.Empty;

    [JsonPropertyName("txn_ref")]
    public string TxnRef { get; init; } = string.Empty;

    /// <summary>ISO 4217 numeric currency code (e.g. 976 for CDF).</summary>
    [JsonPropertyName("currency")]
    public string Currency { get; init; } = string.Empty;

    /// <summary>Amount in minor units (e.g. cents), as a string.</summary>
    [JsonPropertyName("order_amount")]
    public string OrderAmount { get; init; } = string.Empty;

    [JsonPropertyName("merchant_country")]
    public string MerchantCountry { get; init; } = string.Empty;
}
