using Quartz;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using SNEL.WalletPlatform.Application.Auth.Interfaces;

namespace SNEL.WalletPlatform.Infrastructure.Jobs;

[DisallowConcurrentExecution]
public class DummyJob : IJob
{
    private readonly ILogger<DummyJob> _logger;
    private readonly IServiceProvider _serviceProvider;

    public DummyJob(ILogger<DummyJob> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }


    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation($"Dummy Job: started at {DateTime.UtcNow} UTC.");
        Thread.Sleep(130000);
        _logger.LogInformation($"Dummy Job: completed at {DateTime.UtcNow} UTC.");
    }
}
