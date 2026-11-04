namespace SNEL.WalletPlatform.Infrastructure.Persistence.Seed;

public static class OrangePaymentSeedData
{
    public const long PartnerId = 103L;
    public const long PaymentProviderId = 1005L;
    public const long PaymentMethodId = 1006L;
    public const long PaymentMethodCurrencyCdfId = 1011L;
    public const long PaymentMethodCurrencyUsdId = 1012L;
    public const string ProviderCode = "ORANGE";
    public const string MethodCode = "CASHOUT";
    public const string MethodName = "Orange Money Cashout";

    public static readonly Guid PspLiabilityCdfAccountId = Guid.Parse("d4e5f6a7-b8c9-4012-d345-6a7b8c9d1001");
    public static readonly Guid PspLiabilityUsdAccountId = Guid.Parse("d4e5f6a7-b8c9-4012-d345-6a7b8c9d1002");
    public static readonly Guid PspBankCdfAccountId = Guid.Parse("d4e5f6a7-b8c9-4012-d345-6a7b8c9d1003");
    public static readonly Guid PspBankUsdAccountId = Guid.Parse("d4e5f6a7-b8c9-4012-d345-6a7b8c9d1004");
    public static DateTimeOffset CreatedAtUtc => new(2026, 8, 22, 0, 0, 0, TimeSpan.Zero);

    public static string BuildPaymentMethodConfigJson() =>
        """
        {
          "fields": {
            "environment": "sandbox",
            "orange_base_url": "https://api.orange.com",
            "token_path": "/oauth/v3/token",
            "withdraw_path": "/om_partner_api/v1/{country}/withdraw",
            "status_path": "/om_partner_api/v1/{country}/withdraw/transactions/{transactionId}",
            "basic_auth": "UTZ3M005TjY2cTFkM01RNGVXSVZNNHBiSU85T3dPbWw6blZJN2FoMW52MVRqemNod3FaQ3MxeldRMVYyaGRFd3pTR09WZmI5Rk14SVo=",
            "country": "sx",
            "currency": "OUV",
            "sandbox_peer_id": "7704100023",
            "webhook_auth": "b3JhbmdlOlBAc3N3MHJk"
          },
          "status_polling": {
            "total_attempts": 10,
            "initial_delay_seconds": 60,
            "interval_seconds": 60,
            "not_found_fail_after_attempts": 3
          }
        }
        """.Trim();
}
