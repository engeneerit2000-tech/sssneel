using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using SNEL.WalletPlatform.Application.BillingServices.Interfaces;

namespace SNEL.WalletPlatform.Infrastructure.Jobs;

[DisallowConcurrentExecution]
public sealed class SnelBootstrapJob(
    IServiceScopeFactory scopeFactory,
    ILogger<SnelBootstrapJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
           using var scope = scopeFactory.CreateScope();
            var bootstrapService = scope.ServiceProvider.GetRequiredService<ISnelBootstrapService>();

            await bootstrapService.EnsureInitializedAsync(context.CancellationToken);
            logger.LogInformation("SnelBootstrapJob completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "SnelBootstrapJob failed.");
            throw;
        }
    }
}
