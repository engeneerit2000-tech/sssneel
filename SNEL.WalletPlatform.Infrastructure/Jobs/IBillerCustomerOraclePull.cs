using SNEL.WalletPlatform.Infrastructure.Persistence;

namespace SNEL.WalletPlatform.Infrastructure.Jobs;

/// <summary>
/// Pulls biller customer rows from Oracle (GCO_RACCORDEMENTS) into <see cref="ApplicationDbContext.BillerCustomerStagings"/>.
/// </summary>
public interface IBillerCustomerOraclePull
{
    Task<long> PullAllToStagingAsync(
        string areaCode,
        long billerId,
        string jobKey,
        int pageSize,
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken = default);
}
