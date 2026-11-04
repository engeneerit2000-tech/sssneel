namespace SNEL.WalletPlatform.Application.Notification;

public static class DataPushPayloadBuilder
{
    public const string TopupSuccessEventType = "topupSuccess";

    /// <summary>
    /// Merges base payload with additional keys. Non-whitespace values from <paramref name="fields"/> override existing keys.
    /// </summary>
    public static Dictionary<string, string> Merge(
        IDictionary<string, string>? baseData,
        params (string Key, string? Value)[] fields)
    {
        var result = baseData != null
            ? new Dictionary<string, string>(baseData)
            : new Dictionary<string, string>();

        foreach (var (key, value) in fields)
        {
            if (!string.IsNullOrWhiteSpace(value))
                result[key] = value;
        }

        return result;
    }

    public static Dictionary<string, string> BuildTopupSuccessPayload(
        string paymentId,
        string walletAccountId,
        string amount,
        string currency,
        string balance,
        IDictionary<string, string>? additionalData = null)
    {
        return Merge(additionalData,
            ("eventType", TopupSuccessEventType),
            ("paymentId", paymentId),
            ("walletAccountId", walletAccountId),
            ("amount", amount),
            ("currency", currency),
            ("balance", balance));
    }
}
