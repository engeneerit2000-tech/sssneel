using System.Globalization;
using Quartz;

namespace SNEL.WalletPlatform.Infrastructure.Jobs;

/// <summary>
/// With Quartz JDBC store <c>useProperties = true</c>, JobDataMap values must be strings when persisted.
/// Manual one-off triggers pass data that gets persisted the same way.
/// </summary>
internal static class ExpireStalePendingPaymentsJobData
{
    internal const string PageSizeKey = "PageSize";
    internal const string StaleAfterMinutesKey = "StaleAfterMinutes";

    internal static JobDataMap CreateMapForManualTrigger(int? pageSize, int? staleAfterMinutes)
    {
        var map = new JobDataMap();
        if (pageSize is > 0)
            map.Add(PageSizeKey, pageSize.Value.ToString(CultureInfo.InvariantCulture));
        if (staleAfterMinutes is > 0)
            map.Add(StaleAfterMinutesKey, staleAfterMinutes.Value.ToString(CultureInfo.InvariantCulture));
        return map;
    }

    internal static int ReadPageSize(JobDataMap map, int defaultValue)
    {
        if (!map.ContainsKey(PageSizeKey))
            return defaultValue;
        return ReadPositiveInt(map[PageSizeKey], defaultValue);
    }

    internal static int ReadStaleAfterMinutes(JobDataMap map, int defaultValue)
    {
        if (!map.ContainsKey(StaleAfterMinutesKey))
            return defaultValue;
        return ReadPositiveInt(map[StaleAfterMinutesKey], defaultValue);
    }

    private static int ReadPositiveInt(object? raw, int defaultValue)
    {
        switch (raw)
        {
            case int i when i > 0:
                return i;
            case long l when l > 0 && l <= int.MaxValue:
                return (int)l;
            case string s when int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var p) && p > 0:
                return p;
            default:
                return defaultValue;
        }
    }
}
