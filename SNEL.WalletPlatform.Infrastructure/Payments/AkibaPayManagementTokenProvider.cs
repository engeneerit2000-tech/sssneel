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
using SNEL.WalletPlatform.Application.Payments.Models.External.AkibaPay;
using SNEL.WalletPlatform.Infrastructure.Payments.Clients;

namespace SNEL.WalletPlatform.Infrastructure.Payments;

public sealed class AkibaPayManagementTokenProvider(
    IServiceScopeFactory scopeFactory,
    IStringLocalizer<SharedResource> localizer,
    ILogger<AkibaPayManagementTokenProvider> logger) : IAkibaPayManagementTokenProvider
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
        string login,
        string password,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = BuildCacheKey(managementBaseUrl, login);
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

            return await LoginAsync(managementBaseUrl, login, password, cacheKey, cancellationToken)
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
        string login,
        string password,
        string cacheKey,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
        {
            return Result<string>.Failure(
                localizer[LocalizationKeys.PaymentAkibaPayManagementCredentialsNotConfigured],
                AppHttpStatusCodes.Failed);
        }

        var loginRequest = new AkibaPayLoginRequestModel
        {
            Login = login.Trim(),
            Password = password
        };

        var loginUrl = $"{managementBaseUrl.TrimEnd('/')}/api/login";
        logger.LogInformation(
            "AkibaPay management login request (token cache refresh). Url={Url}, Login={Login}",
            loginUrl,
            loginRequest.Login);

        using var scope = scopeFactory.CreateScope();
        var httpClient = scope.ServiceProvider.GetRequiredService<AkibaPayHttpClient>();

        var loginResponse = await httpClient
            .PostLoginAsync(loginUrl, loginRequest, cancellationToken)
            .ConfigureAwait(false);
        var loginResponseBody = await loginResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (loginResponse.StatusCode != HttpStatusCode.OK)
        {
            logger.LogWarning(
                "AkibaPay management login failed during token cache refresh. StatusCode={StatusCode}, Body={ResponseBody}",
                (int)loginResponse.StatusCode,
                loginResponseBody);

            return Result<string>.Failure(
                localizer[LocalizationKeys.PaymentAkibaPayManagementLoginHttpFailed, ((int)loginResponse.StatusCode).ToString(CultureInfo.InvariantCulture)],
                AppHttpStatusCodes.Failed);
        }

        var loginModel = JsonSerializer.Deserialize<AkibaPayLoginResponseModel>(loginResponseBody, JsonOptions);
        if (loginModel is null || string.IsNullOrWhiteSpace(loginModel.Token))
        {
            return Result<string>.Failure(
                localizer[LocalizationKeys.PaymentAkibaPayManagementLoginParseFailed],
                AppHttpStatusCodes.Failed);
        }

        _cache[cacheKey] = new TokenCacheEntry(loginModel.Token, DateTimeOffset.UtcNow.Add(DefaultTokenLifetime));
        logger.LogInformation("AkibaPay management token cached until {ExpiresAtUtc} UTC.", _cache[cacheKey].ExpiresAtUtc);

        return Result<string>.Success(loginModel.Token);
    }

    private sealed record TokenCacheEntry(string Token, DateTimeOffset ExpiresAtUtc)
    {
        public bool IsValid => !string.IsNullOrWhiteSpace(Token) && DateTimeOffset.UtcNow < ExpiresAtUtc;
    }
}
