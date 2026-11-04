using SNEL.WalletPlatform.Application.Common;

namespace SNEL.WalletPlatform.Application.Payments.Interfaces;

/// <summary>
/// Provides a cached OAuth bearer token for Afrimony API calls.
/// </summary>
public interface IAfrimonyTokenProvider
{
    Task<Result<string>> GetTokenAsync(
        string oauthUrl,
        string oauthBasic,
        CancellationToken cancellationToken = default);

    void InvalidateToken(string oauthUrl, string oauthBasic);
}
