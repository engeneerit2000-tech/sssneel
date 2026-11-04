namespace SNEL.WalletPlatform.Infrastructure.Persistence.Seed;

public static class MpesaPaymentSeedData
{
    public const long PartnerId = 102L;
    public const long PaymentProviderId = 1004L;
    public const long PaymentMethodId = 1005L;
    public const long PaymentMethodCurrencyCdfId = 1009L;
    public const long PaymentMethodCurrencyUsdId = 1010L;
    public const string ProviderCode = "MPESA";

    public const string MethodCode = "C2B_PAYMENT";
    public const string MethodName = "MPesa C2B Payment";

    public static readonly Guid PspLiabilityCdfAccountId = Guid.Parse("c3d4e5f6-a7b8-4901-c234-5f6a7b8c9001");
    public static readonly Guid PspLiabilityUsdAccountId = Guid.Parse("c3d4e5f6-a7b8-4901-c234-5f6a7b8c9002");
    public static readonly Guid PspBankCdfAccountId = Guid.Parse("c3d4e5f6-a7b8-4901-c234-5f6a7b8c9003");
    public static readonly Guid PspBankUsdAccountId = Guid.Parse("c3d4e5f6-a7b8-4901-c234-5f6a7b8c9004");
    public static DateTimeOffset CreatedAtUtc => new(2026, 8, 18, 12, 0, 0, TimeSpan.Zero);

    public static string BuildPaymentMethodConfigJson() =>
        """
        {
          "fields": {
            "environment": "sandbox",
            "mpesa_base_url": "https://openapi.m-pesa.com",
            "session_path": "/sandbox/ipg/v2/vodacomDRC/getSession/",
            "payment_path": "/sandbox/ipg/v2/vodacomDRC/c2bPayment/singleStage/",
            "status_path": "/sandbox/ipg/v2/vodacomDRC/queryTransactionStatus/",
            "public_key": "MIICIjANBgkqhkiG9w0BAQEFAAOCAg8AMIICCgKCAgEArv9yxA69XQKBo24BaF/D+fvlqmGdYjqLQ5WtNBb5tquqGvAvG3WMFETVUSow/LizQalxj2ElMVrUmzu5mGGkxK08bWEXF7a1DEvtVJs6nppIlFJc2SnrU14AOrIrB28ogm58JjAl5BOQawOXD5dfSk7MaAA82pVHoIqEu0FxA8BOKU+RGTihRU+ptw1j4bsAJYiPbSX6i71gfPvwHPYamM0bfI4CmlsUUR3KvCG24rB6FNPcRBhM3jDuv8ae2kC33w9hEq8qNB55uw51vK7hyXoAa+U7IqP1y6nBdlN25gkxEA8yrsl1678cspeXr+3ciRyqoRgj9RD/ONbJhhxFvt1cLBh+qwK2eqISfBb06eRnNeC71oBokDm3zyCnkOtMDGl7IvnMfZfEPFCfg5QgJVk1msPpRvQxmEsrX9MQRyFVzgy2CWNIb7c+jPapyrNwoUbANlN8adU1m6yOuoX7F49x+OjiG2se0EJ6nafeKUXw/+hiJZvELUYgzKUtMAZVTNZfT8jjb58j8GVtuS+6TM2AutbejaCV84ZK58E2CRJqhmjQibEUO6KPdD7oTlEkFy52Y1uOOBXgYpqMzufNPmfdqqqSM4dU70PO8ogyKGiLAIxCetMjjm6FCMEA3Kc8K0Ig7/XtFm9By6VxTJK1Mg36TlHaZKP6VzVLXMtesJECAwEAAQ==",
            "api_key": "6EucRLzlyGXNhgEtf8bmBCPjTmZGKkTg",
            "service_provider_code": "000000",
            "country": "DRC",
            "origin": "*",
            "purchased_items_desc": "Money",
            "session_activation_delay_seconds": "30",
            "session_lifetime_minutes": "15"
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
