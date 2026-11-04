using SNEL.WalletPlatform.Application.Common;

namespace SNEL.WalletPlatform.Application.Payments.Interfaces;

public interface IPaymentProviderAccessPolicy
{
    /// <summary>
    /// Returns null when caller can view all payment providers (platform admin or customer).
    /// Returns partner id when caller is PSP admin and must be partner scoped.
    /// </summary>
    Task<Result<long?>> GetPaymentProvidersReadScopeAsync(CancellationToken cancellationToken = default);
}
