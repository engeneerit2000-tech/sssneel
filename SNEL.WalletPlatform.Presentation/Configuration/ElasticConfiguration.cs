namespace SNEL.WalletPlatform.Presentation.Configuration;

public sealed class ElasticConfiguration
{
    public const string SectionName = "ElasticConfiguration";

    public string? Uri { get; init; }
    public string? Username { get; init; }
    public string? Password { get; init; }
    public string? IndexFormat { get; init; }
}
