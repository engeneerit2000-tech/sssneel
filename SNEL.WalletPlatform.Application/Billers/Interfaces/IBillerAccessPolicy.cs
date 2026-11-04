using SNEL.WalletPlatform.Application.Common;

namespace SNEL.WalletPlatform.Application.Billers.Interfaces;

public interface IBillerAccessPolicy
{
    /// <summary>
    /// Returns null when caller can view all billers (platform admin or customer).
    /// Returns partner id when caller is biller admin and must be partner scoped.
    /// </summary>
    Task<Result<long?>> GetBillersReadScopeAsync(CancellationToken cancellationToken = default);
}
