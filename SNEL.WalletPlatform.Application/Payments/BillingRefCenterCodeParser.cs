namespace SNEL.WalletPlatform.Application.Payments;

internal static class BillingRefCenterCodeParser
{
    /// <summary>
    /// Extracts center code from billing ref (e.g. "06-03-634093337" → "06-03").
    /// Uses all segments before the last hyphen-separated segment.
    /// </summary>
    public static string? ExtractCenterCode(string? billingRef)
    {
        if (string.IsNullOrWhiteSpace(billingRef))
            return null;

        var trimmed = billingRef.Trim();
        var lastDashIndex = trimmed.LastIndexOf('-');
        if (lastDashIndex <= 0)
            return trimmed;

        return trimmed[..lastDashIndex];
    }

    /// <summary>
    /// Normalizes legacy region-only billing refs (e.g. "06-634093337" → "06-03-634093337")
    /// using the same rules as the NewCenters data migration.
    /// </summary>
    public static string NormalizeBillingRefForCenterMatch(string billingRef, IReadOnlyList<(string Code, string Name)> centers)
    {
        var trimmed = billingRef.Trim();
        if (trimmed.Split('-').Length != 3)          
        {
            return trimmed;
        }
        if(centers.Count > 0) {
            return centers.First().Code + trimmed[2..];
        }        
        return "06-03" + trimmed[2..];
    }

    public static string? ResolveCenterCode(
        string? billingRef,
        IReadOnlyList<(string Code, string Name)> centers)
    {
        if (string.IsNullOrWhiteSpace(billingRef) || centers.Count == 0)
            return null;

        var trimmed = NormalizeBillingRefForCenterMatch(billingRef, centers);
        var prefixMatch = centers
            .Where(c => trimmed.StartsWith(c.Code + "-", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(trimmed, c.Code, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(c => c.Code.Length)
            .FirstOrDefault();

        if (prefixMatch.Code is not null)
            return prefixMatch.Code;

        var parsedCode = ExtractCenterCode(trimmed);
        if (parsedCode is null)
            return null;

        return centers.Any(c => string.Equals(c.Code, parsedCode, StringComparison.OrdinalIgnoreCase))
            ? parsedCode
            : null;
    }

    public static string? ResolveCenterName(
        string? billingRef,
        IReadOnlyList<(string Code, string Name)> centers)
    {
        var centerCode = ResolveCenterCode(billingRef, centers);
        if (centerCode is null)
            return null;

        return centers
            .FirstOrDefault(c => string.Equals(c.Code, centerCode, StringComparison.OrdinalIgnoreCase))
            .Name;
    }
}
