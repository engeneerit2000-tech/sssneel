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

public sealed class AfrimonyStatusPollingBackgroundService(
    AfrimonyStatusPollingCoordinator coordinator,
    IServiceScopeFactory serviceScopeFactory,
    IOptions<AfrimonyOptions> afrimonyOptions,
    ILogger<AfrimonyStatusPollingBackgroundService> logger) : BackgroundService
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
                    "Afrimony status polling skipped for payment {PaymentId} because payment method was not found.",
                    paymentId);
                return;
            }

            var pollingSchedule = PaymentMethodConfigReader.ResolveStatusPolling(
                payment.PaymentMethod.Config,
                PaymentMethodConfigReader.StatusPollingSchedule.FromAfrimonyOptions(afrimonyOptions.Value));

            logger.LogInformation(
                "Afrimony status polling scheduled for payment {PaymentId}. First check in {InitialDelaySeconds} seconds.",
                paymentId,
                pollingSchedule.InitialDelaySeconds);

            await Task.Delay(
                TimeSpan.FromSeconds(pollingSchedule.InitialDelaySeconds),
                pollingToken);

            for (var attempt = 1; attempt <= pollingSchedule.TotalAttempts; attempt++)
            {
                pollingToken.ThrowIfCancellationRequested();

                var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();

                var syncResult = await paymentService.SyncAfrimonyStatusFromEnquiryAsync(
                    paymentId,
                    passApply: false,
                    cancellationToken: stoppingToken,
                    statusPollingAttempt: attempt);

                if (!syncResult.IsSuccess)
                {
                    logger.LogWarning(
                        "Afrimony status polling attempt {Attempt}/{TotalAttempts} failed for payment {PaymentId}: {Message}",
                        attempt,
                        pollingSchedule.TotalAttempts,
                        paymentId,
                        syncResult.Message);
                }
                else if (syncResult.Value.IsFinal())
                {
                    logger.LogInformation(
                        "Afrimony status polling resolved payment {PaymentId} to {PaymentStatus} on attempt {Attempt}/{TotalAttempts}.",
                        paymentId,
                        syncResult.Value,
                        attempt,
                        pollingSchedule.TotalAttempts);
                    return;
                }
                else
                {
                    logger.LogInformation(
                        "Afrimony status polling attempt {Attempt}/{TotalAttempts}: payment {PaymentId} is still {PaymentStatus}.",
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

            // If we never got a definitive status (Completed/Failed) from Afrimony within the polling window,
            // keep the payment as Processing so it can be retried/resolved later (e.g. after auth/provider issue).
            if (payment is not null && !payment.Status.IsFinal())
            {
                await unitOfWork.ExecuteInTransactionAsync(async _ =>
                {
                    payment.ChangePaymentArtifactStatus(PaymentStatus.Processing);
                    await unitOfWork.CompleteAsync();
                }, stoppingToken);

                logger.LogWarning(
                    "Afrimony status polling ended without a final status; marked payment {PaymentId} as Processing after {TotalAttempts} attempts.",
                    paymentId,
                    pollingSchedule.TotalAttempts);
            }
            else
            {
                logger.LogDebug(
                    "Afrimony status polling ended for payment {PaymentId} but payment was already final.",
                    paymentId);
            }
        }
        catch (OperationCanceledException) when (pollingToken.IsCancellationRequested)
        {
            logger.LogInformation(
                "Afrimony status polling cancelled for payment {PaymentId}.",
                paymentId);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Afrimony status polling encountered an unexpected error for payment {PaymentId}.",
                paymentId);
        }
        finally
        {
            coordinator.Complete(paymentId);
        }
    }
}
