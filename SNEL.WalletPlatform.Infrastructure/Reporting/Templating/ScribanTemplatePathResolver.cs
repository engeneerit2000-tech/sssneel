using Microsoft.Extensions.Options;
using SNEL.WalletPlatform.Application.Reporting.Interfaces;

namespace SNEL.WalletPlatform.Infrastructure.Reporting.Templating;

public class ScribanTemplatePathResolver:ITemplatePathResolver
{
    private readonly ScribanTemplateRendererOptions _options;

    public ScribanTemplatePathResolver(IOptions<ScribanTemplateRendererOptions> options)
    {
        _options = options.Value;
    }

    public string Resolve(string templateName)
    {
        var normalizedName = templateName.Replace('/', Path.DirectorySeparatorChar);

        var fullPath = Path.Combine(
            AppContext.BaseDirectory,
            _options.TemplatesRootPath,
            normalizedName + _options.TemplateExtension);

        return fullPath;
    }
}