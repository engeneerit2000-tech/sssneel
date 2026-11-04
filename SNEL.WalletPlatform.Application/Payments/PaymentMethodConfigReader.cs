using System.Text.Json;
using SNEL.WalletPlatform.Application.Jobs;
using SNEL.WalletPlatform.Application.Payments.Configurations;

namespace SNEL.WalletPlatform.Application.Payments;

/// <summary>
/// Reads shared <c>PaymentMethods.Config</c> sections (<c>fields</c>, <c>status_polling</c>) for any PSP.
/// </summary>
public static class PaymentMethodConfigReader
{
    public const string FieldsPropertyName = "fields";
    public const string StatusPollingPropertyName = "status_polling";

    public const string TotalAttemptsPropertyName = "total_attempts";
    public const string InitialDelaySecondsPropertyName = "initial_delay_seconds";
    public const string InitialDelayMinutesPropertyName = "initial_delay_minutes";
    public const string IntervalSecondsPropertyName = "interval_seconds";
    public const string NotFoundFailAfterAttemptsPropertyName = "not_found_fail_after_attempts";
    public const int DefaultNotFoundFailAfterAttempts = 3;

    public const string ManagementBaseUrlFieldName = "management_base_url";
    public const string ManagementLoginFieldName = "management_login";
    public const string ManagementPasswordFieldName = "management_password";
    
    public sealed record StatusPollingSchedule(
        int TotalAttempts,
        int InitialDelaySeconds,
        int IntervalSeconds,
        int NotFoundFailAfterAttempts = PaymentMethodConfigReader.DefaultNotFoundFailAfterAttempts)
    {
        public static StatusPollingSchedule QuartzDefaults { get; } = new(
            PaymentStatusPollingJobConstants.DefaultTotalAttempts,
            PaymentStatusPollingJobConstants.DefaultInitialDelaySeconds,
            PaymentStatusPollingJobConstants.DefaultIntervalSeconds);

        public static StatusPollingSchedule FromAkibaPayOptions(AkibaPayOptions options) => new(
            options.StatusPollingAttempts,
            options.StatusPollingInitialDelayMinutes * 60,
            options.StatusPollingIntervalSeconds);
        public static StatusPollingSchedule FromEasypayOptions(EasypayOptions options) => new(
            options.StatusPollingAttempts,
            options.StatusPollingInitialDelayMinutes * 60,
            options.StatusPollingIntervalSeconds);

        public static StatusPollingSchedule FromAfrimonyOptions(AfrimonyOptions options) => new(
            options.StatusPollingAttempts,
            options.StatusPollingInitialDelayMinutes * 60,
            options.StatusPollingIntervalSeconds);
        public static StatusPollingSchedule FromMpesaOptions(MpesaOptions options) => new(
            options.StatusPollingAttempts,
            options.StatusPollingInitialDelayMinutes * 60,
            options.StatusPollingIntervalSeconds);
    }

    public sealed record AkibaPayManagementCredentials(
        string ManagementBaseUrl,
        string Login,
        string Password);
    public sealed record EasypayManagementCredentials(
        string ManagementBaseUrl,
        string apiKey,
        string webhookAuth);
    public static IReadOnlyDictionary<string, string> GetFields(string? paymentMethodConfigJson)
    {
        if (string.IsNullOrWhiteSpace(paymentMethodConfigJson))
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        using var document = JsonDocument.Parse(paymentMethodConfigJson);
        var root = document.RootElement;

        if (!root.TryGetProperty(FieldsPropertyName, out var fieldsElement) ||
            fieldsElement.ValueKind != JsonValueKind.Object)
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        var configuredFields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var property in fieldsElement.EnumerateObject())
        {
            configuredFields[property.Name] = property.Value.GetString() ?? string.Empty;
        }

