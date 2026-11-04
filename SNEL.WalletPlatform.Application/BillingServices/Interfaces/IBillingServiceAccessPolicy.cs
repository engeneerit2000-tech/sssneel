using SNEL.WalletPlatform.Application.Common;

namespace SNEL.WalletPlatform.Application.BillingServices.Interfaces;

public interface IBillingServiceAccessPolicy
{
    /// <summary>
    /// Returns null when caller can view all billing services (platform admin or customer).
    /// Returns partner id when caller is biller admin and must be partner scoped.
    /// </summary>
    Task<Result<long?>> GetBillingServicesReadScopeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns null when caller can manage all billing services (platform admin).
    /// Returns partner id when caller is biller admin and must be partner scoped.
    /// </summary>
    Task<Result<long?>> GetBillingServiceManagementScopeAsync(CancellationToken cancellationToken = default);
}
