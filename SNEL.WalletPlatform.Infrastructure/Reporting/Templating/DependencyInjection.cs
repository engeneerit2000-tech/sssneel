using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SNEL.WalletPlatform.Application.Reporting.Interfaces;

namespace SNEL.WalletPlatform.Infrastructure.Reporting.Templating;

public static class DependencyInjection
{
    public static IServiceCollection AddTemplateRendering(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ScribanTemplateRendererOptions>(
            configuration.GetSection("TemplateRendering"));

        services.AddSingleton<ITemplatePathResolver, ScribanTemplatePathResolver>();
        services.AddScoped<ITemplateRednerer, ScribanTemplateRenderer>();

        return services;
    }
}