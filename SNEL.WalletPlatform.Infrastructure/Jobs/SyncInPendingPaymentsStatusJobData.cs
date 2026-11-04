using System.Globalization;
using Quartz;

namespace SNEL.WalletPlatform.Infrastructure.Jobs;

internal static class SyncInPendingPaymentsStatusJobData
{
    internal const string PageSizeKey = "PageSize";
    internal const string MinimumAgeMinutesKey = "MinimumAgeMinutes";

    internal static JobDataMap CreateMapForManualTrigger(int? pageSize, int? minimumAgeMinutes)
    {
        var map = new JobDataMap();
        if (pageSize is > 0)
            map.Add(PageSizeKey, pageSize.Value.ToString(CultureInfo.InvariantCulture));
        if (minimumAgeMinutes is > 0)
            map.Add(MinimumAgeMinutesKey, minimumAgeMinutes.Value.ToString(CultureInfo.InvariantCulture));
        return map;
    }
    internal static int ReadPageSize(JobDataMap map, int defaultValue)
    {
        if (!map.ContainsKey(PageSizeKey))
            return defaultValue;

        return ReadPositiveInt(map[PageSizeKey], defaultValue);
    }

    internal static int ReadMinimumAgeMinutes(JobDataMap map, int defaultValue)
    {
        if (!map.ContainsKey(MinimumAgeMinutesKey))
            return defaultValue;

        return ReadPositiveInt(map[MinimumAgeMinutesKey], defaultValue);
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
