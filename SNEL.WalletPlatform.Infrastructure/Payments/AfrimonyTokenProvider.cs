using System.Collections.Concurrent;
using System.Globalization;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using SNEL.WalletPlatform.Application;
using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Common.Localization;
using SNEL.WalletPlatform.Application.Payments.Interfaces;
using SNEL.WalletPlatform.Application.Payments.Models.External.Afrimony;
using SNEL.WalletPlatform.Infrastructure.Payments.Clients;

namespace SNEL.WalletPlatform.Infrastructure.Payments;

public sealed class AfrimonyTokenProvider(
    IServiceScopeFactory scopeFactory,
    IStringLocalizer<SharedResource> localizer,
    ILogger<AfrimonyTokenProvider> logger) : IAfrimonyTokenProvider
{
    private static readonly TimeSpan TokenExpirySkew = TimeSpan.FromSeconds(30);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ConcurrentDictionary<string, TokenCacheEntry> _cache = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _refreshLocks = new(StringComparer.Ordinal);

    public async Task<Result<string>> GetTokenAsync(
        string oauthUrl,
        string oauthBasic,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = BuildCacheKey(oauthUrl, oauthBasic);
        if (_cache.TryGetValue(cacheKey, out var cached) && cached.IsValid)
        {
            return Result<string>.Success(cached.Token);
        }

        var refreshLock = _refreshLocks.GetOrAdd(cacheKey, _ => new SemaphoreSlim(1, 1));
        await refreshLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_cache.TryGetValue(cacheKey, out cached) && cached.IsValid)
            {
                return Result<string>.Success(cached.Token);
            }

            return await LoginAsync(oauthUrl, oauthBasic, cacheKey, cancellationToken)
                .ConfigureAwait(false);
        }
        finally
        {
            refreshLock.Release();
        }
    }

    public void InvalidateToken(string oauthUrl, string oauthBasic)
    {
        var cacheKey = BuildCacheKey(oauthUrl, oauthBasic);
        _cache.TryRemove(cacheKey, out _);
    }

    private static string BuildCacheKey(string oauthUrl, string oauthBasic) =>
        $"{oauthUrl.Trim().ToUpperInvariant()}|{oauthBasic.Trim()}";

    private async Task<Result<string>> LoginAsync(
        string oauthUrl,
        string oauthBasic,
        string cacheKey,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(oauthUrl) || string.IsNullOrWhiteSpace(oauthBasic))
        {
            return Result<string>.Failure(
                localizer[LocalizationKeys.PaymentAfrimonyOAuthCredentialsNotConfigured],
                AppHttpStatusCodes.Failed);
        }

        logger.LogInformation(
            "Afrimony OAuth token request (token cache refresh). Url={Url}",
            oauthUrl);

        using var scope = scopeFactory.CreateScope();
        var httpClient = scope.ServiceProvider.GetRequiredService<AfrimonyHttpClient>();

        var loginResponse = await httpClient
            .PostOAuthTokenAsync(oauthUrl, oauthBasic, cancellationToken)
            .ConfigureAwait(false);
        var loginResponseBody = await loginResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (loginResponse.StatusCode != HttpStatusCode.OK)
        {
            logger.LogWarning(
                "Afrimony OAuth failed during token cache refresh. StatusCode={StatusCode}, Body={ResponseBody}",
                (int)loginResponse.StatusCode,
                loginResponseBody);

            return Result<string>.Failure(
                localizer[LocalizationKeys.PaymentAfrimonyOAuthServiceUnavailable],
                AppHttpStatusCodes.BadRequest);
        }

        var loginModel = JsonSerializer.Deserialize<AfrimonyOAuthTokenResponseModel>(loginResponseBody, JsonOptions);
        if (loginModel is null || string.IsNullOrWhiteSpace(loginModel.AccessToken))
        {
            return Result<string>.Failure(
                localizer[LocalizationKeys.PaymentAfrimonyOAuthParseFailed],
                AppHttpStatusCodes.Failed);
        }

        var lifetimeSeconds = loginModel.ExpiresIn > 0 ? loginModel.ExpiresIn : 260;
        var expiresAt = DateTimeOffset.UtcNow.AddSeconds(lifetimeSeconds).Subtract(TokenExpirySkew);
        _cache[cacheKey] = new TokenCacheEntry(loginModel.AccessToken, expiresAt);
        logger.LogInformation("Afrimony OAuth token cached until {ExpiresAtUtc} UTC.", expiresAt);

        return Result<string>.Success(loginModel.AccessToken);
    }

    private sealed record TokenCacheEntry(string Token, DateTimeOffset ExpiresAtUtc)
    {
        public bool IsValid => !string.IsNullOrWhiteSpace(Token) && DateTimeOffset.UtcNow < ExpiresAtUtc;
    }
}
