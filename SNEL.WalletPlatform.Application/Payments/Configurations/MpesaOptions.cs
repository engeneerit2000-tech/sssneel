namespace SNEL.WalletPlatform.Application.Payments.Configurations;

public sealed class MpesaOptions
{
    public const string SectionName = "Mpesa";
    public string Environment { get; set; } = "sandbox";
    public string BaseUrl { get; set; } = "https://openapi.m-pesa.com";
    public string SessionPath { get; set; } = "/sandbox/ipg/v2/vodacomDRC/getSession/";
    public string PaymentPath { get; set; } = "/sandbox/ipg/v2/vodacomDRC/c2bPayment/singleStage/";
    public string StatusPath { get; set; } = "/sandbox/ipg/v2/vodacomDRC/queryTransactionStatus/";
    public string PublicKey { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ServiceProviderCode { get; set; } = string.Empty;
    public string Country { get; set; } = "DRC";
    public string Origin { get; set; } = "*";
    public int SessionActivationDelaySeconds { get; set; } = 30;
    public int SessionLifetimeMinutes { get; set; } = 15;
    public int StatusPollingInitialDelayMinutes { get; set; } = 1;
    public int StatusPollingAttempts { get; set; } = 10;
    public int StatusPollingIntervalSeconds { get; set; } = 60;
}
