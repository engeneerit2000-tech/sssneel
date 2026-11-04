namespace SNEL.WalletPlatform.Infrastructure.Payments;

/// <summary>
/// Maps wallet currency abbreviations to Afrimony numeric currency codes.
/// </summary>
public static class AfrimonyCurrencyHelper
{
    public const string UsdCode = "101";
    public const string CdfCode = "102";

    public static bool TryMapCurrencyCode(string? currencyAbbreviation, out string afrimonyCurrencyCode)
    {
        afrimonyCurrencyCode = string.Empty;
        var normalized = (currencyAbbreviation ?? string.Empty).Trim().ToUpperInvariant();
        switch (normalized)
        {
            case "USD":
                afrimonyCurrencyCode = UsdCode;
                return true;
            case "CDF":
                afrimonyCurrencyCode = CdfCode;
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// Converts MSISDN to Afrimony local format: +243910100390 → 0910100390.
    /// Already-local numbers (starting with 0) are returned as digits only.
    /// </summary>
    public static string NormalizeFromNumber(string fromNumber)
    {
        var digits = new string(fromNumber.Trim().Where(char.IsDigit).ToArray());

        if (digits.StartsWith("243", StringComparison.Ordinal) && digits.Length > 3)
        {
            return "0" + digits[3..];
        }

        if (digits.StartsWith('0'))
        {
            return digits;
        }

        return digits.Length > 0 ? "0" + digits : digits;
    }
}
