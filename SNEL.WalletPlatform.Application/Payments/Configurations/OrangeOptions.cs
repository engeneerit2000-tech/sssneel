namespace SNEL.WalletPlatform.Application.Payments.Configurations;

public sealed class OrangeOptions
{
    public const string SectionName = "Orange";
    public string Environment { get; set; } = "sandbox";
    public string BaseUrl { get; set; } = string.Empty;
    public string TokenPath { get; set; } = "/oauth/v3/token";
    public string WithdrawPath { get; set; } = "/om_partner_api/v1/{country}/withdraw";
    public string StatusPath { get; set; } = "/om_partner_api/v1/{country}/withdraw/transactions/{transactionId}";
    public string BasicAuth { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public string SandboxPeerId { get; set; } = string.Empty;
    public string WebhookAuth { get; set; } = string.Empty;
}
