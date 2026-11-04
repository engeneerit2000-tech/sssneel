using SNEL.WalletPlatform.Application.Reporting.Models;

namespace SNEL.WalletPlatform.Application.Reporting.Interfaces;

public interface IHtmlToPdfGenerator
{
    Task<byte[]> GenerateFromHtmlAsync(
        string html,
        CancellationToken cancellationToken = default,
        PdfPageOptions? pageOptions = null);
}
