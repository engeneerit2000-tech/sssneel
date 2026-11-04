using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SNEL.WalletPlatform.Application.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNEL.WalletPlatform.Infrastructure.Persistence
{
    internal class CacheInitializationService(IServiceProvider serviceProvider) : IHostedService
    {
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = serviceProvider.CreateScope();

            var cacheRepository = scope.ServiceProvider
                .GetRequiredService<ICachingRepository>();

            await cacheRepository.LoadCache();
        }

        public Task StopAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;
    }
}
