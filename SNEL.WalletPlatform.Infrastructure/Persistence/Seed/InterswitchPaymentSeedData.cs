namespace SNEL.WalletPlatform.Infrastructure.Persistence.Seed;

/// <summary>
/// Defaults for Interswitch Quartz status polling (<c>PaymentMethods.Config.status_polling</c>).
/// </summary>
public static class InterswitchPaymentSeedData
{
    public const string ProviderCode = "INTERSWITCH";

    public const int DefaultTotalAttempts = 10;
    public const int DefaultInitialDelaySeconds = 60;
    public const int DefaultIntervalSeconds = 15;

    public const string DefaultStatusPollingJson =
        """
        {"total_attempts":10,"initial_delay_seconds":60,"interval_seconds":15}
        """;
}
