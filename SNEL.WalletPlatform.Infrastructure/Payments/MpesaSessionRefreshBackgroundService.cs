using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SNEL.WalletPlatform.Application.Payments;
using SNEL.WalletPlatform.Application.Payments.Configurations;
using SNEL.WalletPlatform.Application.Payments.Interfaces;

namespace SNEL.WalletPlatform.Infrastructure.Payments;

public sealed class MpesaSessionRefreshBackgroundService(
    IMpesaSessionProvider sessionProvider,
    IOptions<MpesaOptions> options,
    ILogger<MpesaSessionRefreshBackgroundService> logger) : BackgroundService
{
    private static readonly TimeSpan RefreshCheckInterval = TimeSpan.FromSeconds(15);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var defaultCredentials = PaymentMethodConfigReader.ResolveMpesaCredentials(null, options.Value);
        if (HasSessionCredentials(defaultCredentials))
        {
            var warmup = await sessionProvider.WarmUpAsync(defaultCredentials, stoppingToken);
            if (!warmup.IsSuccess)
                logger.LogWarning("MPesa startup session warm-up failed: {Message}", warmup.Message);
        }
        else
        {
            logger.LogInformation(
                "MPesa startup warm-up skipped because default BaseUrl, SessionPath, PublicKey, or ApiKey is missing.");
        }

        using var timer = new PeriodicTimer(RefreshCheckInterval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await sessionProvider.RefreshDueSessionsAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "MPesa session refresh cycle failed.");
            }
        }
    }

    private static bool HasSessionCredentials(PaymentMethodConfigReader.MpesaCredentials credentials) =>
        !string.IsNullOrWhiteSpace(credentials.BaseUrl) &&
        !string.IsNullOrWhiteSpace(credentials.SessionPath) &&
        !string.IsNullOrWhiteSpace(credentials.PublicKey) &&
        !string.IsNullOrWhiteSpace(credentials.ApiKey);
}
