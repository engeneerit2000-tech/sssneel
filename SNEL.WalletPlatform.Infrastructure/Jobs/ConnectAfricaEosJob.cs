using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using SNEL.WalletPlatform.Infrastructure.BillingServices.ConnectAfrica;

namespace SNEL.WalletPlatform.Infrastructure.Jobs;

[DisallowConcurrentExecution]
public sealed class ConnectAfricaEosJob(
    IServiceScopeFactory scopeFactory,
    ILogger<ConnectAfricaEosJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var sw = Stopwatch.StartNew();
        logger.LogInformation("ConnectAfricaEosJob started.");

        try
        {
            using var scope = scopeFactory.CreateScope();
            var connectAfricaHttpClient = scope.ServiceProvider.GetRequiredService<ConnectAfricaHttpClient>();

            logger.LogInformation("Connect Africa EOS: calling EosAsync (third-party request).");
            var response = await connectAfricaHttpClient.EosAsync();
            logger.LogInformation(
                "Connect Africa EOS: EosAsync response. IsSuccess={IsSuccess}, ResponseCode={ResponseCode}, ResponseBody={ResponseBody}, ShiftNumber={ShiftNumber}, UnitNumber={UnitNumber}",
                response.IsSuccess,
                response.ResponseCode,
                response.ResponseBody,
                response.ShiftNumber,
                response.UnitNumber);
            if (!response.IsSuccess)
            {
                throw new InvalidOperationException(
                    $"ConnectAfrica EOS returned unsuccessful response. ResponseCode={response.ResponseCode}, ResponseBody={response.ResponseBody}");
            }

            sw.Stop();
            logger.LogInformation(
                "ConnectAfricaEosJob completed successfully. ShiftNumber={ShiftNumber}, UnitNumber={UnitNumber}, Date={Date}, Time={Time}, TotalAmount={TotalAmount}, ElapsedMs={ElapsedMs}",
                response.ShiftNumber,
                response.UnitNumber,
                response.Date,
                response.Time,
                response.TotalAmount,
                sw.ElapsedMilliseconds);
            logger.LogDebug(
                "ConnectAfricaEosJob raw response: {ResponseBody}",
                response.ResponseBody);
        }
        catch (Exception ex)
        {
            sw.Stop();
            logger.LogError(ex, "ConnectAfricaEosJob failed after {ElapsedMs} ms.", sw.ElapsedMilliseconds);
            throw;
        }
    }
}
