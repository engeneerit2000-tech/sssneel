using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SNEL.WalletPlatform.Application.Interfaces.UnitOfWork;
using SNEL.WalletPlatform.Application.Payments;
using SNEL.WalletPlatform.Application.Payments.Configurations;
using SNEL.WalletPlatform.Application.Payments.Interfaces;
using SNEL.WalletPlatform.Domain.Payments;

namespace SNEL.WalletPlatform.Infrastructure.Payments;

public sealed class AkibaPayStatusPollingBackgroundService(
    AkibaPayStatusPollingCoordinator coordinator,
    IServiceScopeFactory serviceScopeFactory,
    IOptions<AkibaPayOptions> akibaPayOptions,
    ILogger<AkibaPayStatusPollingBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var paymentId in coordinator.Reader.ReadAllAsync(stoppingToken))
        {
            _ = PollPaymentStatusAsync(paymentId, stoppingToken);
        }
    }

    private async Task PollPaymentStatusAsync(Guid paymentId, CancellationToken stoppingToken)
    {
        var pollingToken = coordinator.GetPollingCancellationToken(paymentId, stoppingToken);

        try
        {
            using var scope = serviceScopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var payment = await unitOfWork.PaymentRepository.GetByIdWithPspContextAsync(paymentId);
            if (payment?.PaymentMethod is null)
            {
                logger.LogWarning(
                    "AkibaPay status polling skipped for payment {PaymentId} because payment method was not found.",
                    paymentId);
                return;
            }

            var pollingSchedule = PaymentMethodConfigReader.ResolveStatusPolling(
                payment.PaymentMethod.Config,
                PaymentMethodConfigReader.StatusPollingSchedule.FromAkibaPayOptions(akibaPayOptions.Value));

            logger.LogInformation(
                "AkibaPay status polling scheduled for payment {PaymentId}. First check in {InitialDelaySeconds} seconds.",
                paymentId,
                pollingSchedule.InitialDelaySeconds);

            await Task.Delay(
                TimeSpan.FromSeconds(pollingSchedule.InitialDelaySeconds),
                pollingToken);

            for (var attempt = 1; attempt <= pollingSchedule.TotalAttempts; attempt++)
            {
                pollingToken.ThrowIfCancellationRequested();

                var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();

                var syncResult = await paymentService.SyncAkibaPayStatusFromManagementAsync(
                    paymentId,
                    passApply: false,
                    cancellationToken: stoppingToken,
                    statusPollingAttempt: attempt);

                if (!syncResult.IsSuccess)
                {
                    logger.LogWarning(
                        "AkibaPay status polling attempt {Attempt}/{TotalAttempts} failed for payment {PaymentId}: {Message}",
                        attempt,
                        pollingSchedule.TotalAttempts,
                        paymentId,
                        syncResult.Message);
                }
                else if (syncResult.Value.IsFinal())
                {
                    logger.LogInformation(
                        "AkibaPay status polling resolved payment {PaymentId} to {PaymentStatus} on attempt {Attempt}/{TotalAttempts}.",
                        paymentId,
                        syncResult.Value,
                        attempt,
                        pollingSchedule.TotalAttempts);
                    return;
                }
                else
                {
                    logger.LogInformation(
                        "AkibaPay status polling attempt {Attempt}/{TotalAttempts}: payment {PaymentId} is still {PaymentStatus}.",
                        attempt,
                        pollingSchedule.TotalAttempts,
                        paymentId,
                        syncResult.Value);
                }

                if (attempt < pollingSchedule.TotalAttempts)
                {
                    await Task.Delay(
                        TimeSpan.FromSeconds(pollingSchedule.IntervalSeconds),
                        pollingToken);
                }
            }

            var failResult = await scope.ServiceProvider
                .GetRequiredService<IPaymentService>()
                .ApplyAkibaPayWebhookOutcomeAsync(
                    paymentId,
                    "FAILED",
                    akibaReference: null,
                    stoppingToken);

            if (failResult.IsSuccess)
            {
                logger.LogWarning(
                    "AkibaPay status polling marked payment {PaymentId} as Failed after {TotalAttempts} attempts without a final status.",
                    paymentId,
                    pollingSchedule.TotalAttempts);
            }
            else
            {
                logger.LogWarning(
                    "AkibaPay status polling could not mark payment {PaymentId} as Failed after {TotalAttempts} attempts: {Message}",
                    paymentId,
                    pollingSchedule.TotalAttempts,
                    failResult.Message);
            }
        }
        catch (OperationCanceledException) when (pollingToken.IsCancellationRequested)
        {
            logger.LogInformation(
                "AkibaPay status polling cancelled for payment {PaymentId}.",
                paymentId);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "AkibaPay status polling encountered an unexpected error for payment {PaymentId}.",
                paymentId);
        }
        finally
        {
            coordinator.Complete(paymentId);
        }
    }
}
