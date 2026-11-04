using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using SNEL.WalletPlatform.Application.Interfaces.UnitOfWork;
using SNEL.WalletPlatform.Application.Payments.Interfaces;
using SNEL.WalletPlatform.Domain.Payments;

namespace SNEL.WalletPlatform.Infrastructure.Jobs;

[DisallowConcurrentExecution]
public sealed class ExpireStalePendingPaymentsJob(
    IServiceScopeFactory scopeFactory,
    ILogger<ExpireStalePendingPaymentsJob> logger) : IJob
{
    private const int DefaultPageSize = 100;
    private const int DefaultStaleAfterMinutes = 5;

    public async Task Execute(IJobExecutionContext context)
    {
        var sw = Stopwatch.StartNew();
        var ct = context.CancellationToken;

        var map = context.MergedJobDataMap;
        var pageSize = ExpireStalePendingPaymentsJobData.ReadPageSize(map, DefaultPageSize);
        var staleAfterMinutes = ExpireStalePendingPaymentsJobData.ReadStaleAfterMinutes(map, DefaultStaleAfterMinutes);

        logger.LogInformation(
            "ExpireStalePendingPaymentsJob started (staleAfterMinutes={StaleAfterMinutes}, pageSize={PageSize}).",
            staleAfterMinutes,
            pageSize);

        try
        {
            using var scope = scopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();

            var cutoffUtc = DateTimeOffset.UtcNow.AddMinutes(-staleAfterMinutes);
            var paymentIds = await unitOfWork.PaymentRepository.GetCanBeExpiredPaymentIdsCreatedBeforeUtcAsync(
                cutoffUtc,
                pageSize,
                ct);
            logger.LogInformation($"ExpireStalePendingPaymentsJob: Getting payment ids to be expired befor {cutoffUtc}, count is {paymentIds.Count}");

            var expiredCount = 0;
            var resolvedFromPspCount = 0;
            foreach (var paymentId in paymentIds)
            {
                try
                {
                    var result = await paymentService.ExpireStalePendingPaymentAsync(paymentId);
                    if (!result.IsSuccess)
                    {
                        logger.LogWarning(
                            "ExpireStalePendingPaymentsJob could not expire payment {PaymentId}: {Message}",
                            paymentId,
                            result.Message);
                        continue;
                    }

                    if (result.Value == PaymentStatus.Expired)
                    {
                        expiredCount++;
                    }
                    else
                    {
                        resolvedFromPspCount++;
                        logger.LogInformation(
                            "ExpireStalePendingPaymentsJob resolved payment {PaymentId} from PSP with status {PaymentStatus} instead of expiring.",
                            paymentId,
                            result.Value);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(
                        ex,
                        "ExpireStalePendingPaymentsJob failed to expire payment {PaymentId}. Continuing with remaining candidates.",
                        paymentId);
                }
            }

            sw.Stop();
            logger.LogInformation(
                "ExpireStalePendingPaymentsJob finished in {ElapsedMs} ms. Candidates={CandidateCount}, expired={ExpiredCount}, resolvedFromPsp={ResolvedFromPspCount}.",
                sw.ElapsedMilliseconds,
                paymentIds.Count,
                expiredCount,
                resolvedFromPspCount);
        }
        catch (Exception ex)
        {
            sw.Stop();
            logger.LogError(ex, "ExpireStalePendingPaymentsJob failed after {ElapsedMs} ms.", sw.ElapsedMilliseconds);
            throw;
        }
    }
}
