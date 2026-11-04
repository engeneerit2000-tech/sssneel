namespace SNEL.WalletPlatform.Application.Payments.Configurations;

/// <summary>
/// Fallback when <c>PaymentMethods.Config</c> does not define management or <c>status_polling</c> (prefer DB config).
/// </summary>
public sealed class EasypayOptions
{
    public const string SectionName = "Easypay";

    public string ManagementBaseUrl { get; set; } = "https://www.easypay-gateway.com/payments/api/v1/";

    public string Apikey { get; set; } = string.Empty;
    
    public string WebhookAuth { get; set; } = string.Empty;

    public int StatusPollingInitialDelayMinutes { get; set; } = 10;

    public int StatusPollingAttempts { get; set; } = 10;

    public int StatusPollingIntervalSeconds { get; set; } = 60;
}
