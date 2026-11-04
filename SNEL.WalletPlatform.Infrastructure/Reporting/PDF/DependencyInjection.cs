using Gotenberg.Sharp.API.Client.Domain.Settings;
using Gotenberg.Sharp.API.Client.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SNEL.WalletPlatform.Application.Reporting.Interfaces;

namespace SNEL.WalletPlatform.Infrastructure.Reporting.PDF;

public static class  DependencyInjection
{
    public static IServiceCollection AddHtmlToPdf(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<GotenbergSharpClientOptions>()
            .Bind(configuration.GetSection(GotenbergOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddGotenbergSharpClient();

        services.AddScoped<IHtmlToPdfGenerator, GotenbergHtmlToPdfGenerator>();

        return services;
    }
}