using SNEL.WalletPlatform.Application.Common;

namespace SNEL.WalletPlatform.Application.Payments.Interfaces;

/// <summary>
/// Provides a cached bearer token for AkibaPay management API calls (status lookup).
/// </summary>
public interface IEasypayManagementTokenProvider
{
    Task<Result<string>> GetTokenAsync(
        string managementBaseUrl,
        string apiKey,
        CancellationToken cancellationToken = default);

    void InvalidateToken(string managementBaseUrl, string login);
}
