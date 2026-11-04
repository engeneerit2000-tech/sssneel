namespace SNEL.WalletPlatform.Infrastructure.Persistence.Seed;

/// <summary>
/// Fixed identifiers and configuration for the EasyPay payment-provider seed.
/// Keep these identifiers immutable because migrations reference them.
/// </summary>
public static class EasypayPaymentSeedData
{
    public const long PartnerId = 1016L;
    public const long PaymentProviderId = 1003L;
    public const long PaymentMethodId = 1003L;
    public const long PaymentMethodCurrencyCdfId = 1005L;
    public const long PaymentMethodCurrencyUsdId = 1006L;
    public const long CardPaymentMethodId = 1007L;
    public const long CardPaymentMethodCurrencyUsdId = 1013L;
    public const long CardPaymentMethodCurrencyCdfId = 1014L;
    public const int PaymentProviderRouterId = 1001;

    public const string ProviderCode = "EASYPAY";
    public const string MethodCode = "COLLECTIONS";
    public const string MethodName = "Collections";
    public const string CardMethodCode = "CARD_COLLECTIONS";
    public const string CardMethodName = "Card Collections";
    public const string CountryCode = "243";
    public const string CompanyPrefix = "22";

    public const string BaseUrl = "https://www.easypay-gateway.com/payments/api/v1/";
    public const string ApiKey = "dAFvNUlBK2pLg8cmTgp7/+maPAHyW6a+a8mIpsWawH+HN4lNDn2YE0zg8z0Yjc3u1LssE8qNg5fl2uByHzR/YffD4MTh";
    public const string ApiKey_Card = "qHHlxdDlnD3I1pQI67R9DqsWsCA5jQwtThuv0+FbUGmkNgV3OtL7r7kre2COR/FL";
    public const string WebhookAuth = "dGVzdDp0ZXN0";

    public const int StatusPollingInitialDelaySeconds = 600;
    public const int StatusPollingIntervalSeconds = 60;
    public const int StatusPollingTotalAttempts = 10;

    public const decimal CdfMinAmount = 0m;
    public const decimal UsdMinAmount = 0m;
    public const decimal CardCdfMinAmount = 1m;
    public const decimal CardUsdMinAmount = 1m;
    public const decimal CdfMaxAmount = 1_000_000m;
    public const decimal UsdMaxAmount = 1_000m;
    public const decimal FeesValue = 0m;
    public const decimal CostValue = 0m;
    public const decimal VatPercent = 0m;
    public const int AmountScale = 2;

    public static readonly Guid PspLiabilityCdfAccountId =
        Guid.Parse("a1b2c3d4-e5f6-4789-a012-3d4e5f6a7007");
    public static readonly Guid PspLiabilityUsdAccountId =
        Guid.Parse("a1b2c3d4-e5f6-4789-a012-3d4e5f6a7005");
    public static readonly Guid PspBankCdfAccountId =
        Guid.Parse("a1b2c3d4-e5f6-4789-a012-3d4e5f6a7008");
    public static readonly Guid PspBankUsdAccountId =
        Guid.Parse("a1b2c3d4-e5f6-4789-a012-3d4e5f6a7006");

    private static readonly DateTimeOffset SeedTimestamp =
        new(2026, 6, 12, 0, 16, 0, TimeSpan.Zero);

    public static DateTimeOffset CreatedAtUtc => SeedTimestamp;

    public static string BuildPaymentMethodConfigJson() =>
        $$"""
        {
          "fields": {
            "easypay_base_url": "{{BaseUrl}}",
            "api_key": "{{ApiKey}}",
            "webhook_auth": "{{WebhookAuth}}"
          },
          "status_polling": {
            "total_attempts": {{StatusPollingTotalAttempts}},
            "initial_delay_seconds": {{StatusPollingInitialDelaySeconds}},
            "interval_seconds": {{StatusPollingIntervalSeconds}}
          }
        }
        """.Trim();

    /// <summary>
    /// Card Collections uses a separate EasyPay application and API key. The method is seeded inactive;
    /// operators must replace the empty credentials and activate it after creating the card application.
    /// </summary>
    public static string BuildCardPaymentMethodConfigJson() =>
        $$"""
        {
          "fields": {
            "easypay_base_url": "{{BaseUrl}}",
            "api_key": "{{ApiKey_Card}}",
            "webhook_auth": "{{WebhookAuth}}"
          },
          "status_polling": {
            "total_attempts": {{StatusPollingTotalAttempts}},
            "initial_delay_seconds": {{StatusPollingInitialDelaySeconds}},
            "interval_seconds": {{StatusPollingIntervalSeconds}}
          }
        }
        """.Trim();
}
