namespace SNEL.WalletPlatform.Application.Jobs.BatchProcessing;

public sealed class BatchSleepCalculator : IBatchSleepCalculator
{
    public int CalculateSleepMilliseconds(
        int batchSize,
        int sleepMillisecondsPerRecipient,
        int minSleepMs,
        int maxSleepMs)
    {
        if (batchSize <= 0)
            return minSleepMs;

        var sleepMs = batchSize * sleepMillisecondsPerRecipient;
        return Math.Clamp(sleepMs, minSleepMs, maxSleepMs);
    }
}
