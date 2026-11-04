using System.Text.Json.Serialization;

namespace SNEL.WalletPlatform.Application.Payments.Models.ConnectAfrica;

/// <summary>
/// One tax block from Connect Africa vend: tk43 (amount) + tk92 (name) + tk73 (rate).
/// tk92 distinguishes TVA vs TEP.
/// </summary>
public sealed class ConnectAfricaReceiptTaxLine
{
    [JsonPropertyName("amount")]
    public string? Amount { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("percentage")]
    public string? Percentage { get; init; }

    public bool IsTep =>
        ContainsTaxCode(Name, "TEP");

    public bool IsVat =>
        ContainsTaxCode(Name, "TVA");

    public bool IsPublicLighting =>
        !IsVat &&
        !IsTep &&
        ContainsTaxCode(Name, "Eclairage");

    private static bool ContainsTaxCode(string? name, string code) =>
        !string.IsNullOrWhiteSpace(name) &&
        name.Contains(code, StringComparison.OrdinalIgnoreCase);
}
