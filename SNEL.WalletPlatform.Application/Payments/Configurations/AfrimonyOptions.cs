namespace SNEL.WalletPlatform.Application.Payments.Configurations;

/// <summary>
/// Fallback when <c>PaymentMethods.Config</c> does not define Afrimony fields or <c>status_polling</c> (prefer DB config).
/// </summary>
public sealed class AfrimonyOptions
{
    public const string SectionName = "Afrimony";

    public string BaseUrl { get; set; } = "https://api.afrimoney.cd";

    public string OAuthUrl { get; set; } = "https://api.afrimoney.cd/v1/oauth2/";

    public string OAuthBasic { get; set; } = string.Empty;

    public int StatusPollingInitialDelayMinutes { get; set; } = 10;

    public int StatusPollingAttempts { get; set; } = 10;

    public int StatusPollingIntervalSeconds { get; set; } = 60;
}
