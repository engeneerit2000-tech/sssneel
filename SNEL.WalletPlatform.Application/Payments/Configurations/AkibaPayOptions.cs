namespace SNEL.WalletPlatform.Application.Payments.Configurations;

/// <summary>
/// Fallback when <c>PaymentMethods.Config</c> does not define management or <c>status_polling</c> (prefer DB config).
/// </summary>
public sealed class AkibaPayOptions
{
    public const string SectionName = "AkibaPay";

    public string ManagementBaseUrl { get; set; } = "https://management.brainstormtechs.com";

    public string Login { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public int StatusPollingInitialDelayMinutes { get; set; } = 10;

    public int StatusPollingAttempts { get; set; } = 10;

    public int StatusPollingIntervalSeconds { get; set; } = 60;
}