        return configuredFields;
    }

    public static bool TryGetField(
        string? paymentMethodConfigJson,
        string fieldName,
        out string? value)
    {
        value = null;
        var fields = GetFields(paymentMethodConfigJson);
        if (!fields.TryGetValue(fieldName, out var raw) || string.IsNullOrWhiteSpace(raw))
        {
            return false;
        }

        value = raw.Trim();
        return true;
    }

    public static StatusPollingSchedule ResolveStatusPolling(
        string? paymentMethodConfigJson,
        StatusPollingSchedule? fallback = null)
    {
        fallback ??= StatusPollingSchedule.QuartzDefaults;
        return TryParseStatusPolling(paymentMethodConfigJson, out var settings) ? settings : fallback;
    }

    public static bool TryParseStatusPolling(
        string? paymentMethodConfigJson,
        out StatusPollingSchedule settings)
    {
        settings = StatusPollingSchedule.QuartzDefaults;

        if (string.IsNullOrWhiteSpace(paymentMethodConfigJson))
        {
            return false;
        }

        try
        {
            using var document = JsonDocument.Parse(paymentMethodConfigJson);
            var root = document.RootElement;

            if (!root.TryGetProperty(StatusPollingPropertyName, out var statusPolling) ||
                statusPolling.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            var totalAttempts = ReadPositiveInt(
                statusPolling,
                TotalAttemptsPropertyName,
                StatusPollingSchedule.QuartzDefaults.TotalAttempts);
            var initialDelaySeconds = ReadInitialDelaySeconds(statusPolling);
            var intervalSeconds = ReadPositiveInt(
                statusPolling,
                IntervalSecondsPropertyName,
                StatusPollingSchedule.QuartzDefaults.IntervalSeconds);
            var notFoundFailAfterAttempts = ReadPositiveInt(
                statusPolling,
                NotFoundFailAfterAttemptsPropertyName,
                DefaultNotFoundFailAfterAttempts);

            settings = new StatusPollingSchedule(
                totalAttempts,
                initialDelaySeconds,
                intervalSeconds,
                notFoundFailAfterAttempts);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    public static AkibaPayManagementCredentials ResolveAkibaPayManagementCredentials(
        string? paymentMethodConfigJson,
        AkibaPayOptions? fallbackOptions = null)
    {
        var fields = GetFields(paymentMethodConfigJson);

        var baseUrl = GetFieldValue(fields, ManagementBaseUrlFieldName)
            ?? fallbackOptions?.ManagementBaseUrl?.Trim()
            ?? string.Empty;
        var login = GetFieldValue(fields, ManagementLoginFieldName)
            ?? fallbackOptions?.Login?.Trim()
            ?? string.Empty;
        var password = GetFieldValue(fields, ManagementPasswordFieldName)
            ?? fallbackOptions?.Password
            ?? string.Empty;

        return new AkibaPayManagementCredentials(baseUrl, login, password);
    }

    public static EasypayManagementCredentials ResolveEasypayManagementCredentials(
        string? paymentMethodConfigJson,
        EasypayOptions? fallbackOptions = null)
    {
        var fields = GetFields(paymentMethodConfigJson);

        var baseUrl = GetFieldValue(fields, "easypay_base_url")
            ?? fallbackOptions?.ManagementBaseUrl?.Trim()
            ?? string.Empty;
        var apiKey = GetFieldValue(fields, "api_key")
            ?? fallbackOptions?.Apikey?.Trim()
            ?? string.Empty;
        var webhookAuth = GetFieldValue(fields, "webhook_auth")
            ?? fallbackOptions?.WebhookAuth?.Trim()
            ?? string.Empty;
        return new EasypayManagementCredentials(baseUrl, apiKey, webhookAuth);
    }

    public static AfrimonyCredentials ResolveAfrimonyCredentials(
        string? paymentMethodConfigJson,
        AfrimonyOptions? fallbackOptions = null)
    {
        var fields = GetFields(paymentMethodConfigJson);

        var baseUrl = GetFieldValue(fields, "afrimony_base_url")
            ?? fallbackOptions?.BaseUrl?.Trim()
            ?? string.Empty;
        var oauthUrl = GetFieldValue(fields, "oauth_url")
            ?? fallbackOptions?.OAuthUrl?.Trim()
            ?? string.Empty;
        var oauthBasic = GetFieldValue(fields, "oauth_basic")
            ?? fallbackOptions?.OAuthBasic?.Trim()
            ?? string.Empty;

        return new AfrimonyCredentials(baseUrl, oauthUrl, oauthBasic);
    }

    public sealed record AfrimonyCredentials(
        string BaseUrl,
        string OAuthUrl,
        string OAuthBasic);

    public static MpesaCredentials ResolveMpesaCredentials(
        string? paymentMethodConfigJson,
        MpesaOptions? fallbackOptions = null)
    {
        var fields = GetFields(paymentMethodConfigJson);
        return new MpesaCredentials(
            GetFieldValue(fields, "environment") ?? fallbackOptions?.Environment?.Trim() ?? string.Empty, 
            GetFieldValue(fields, "mpesa_base_url") ?? fallbackOptions?.BaseUrl?.Trim() ?? string.Empty,
            GetFieldValue(fields, "session_path") ?? fallbackOptions?.SessionPath?.Trim() ?? string.Empty,
            GetFieldValue(fields, "payment_path") ?? fallbackOptions?.PaymentPath?.Trim() ?? string.Empty,
            GetFieldValue(fields, "status_path") ?? fallbackOptions?.StatusPath?.Trim() ?? string.Empty,
            GetFieldValue(fields, "public_key") ?? fallbackOptions?.PublicKey?.Trim() ?? string.Empty,
            GetFieldValue(fields, "api_key") ?? fallbackOptions?.ApiKey?.Trim() ?? string.Empty,
            GetFieldValue(fields, "service_provider_code") ?? fallbackOptions?.ServiceProviderCode?.Trim() ?? string.Empty,
            GetFieldValue(fields, "country") ?? fallbackOptions?.Country?.Trim() ?? "DRC",
            GetFieldValue(fields, "origin") ?? fallbackOptions?.Origin?.Trim() ?? "*",
            GetFieldValue(fields, "purchased_items_desc") ?? "Money",
            ReadConfiguredInt(fields, "session_activation_delay_seconds", fallbackOptions?.SessionActivationDelaySeconds ?? 30),
            ReadConfiguredInt(fields, "session_lifetime_minutes", fallbackOptions?.SessionLifetimeMinutes ?? 15));
    }

    public sealed record MpesaCredentials(
        string Environment,
        string BaseUrl,
        string SessionPath,
        string PaymentPath,
        string StatusPath,
        string PublicKey,
        string ApiKey,
        string ServiceProviderCode,
        string Country,
        string Origin,
        string PurchasedItemsDescription,
        int SessionActivationDelaySeconds,
        int SessionLifetimeMinutes);

    public static OrangeCredentials ResolveOrangeCredentials(
        string? paymentMethodConfigJson,
        OrangeOptions? fallbackOptions = null)
    {
        var fields = GetFields(paymentMethodConfigJson);
        return new OrangeCredentials(
            GetFieldValue(fields, "environment") ?? fallbackOptions?.Environment?.Trim() ?? "sandbox",
            GetFieldValue(fields, "orange_base_url") ?? fallbackOptions?.BaseUrl?.Trim() ?? string.Empty,
            GetFieldValue(fields, "token_path") ?? fallbackOptions?.TokenPath?.Trim() ?? "/oauth/v3/token",
            GetFieldValue(fields, "withdraw_path") ?? fallbackOptions?.WithdrawPath?.Trim() ?? "/om_partner_api/v1/{country}/withdraw",
            GetFieldValue(fields, "status_path") ?? fallbackOptions?.StatusPath?.Trim() ?? "/om_partner_api/v1/{country}/withdraw/transactions/{transactionId}",
            GetFieldValue(fields, "basic_auth") ?? fallbackOptions?.BasicAuth?.Trim() ?? string.Empty,
            GetFieldValue(fields, "country") ?? fallbackOptions?.Country?.Trim() ?? string.Empty,
            GetFieldValue(fields, "currency") ?? fallbackOptions?.Currency?.Trim() ?? string.Empty,
            GetFieldValue(fields, "sandbox_peer_id") ?? fallbackOptions?.SandboxPeerId?.Trim() ?? string.Empty,
            GetFieldValue(fields, "webhook_auth") ?? fallbackOptions?.WebhookAuth?.Trim() ?? string.Empty);
    }

    public sealed record OrangeCredentials(
        string Environment,
        string BaseUrl,
        string TokenPath,
        string WithdrawPath,
        string StatusPath,
        string BasicAuth,
        string Country,
        string Currency,
        string SandboxPeerId,
        string WebhookAuth);

    private static int ReadConfiguredInt(
        IReadOnlyDictionary<string, string> fields,
        string key,
        int fallback) =>
        fields.TryGetValue(key, out var value) && int.TryParse(value, out var parsed) && parsed >= 0
            ? parsed
            : fallback;

    private static int ReadInitialDelaySeconds(JsonElement statusPolling)
    {
        if (statusPolling.TryGetProperty(InitialDelaySecondsPropertyName, out var secondsElement) &&
            secondsElement.ValueKind == JsonValueKind.Number &&
            secondsElement.TryGetInt32(out var seconds) &&
            seconds > 0)
        {
            return seconds;
        }

        if (statusPolling.TryGetProperty(InitialDelayMinutesPropertyName, out var minutesElement) &&
            minutesElement.ValueKind == JsonValueKind.Number &&
            minutesElement.TryGetInt32(out var minutes) &&
            minutes > 0)
        {
            return minutes * 60;
        }

        return StatusPollingSchedule.QuartzDefaults.InitialDelaySeconds;
    }

    private static string? GetFieldValue(IReadOnlyDictionary<string, string> fields, string fieldName)
    {
        if (!fields.TryGetValue(fieldName, out var value) || string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }

    private static int ReadPositiveInt(JsonElement parent, string propertyName, int fallback)
    {
        if (!parent.TryGetProperty(propertyName, out var value) || value.ValueKind != JsonValueKind.Number)
        {
            return fallback;
        }

        if (!value.TryGetInt32(out var parsed) || parsed <= 0)
        {
            return fallback;
        }

        return parsed;
    }
}
