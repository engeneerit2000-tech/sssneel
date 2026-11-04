using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Payments;
using SNEL.WalletPlatform.Application.Payments.Interfaces;
using SNEL.WalletPlatform.Application.Payments.Models.External.Mpesa;
using SNEL.WalletPlatform.Infrastructure.Payments.Clients;

namespace SNEL.WalletPlatform.Infrastructure.Payments;

public sealed class MpesaSessionProvider(
    IServiceScopeFactory scopeFactory,
    ILogger<MpesaSessionProvider> logger) : IMpesaSessionProvider
{
    private static readonly TimeSpan RefreshSafetyWindow = TimeSpan.FromSeconds(30);
    private readonly ConcurrentDictionary<string, SessionEntry> cache = new();
    private readonly ConcurrentDictionary<string, PaymentMethodConfigReader.MpesaCredentials> registeredCredentials = new();
    private readonly ConcurrentDictionary<string, SemaphoreSlim> locks = new();

    public Task<Result<string>> GetSessionAsync(
        PaymentMethodConfigReader.MpesaCredentials credentials,
        CancellationToken cancellationToken = default)
    {
        var key = GetKey(credentials);
        registeredCredentials[key] = credentials;
        var now = DateTimeOffset.UtcNow;

        if (cache.TryGetValue(key, out var entry) && entry.ExpiresAt > now)
        {
            if (entry.RefreshAt <= now)
                TriggerRefresh(credentials);

            return Task.FromResult(Result<string>.Success(entry.SessionId));
        }

        // Never make a customer payment request wait for MPesa's activation window.
        TriggerRefresh(credentials);
        return Task.FromResult(Result<string>.Failure(
            "MPesa session is warming up. Please retry shortly.",
            AppHttpStatusCodes.Failed));
    }

    public Task<Result<string>> WarmUpAsync(
        PaymentMethodConfigReader.MpesaCredentials credentials,
        CancellationToken cancellationToken = default)
    {
        registeredCredentials[GetKey(credentials)] = credentials;
        return RefreshSessionAsync(credentials, force: false, cancellationToken);
    }

    public async Task RefreshDueSessionsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var refreshes = registeredCredentials.Values
            .Where(credentials =>
            {
                var key = GetKey(credentials);
                return !cache.TryGetValue(key, out var entry) || entry.RefreshAt <= now;
            })
            .Select(credentials => RefreshSessionAsync(credentials, force: false, cancellationToken));

        await Task.WhenAll(refreshes);
    }

    public void Invalidate(PaymentMethodConfigReader.MpesaCredentials credentials)
    {
        cache.TryRemove(GetKey(credentials), out _);
        registeredCredentials[GetKey(credentials)] = credentials;
        TriggerRefresh(credentials);
    }

    private void TriggerRefresh(PaymentMethodConfigReader.MpesaCredentials credentials) =>
        _ = RefreshSessionAsync(credentials, force: false, CancellationToken.None);

    private async Task<Result<string>> RefreshSessionAsync(
        PaymentMethodConfigReader.MpesaCredentials credentials,
        bool force,
        CancellationToken cancellationToken)
    {
        var key = GetKey(credentials);
        var gate = locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(cancellationToken);
        try
        {
            var now = DateTimeOffset.UtcNow;
            if (!force && cache.TryGetValue(key, out var current) && current.RefreshAt > now)
                return Result<string>.Success(current.SessionId);

            using var scope = scopeFactory.CreateScope();
            var client = scope.ServiceProvider.GetRequiredService<MpesaHttpClient>();
            var response = await client.GetSessionAsync(credentials, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("MPesa background session request failed. HttpStatus={Status}, Body={Body}",
                    (int)response.StatusCode, body);
                return Result<string>.Failure(
                    $"MPesa session request failed ({(int)response.StatusCode}): {body}",
                    AppHttpStatusCodes.Failed);
            }

            var model = JsonSerializer.Deserialize<MpesaSessionResponseModel>(body);
            if (string.IsNullOrWhiteSpace(model?.SessionId))
                return Result<string>.Failure(
                    "MPesa session response does not contain output_SessionID.",
                    AppHttpStatusCodes.Failed);

            var issuedAt = DateTimeOffset.UtcNow;
            var expiresAt = issuedAt.AddMinutes(Math.Max(1, credentials.SessionLifetimeMinutes));
            var activationDelay = TimeSpan.FromSeconds(Math.Max(0, credentials.SessionActivationDelaySeconds));

            if (activationDelay > TimeSpan.Zero)
                await Task.Delay(activationDelay, cancellationToken);

            var refreshAt = expiresAt - activationDelay - RefreshSafetyWindow;
            if (refreshAt <= DateTimeOffset.UtcNow)
                refreshAt = DateTimeOffset.UtcNow.AddSeconds(5);

            var replacement = new SessionEntry(model.SessionId.Trim(), expiresAt, refreshAt);
            cache[key] = replacement;
            logger.LogInformation(
                "MPesa session activated in background. ExpiresAt={ExpiresAt}, RefreshAt={RefreshAt}",
                replacement.ExpiresAt,
                replacement.RefreshAt);
            return Result<string>.Success(replacement.SessionId);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "MPesa background session acquisition failed.");
            return Result<string>.Failure("MPesa session acquisition failed.", AppHttpStatusCodes.Failed);
        }
        finally
        {
            gate.Release();
        }
    }

    private static string GetKey(PaymentMethodConfigReader.MpesaCredentials credentials) =>
        $"{credentials.BaseUrl}|{credentials.SessionPath}|{credentials.ApiKey}";

    private sealed record SessionEntry(string SessionId, DateTimeOffset ExpiresAt, DateTimeOffset RefreshAt);
}
