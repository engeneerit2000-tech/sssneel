namespace SNEL.WalletPlatform.Application.Reporting.Interfaces;

public interface ITemplateRednerer
{

    Task<string> RenderAsync<TModel>(
        string templateName,
        TModel model,
        CancellationToken cancellationToken = default);
}
