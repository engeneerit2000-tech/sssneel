namespace SNEL.WalletPlatform.Infrastructure.Reporting.PDF;

public class GotenbergOptions
{

    public const string SectionName = "GotenbergSharpClient";

    public string ServiceUrl { get; set; } = default!;
    public string HealthCheckUrl { get; set; } = default!;
}