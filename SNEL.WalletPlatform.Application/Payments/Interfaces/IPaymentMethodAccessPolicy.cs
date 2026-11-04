using SNEL.WalletPlatform.Application.Common;

namespace SNEL.WalletPlatform.Application.Payments.Interfaces;

public interface IPaymentMethodAccessPolicy
{
    /// <summary>
    /// Returns null when caller can access all payment methods.
    /// Returns partner id when caller must be scoped to a PSP partner.
    /// </summary>
    Task<Result<long?>> GetPaymentMethodsScopeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns null when caller can manage all payment methods (platform admin).
    /// Returns partner id when caller is PSP admin and must be scoped.
    /// </summary>
    Task<Result<long?>> GetPaymentMethodManagementScopeAsync(CancellationToken cancellationToken = default);
}
