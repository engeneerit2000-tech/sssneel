namespace SNEL.WalletPlatform.Infrastructure.Persistence.Seed;

/// <summary>
/// Fixed identifiers for Afrimony payment provider seed.
/// </summary>
public static class AfrimonyPaymentSeedData
{
    public const long PartnerId = 101L;
    public const long PaymentProviderId = 1002L;
    public const long PaymentMethodId = 1004L;
    public const long PaymentMethodCurrencyCdfId = 1007L;
    public const long PaymentMethodCurrencyUsdId = 1008L;

    public const string ProviderCode = "AFRIMONEY";
    public const string MethodCode = "MERCHANT_PAY";
    public const string MethodName = "Merchant Pay";

    public const string AfrimonyBaseUrl = "https://api.afrimoney.cd";
    public const string OAuthUrl = "https://api.afrimoney.cd/v1/oauth2/";
    /// <summary>Base64 client credentials from sandbox Postman collection (without "Basic " prefix).</summary>
    public const string OAuthBasic = "c2U3OE9JcnB4cEVPeDhDRHdBcjBQY3pnb213YTp6ejN0bEhhWndjdnZtM1JPZHVDanhyQk95ZlFh";

    public const string IdValue = "0902835324";
    public const string Mpin = "6721";
    public const string ProductId = "12";
    public const string Initiator = "transactor";
    public const string BearerCode = "USSD";

    public const int StatusPollingInitialDelaySeconds = 600;
    public const int StatusPollingIntervalSeconds = 60;
    public const int StatusPollingTotalAttempts = 10;

    public const decimal CurrencyMinAmount = 1m;
    public const decimal UsdMinAmount = 0.1m;
    public const decimal CdfMaxAmount = 1_000_000m;
    public const decimal UsdMaxAmount = 100_000m;
    public const decimal FeesValue = 0m;
    public const decimal CostValue = 0m;
    public const decimal VatPercent = 0m;
    public const int AmountScale = 2;

    public static readonly Guid PspLiabilityCdfAccountId = Guid.Parse("b2c3d4e5-f6a7-4890-b123-4e5f6a7b8001");
    public static readonly Guid PspLiabilityUsdAccountId = Guid.Parse("b2c3d4e5-f6a7-4890-b123-4e5f6a7b8002");
    public static readonly Guid PspBankCdfAccountId = Guid.Parse("b2c3d4e5-f6a7-4890-b123-4e5f6a7b8003");
    public static readonly Guid PspBankUsdAccountId = Guid.Parse("b2c3d4e5-f6a7-4890-b123-4e5f6a7b8004");

    private static readonly DateTimeOffset SeedTimestamp =
        new(2026, 7, 29, 12, 0, 0, TimeSpan.Zero);

    public static DateTimeOffset CreatedAtUtc => SeedTimestamp;
    public static DateTimeOffset UpdatedAtUtc => SeedTimestamp;

    public static string BuildPaymentMethodConfigJson() =>
        $$"""
        {
          "fields": {
            "afrimony_base_url": "{{AfrimonyBaseUrl}}",
            "oauth_url": "{{OAuthUrl}}",
            "oauth_basic": "{{OAuthBasic}}",
            "id_value": "{{IdValue}}",
            "mpin": "{{Mpin}}",
            "product_id": "{{ProductId}}",
            "initiator": "{{Initiator}}",
            "bearer_code": "{{BearerCode}}"
          },
          "status_polling": {
            "total_attempts": {{StatusPollingTotalAttempts}},
            "initial_delay_seconds": {{StatusPollingInitialDelaySeconds}},
            "interval_seconds": {{StatusPollingIntervalSeconds}}
          }
        }
        """.Trim();
}
