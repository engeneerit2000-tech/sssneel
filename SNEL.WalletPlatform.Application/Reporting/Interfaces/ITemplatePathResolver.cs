namespace SNEL.WalletPlatform.Application.Reporting.Interfaces;

public interface ITemplatePathResolver
{
    string Resolve(string templateName);
}