using Gotenberg.Sharp.API.Client;
using Gotenberg.Sharp.API.Client.Domain.Builders;
using Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;
using Gotenberg.Sharp.API.Client.Domain.Dimensions;
using SNEL.WalletPlatform.Application.Reporting.Interfaces;
using SNEL.WalletPlatform.Application.Reporting.Models;

namespace SNEL.WalletPlatform.Infrastructure.Reporting.PDF;

public class GotenbergHtmlToPdfGenerator : IHtmlToPdfGenerator
{
    private readonly GotenbergSharpClient _gotenbergClient;

    public GotenbergHtmlToPdfGenerator(GotenbergSharpClient gotenbergClient)
    {
        _gotenbergClient = gotenbergClient;
    }

    public async Task<byte[]> GenerateFromHtmlAsync(
        string html,
        CancellationToken cancellationToken = default,
        PdfPageOptions? pageOptions = null)
    {
        var builder = new HtmlRequestBuilder()
            .AddDocument(doc => doc.SetBody(html))
            .WithPageProperties(pp => ConfigurePageProperties(pp, pageOptions));

        var request = await builder.BuildAsync();
        await using var responseStream = await _gotenbergClient.HtmlToPdfAsync(request);

        using var memoryStream = new MemoryStream();
        await responseStream.CopyToAsync(memoryStream, cancellationToken);

        return memoryStream.ToArray();
    }

    private static PagePropertyBuilder ConfigurePageProperties(
        PagePropertyBuilder pp,
        PdfPageOptions? pageOptions)
    {
        pp.UseChromeDefaults();

        if (pageOptions is null)
            return pp;

        if (pageOptions.NoMargins)
            pp.SetMargins(Margins.None);

        if (pageOptions.WidthMillimeters is { } widthMm)
            pp.SetPaperWidth(Dimension.FromMillimeters(widthMm));

        if (pageOptions.HeightMillimeters is { } heightMm && !pageOptions.SinglePage)
            pp.SetPaperHeight(Dimension.FromMillimeters(heightMm));

        if (pageOptions.SinglePage)
            pp.SetSinglePage(true);

        pp.SetPreferCssPageSize(pageOptions.PreferCssPageSize);
        pp.SetPrintBackground(pageOptions.PrintBackground);

        return pp;
    }
}
