namespace SNEL.WalletPlatform.Application.Jobs.BatchProcessing;

public interface IBatchSleepCalculator
{
    int CalculateSleepMilliseconds(int batchSize, int sleepMillisecondsPerRecipient, int minSleepMs, int maxSleepMs);
}
