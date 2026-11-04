using Microsoft.Extensions.Logging;
using Scriban;
using Scriban.Runtime;
using SNEL.WalletPlatform.Application.Reporting.Interfaces;

namespace SNEL.WalletPlatform.Infrastructure.Reporting.Templating;

public class ScribanTemplateRenderer:ITemplateRednerer
{
    private readonly ITemplatePathResolver _templatePathResolver;
    private readonly ILogger<ScribanTemplateRenderer> _logger;

    public ScribanTemplateRenderer(
        ITemplatePathResolver templatePathResolver,
        ILogger<ScribanTemplateRenderer> logger)
    {
        _templatePathResolver = templatePathResolver;
        _logger = logger;
    }

    public async Task<string> RenderAsync<TModel>(
        string templateName,
        TModel model,
        CancellationToken cancellationToken = default)
    {
        var templatePath = _templatePathResolver.Resolve(templateName);

        if (!File.Exists(templatePath))
            throw new FileNotFoundException($"Template not found: {templatePath}");

        var templateText = await File.ReadAllTextAsync(templatePath, cancellationToken);

        var template = Template.Parse(templateText);

        if (template.HasErrors)
        {
            var errors = string.Join(Environment.NewLine, template.Messages.Select(x => x.Message));
            _logger.LogError("Scriban parse errors for template {TemplateName}: {Errors}", templateName, errors);
            throw new InvalidOperationException(
                $"Failed to parse template '{templateName}'. {Environment.NewLine}{errors}");
        }

        var scriptObject = new ScriptObject();

        scriptObject.Import(new { model }, renamer: member => member.Name);

        var context = new TemplateContext
        {
            MemberRenamer = member => member.Name
        };
        context.PushGlobal(scriptObject);

        var result = await template.RenderAsync(context);

        return result;
    }
}