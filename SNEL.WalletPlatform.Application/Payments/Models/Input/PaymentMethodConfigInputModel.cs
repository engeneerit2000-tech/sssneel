using System.Text.Json.Serialization;

namespace SNEL.WalletPlatform.Application.Payments.Models.Input;

public sealed record PaymentMethodConfigInputModel(
    /// <summary>
    /// PSP-specific configuration fields (example: Interswitch).
    /// </summary>
    [property: JsonPropertyName("fields")] PaymentMethodFieldsConfigInputModel? Fields,

    /// <summary>
    /// Optional checkout type (example: "inline").
    /// </summary>
    [property: JsonPropertyName("checkoutType")] string? CheckoutType,

    /// <summary>
    /// Confirmation UX configuration (example: SNEL Wallet).
    /// </summary>
    [property: JsonPropertyName("confirmation")] PaymentMethodConfirmationConfigInputModel? Confirmation,

    /// <summary>
    /// Status polling schedule (Quartz POLLING mode or AkibaPay webhook fallback). All delays in seconds.
    /// </summary>
    [property: JsonPropertyName("status_polling")] PaymentMethodStatusPollingConfigInputModel? StatusPolling);

public sealed record PaymentMethodStatusPollingConfigInputModel(
    [property: JsonPropertyName("total_attempts")] int? TotalAttempts,
    [property: JsonPropertyName("initial_delay_seconds")] int? InitialDelaySeconds,
    [property: JsonPropertyName("initial_delay_minutes")] int? InitialDelayMinutes,
    [property: JsonPropertyName("interval_seconds")] int? IntervalSeconds);

public sealed record PaymentMethodFieldsConfigInputModel(
    [property: JsonPropertyName("mode")] string? Mode,
    [property: JsonPropertyName("init_url")] string? InitUrl,
    [property: JsonPropertyName("client_id")] string? ClientId,
    [property: JsonPropertyName("secret_key")] string? SecretKey,
    [property: JsonPropertyName("pay_item_id")] string? PayItemId,
    [property: JsonPropertyName("pay_item_name")] string? PayItemName,
    [property: JsonPropertyName("merchant_code")] string? MerchantCode,
    [property: JsonPropertyName("site_redirect_url")] string? SiteRedirectUrl,
    [property: JsonPropertyName("interswitch_base_url")] string? InterswitchBaseUrl,
    [property: JsonPropertyName("management_base_url")] string? ManagementBaseUrl,
    [property: JsonPropertyName("management_login")] string? ManagementLogin,
    [property: JsonPropertyName("management_password")] string? ManagementPassword);

public sealed record PaymentMethodConfirmationConfigInputModel(
    [property: JsonPropertyName("message")] PaymentMethodConfirmationMessageConfigInputModel? Message);

public sealed record PaymentMethodConfirmationMessageConfigInputModel(
    [property: JsonPropertyName("en")] string? En,
    [property: JsonPropertyName("fr")] string? Fr);
