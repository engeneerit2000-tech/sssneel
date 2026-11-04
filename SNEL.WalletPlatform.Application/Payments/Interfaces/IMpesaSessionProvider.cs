using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Payments.Configurations;

namespace SNEL.WalletPlatform.Application.Payments.Interfaces;

public interface IMpesaSessionProvider
{
    Task<Result<string>> GetSessionAsync(
        PaymentMethodConfigReader.MpesaCredentials credentials,
        CancellationToken cancellationToken = default);

    Task<Result<string>> WarmUpAsync(
        PaymentMethodConfigReader.MpesaCredentials credentials,
        CancellationToken cancellationToken = default);

    Task RefreshDueSessionsAsync(CancellationToken cancellationToken = default);

    void Invalidate(PaymentMethodConfigReader.MpesaCredentials credentials);
}
