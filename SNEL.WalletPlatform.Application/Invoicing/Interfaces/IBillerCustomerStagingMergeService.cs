namespace SNEL.WalletPlatform.Application.Invoicing.Interfaces;

/// <summary>
/// Merges biller customer staging rows into the main biller customer table.
/// </summary>
public interface IBillerCustomerStagingMergeService
{
    /// <param name="jobKey">Staging rows for this Quartz job run only.</param>
    /// <returns>Counts of inserted and updated rows.</returns>
    Task<(int Inserted, int Updated)> MergeFromStagingAsync(long billerId, string jobKey, CancellationToken cancellationToken = default);
}
