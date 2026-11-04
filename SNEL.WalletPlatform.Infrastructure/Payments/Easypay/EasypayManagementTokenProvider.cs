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
using SNEL.WalletPlatform.Application.Payments.Models.External.Easypay;
using SNEL.WalletPlatform.Infrastructure.Payments.Clients;

namespace SNEL.WalletPlatform.Infrastructure.Payments.Easypay;

public sealed class EasypayManagementTokenProvider(
    IServiceScopeFactory scopeFactory,
    IStringLocalizer<SharedResource> localizer,
    ILogger<EasypayManagementTokenProvider> logger) : IEasypayManagementTokenProvider
{
    private static readonly TimeSpan DefaultTokenLifetime = TimeSpan.FromMinutes(50);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ConcurrentDictionary<string, TokenCacheEntry> _cache = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _refreshLocks = new(StringComparer.Ordinal);

    public async Task<Result<string>> GetTokenAsync(
        string managementBaseUrl,
        string apiKey,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = BuildCacheKey(managementBaseUrl, apiKey);
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

            return await LoginAsync(managementBaseUrl, apiKey, cacheKey, cancellationToken)
                .ConfigureAwait(false);
        }
        finally
        {
            refreshLock.Release();
        }
    }

    public void InvalidateToken(string managementBaseUrl, string login)
    {
        var cacheKey = BuildCacheKey(managementBaseUrl, login);
        _cache.TryRemove(cacheKey, out _);
    }

    private static string BuildCacheKey(string managementBaseUrl, string login) =>
        $"{managementBaseUrl.Trim().ToUpperInvariant()}|{login.Trim().ToUpperInvariant()}";

    private async Task<Result<string>> LoginAsync(
        string managementBaseUrl,
        string apiKey,
        string cacheKey,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return Result<string>.Failure(
                localizer[LocalizationKeys.PaymentEasypayManagementCredentialsNotConfigured],
                AppHttpStatusCodes.Failed);
        }
        
        var loginUrl = $"{managementBaseUrl.TrimEnd('/')}/auth";
        logger.LogInformation(
            "Easypay management login request (token cache refresh). Url={Url}",
            loginUrl);

        using var scope = scopeFactory.CreateScope();
        var httpClient = scope.ServiceProvider.GetRequiredService<EasypayHttpClient>();

        var loginResponse = await httpClient
            .PostLoginAsync(loginUrl, apiKey, cancellationToken)
            .ConfigureAwait(false);
        var loginResponseBody = await loginResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (loginResponse.StatusCode != HttpStatusCode.OK)
        {
            logger.LogWarning(
                "Easypay management login failed during token cache refresh. StatusCode={StatusCode}, Body={ResponseBody}",
                (int)loginResponse.StatusCode,
                loginResponseBody);

            return Result<string>.Failure(
                localizer[LocalizationKeys.PaymentEasypayManagementLoginHttpFailed, ((int)loginResponse.StatusCode).ToString(CultureInfo.InvariantCulture)],
                AppHttpStatusCodes.Failed);
        }

        var loginModel = JsonSerializer.Deserialize<EasypayLoginResponseModel>(loginResponseBody, JsonOptions);        
        if (loginModel is null || loginModel.Success != 1)
        {
            return Result<string>.Failure(
                localizer[LocalizationKeys.PaymentEasypayManagementLoginParseFailed],
                AppHttpStatusCodes.Failed);
        }

        _cache[cacheKey] = new TokenCacheEntry(loginModel.Data.Token, DateTimeOffset.UtcNow.Add(DefaultTokenLifetime));
        logger.LogInformation("Easypay management token cached until {ExpiresAtUtc} UTC.", _cache[cacheKey].ExpiresAtUtc);

        return Result<string>.Success(loginModel.Data.Token);
    }

    private sealed record TokenCacheEntry(string Token, DateTimeOffset ExpiresAtUtc)
    {
        public bool IsValid => !string.IsNullOrWhiteSpace(Token) && DateTimeOffset.UtcNow < ExpiresAtUtc;
    }
}
