using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using SNEL.WalletPlatform.Infrastructure.Persistence.Seed;

namespace SNEL.WalletPlatform.Infrastructure.Payments;

/// <summary>
/// AkibaPay request integrity hash (camelCase query + hex HMAC + base64 of hex string).
/// Matches AkibaPay PHP: <c>http_build_query(..., PHP_QUERY_RFC3986)</c>,
/// <c>hash_hmac('sha512', ..., merchantId, false)</c>, <c>base64_encode($hex)</c>.
/// </summary>
public static class AkibaPaySignatureHelper
{
    public static (string Hash, string SignQueryString) ComputeCustomerDepositHash(
        string merchantId,
        string merchantPass,
        string fromNumber,
        decimal amount,
        string currency,
        string callbackUrl,
        string merchantReference,
        int amountScale = AkibaPayPaymentSeedData.AmountScale)
    {
        var normalizedAmount = NormalizeAmount(amount, amountScale);

        var signQueryString = BuildCustomerDepositSignQueryString(
            merchantId,
            merchantPass,
            fromNumber,
            normalizedAmount,
            currency,
            callbackUrl,
            merchantReference,
            amountScale);

        var hex = ComputeHmacSha512HexLower(signQueryString, merchantId);
        var hash = Convert.ToBase64String(Encoding.UTF8.GetBytes(hex));
        return (hash, signQueryString);
    }

    public static string BuildCustomerDepositSignQueryString(
        string merchantId,
        string merchantPass,
        string fromNumber,
        decimal amount,
        string currency,
        string callbackUrl,
        string merchantReference,
        int amountScale = AkibaPayPaymentSeedData.AmountScale)
    {
        var parameters = new (string Key, string Value)[]
        {
            ("merchantId", merchantId),
            ("merchantPass", merchantPass),
            ("fromNumber", NormalizeFromNumber(fromNumber)),
            ("toNumber", string.Empty), // required in sign string even when empty
            ("amount", FormatAmountForSign(amount, amountScale)),
            ("currency", currency),
            ("callbackUrl", callbackUrl),
            ("merchantReference", merchantReference)
        };

        return BuildRfc3986QueryString(parameters);
    }

    /// <summary>
    /// MSISDN for AkibaPay sign string and request body: digits only, no leading <c>+</c>, spaces, or dashes
    /// 
    /// </summary>
    /// <summary>
    /// MSISDN for AkibaPay sign string and request body: digits only, no leading <c>+</c>
    /// 
    /// </summary>
    public static string NormalizeFromNumber(string fromNumber)
    {
        if (string.IsNullOrWhiteSpace(fromNumber))
        {
            return string.Empty;
        }

        var trimmed = fromNumber.Trim();
        if (trimmed.StartsWith('+'))
        {
            trimmed = trimmed[1..].TrimStart();
        }

        return new string(trimmed.Where(char.IsDigit).ToArray());
    }

    /// <summary>
    /// Rounds to AkibaPay currency scale and strips trailing zeros so JSON + sign match PHP
    /// float formatting (e.g. <c>88.2</c> not <c>88.20</c>; <c>1.25</c> not <c>1.250000</c>).
    /// </summary>
    public static decimal NormalizeAmount(decimal amount, int scale = AkibaPayPaymentSeedData.AmountScale)
    {
        var rounded = Math.Round(amount, scale, MidpointRounding.AwayFromZero);
        // Math.Round(..., scale) keeps fixed scale (88.2 → 88.20). AkibaPay/PHP signs "88.2".
        return decimal.Parse(
            rounded.ToString($"0.{new string('#', scale)}", CultureInfo.InvariantCulture),
            CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Amount in the HMAC query string must match AkibaPay PHP casting:
    /// whole numbers without decimals (<c>88</c>), fractional without trailing zeros (<c>88.2</c>).
    /// </summary>
    private static string FormatAmountForSign(decimal amount, int scale = AkibaPayPaymentSeedData.AmountScale)
    {
        if (amount == decimal.Truncate(amount))
        {
            return ((long)amount).ToString(CultureInfo.InvariantCulture);
        }

        return amount.ToString($"0.{new string('#', scale)}", CultureInfo.InvariantCulture);
    }

    private static string ComputeHmacSha512HexLower(string payload, string merchantId)
    {
        using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(merchantId));
        var digest = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(digest).ToLowerInvariant();
    }

    private static string BuildRfc3986QueryString(IEnumerable<(string Key, string Value)> parameters)
    {
        var sb = new StringBuilder();
        foreach (var (key, value) in parameters)
        {
            if (sb.Length > 0)
            {
                sb.Append('&');
            }

            sb.Append(Uri.EscapeDataString(key));
            sb.Append('=');
            sb.Append(Uri.EscapeDataString(value ?? string.Empty));
        }

        return sb.ToString();
    }
}
