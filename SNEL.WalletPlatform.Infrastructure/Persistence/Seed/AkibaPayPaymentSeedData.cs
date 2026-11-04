namespace SNEL.WalletPlatform.Infrastructure.Persistence.Seed;

/// <summary>
/// Fixed identifiers for AkibaPay payment provider seed (migration <c>SeedAkibaPayPaymentProvider</c>).
/// </summary>
public static class AkibaPayPaymentSeedData
{
    public const long PartnerId = 100L;
    public const long PaymentProviderId = 1001L;
    public const long PaymentMethodId = 1001L;
    public const long PaymentMethodCurrencyCdfId = 1002L;
    public const long PaymentMethodCurrencyUsdId = 1003L;

    public const string ProviderCode = "AKIPA-PAY";
    public const string MethodCode = "CUSTOMER_DEPOSIT";
    public const string MethodName = "Customer Deposit";

    public const string AkibaPayGatewayUrl = "https://akibapay.brainstormtechs.com/api/gateway/";
    public const string MerchantId = "08de9200-5e9f-4660-8f54-327591d06a7e";
    public const string MerchantPass = "IbnjCLbL9Y31";

    public const string ManagementBaseUrl = "https://management.brainstormtechs.com";
    public const string ManagementLogin = "snel";
    public const string ManagementPassword = "8x1oM8sOxY8C";

    public const int StatusPollingInitialDelaySeconds = 600;
    public const int StatusPollingIntervalSeconds = 60;
    public const int StatusPollingTotalAttempts = 10;

    /// <summary>Public wallet API host (no trailing slash). Override per environment when applying migrations.</summary>
    public const string DefaultApiHost = "https://sneldev-walletplatform.fivectech.com";

    public const string StagingApiHost = "https://snelstg-walletplatform.fivectech.com";

    public static string DefaultCallbackUrl =>
        $"{DefaultApiHost.TrimEnd('/')}/api/v1/webhooks/akibapay";

    public static string StagingCallbackUrl =>
        $"{StagingApiHost.TrimEnd('/')}/api/v1/webhooks/akibapay";

    public const decimal CurrencyMinAmount = 1m;
    public const decimal CdfMaxAmount = 1_000_000m;
    public const decimal UsdMaxAmount = 100_000m;
    public const decimal FeesValue = 0m;
    public const decimal CostValue = 0m;
    public const decimal VatPercent = 0m;
    public const int AmountScale = 2;

    public static readonly Guid PspLiabilityCdfAccountId = Guid.Parse("a1b2c3d4-e5f6-4789-a012-3d4e5f6a7001");
    public static readonly Guid PspLiabilityUsdAccountId = Guid.Parse("a1b2c3d4-e5f6-4789-a012-3d4e5f6a7002");
    public static readonly Guid PspBankCdfAccountId = Guid.Parse("a1b2c3d4-e5f6-4789-a012-3d4e5f6a7003");
    public static readonly Guid PspBankUsdAccountId = Guid.Parse("a1b2c3d4-e5f6-4789-a012-3d4e5f6a7004");

    private static readonly DateTimeOffset SeedTimestamp =
        new(2026, 5, 17, 12, 0, 0, TimeSpan.Zero);

    public static DateTimeOffset CreatedAtUtc => SeedTimestamp;
    public static DateTimeOffset UpdatedAtUtc => SeedTimestamp;
    public const string AuditUser = "SYSTEM";

    /// <summary>
    /// Customer Deposit API config (JSONB). No <c>to_number</c> — not part of that AkibaPay flow.
    /// </summary>
    public static string BuildPaymentMethodConfigJson(string? callbackUrl = null) =>
        $$"""
        {
          "fields": {
            "akibapay_base_url": "{{AkibaPayGatewayUrl}}",
            "merchant_id": "{{MerchantId}}",
            "merchant_pass": "{{MerchantPass}}",
            "callback_url": "{{callbackUrl ?? DefaultCallbackUrl}}",
            "management_base_url": "{{ManagementBaseUrl}}",
            "management_login": "{{ManagementLogin}}",
            "management_password": "{{ManagementPassword}}"
          },
          "status_polling": {
            "total_attempts": {{StatusPollingTotalAttempts}},
            "initial_delay_seconds": {{StatusPollingInitialDelaySeconds}},
            "interval_seconds": {{StatusPollingIntervalSeconds}}
          }
        }
        """.Trim();
}
