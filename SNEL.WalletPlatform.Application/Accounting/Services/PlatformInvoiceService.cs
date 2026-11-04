using Microsoft.Extensions.Localization;
using SNEL.WalletPlatform.Application;
using SNEL.WalletPlatform.Application.Accounting;
using SNEL.WalletPlatform.Application.Accounting.Interfaces;
using SNEL.WalletPlatform.Application.Accounting.Models.Output;
using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Common.Localization;
using SNEL.WalletPlatform.Application.Interfaces.Auth;
using SNEL.WalletPlatform.Application.Interfaces.UnitOfWork;
using SNEL.WalletPlatform.Application.Reporting.Interfaces;
using SNEL.WalletPlatform.Domain.Accounting;
using System.Globalization;
using System.Text;
using ZXing;
using ZXing.Rendering;

namespace SNEL.WalletPlatform.Application.Accounting.Services;

public sealed class PlatformInvoiceService(
    IUnitOfWork unitOfWork,
    ICurrentUserContext currentUserContext,
    IStringLocalizer<SharedResource> localizer,
    ITemplateRednerer templateRenderer,
    IHtmlToPdfGenerator htmlToPdfConverter) : IPlatformInvoiceService
{
    private const string PlatformPartnerTypeCode = "PLATFORM";

    private sealed record UserPartnerAccess(long PartnerId, bool IsPlatform);

    public async Task<Result<byte[]>> GetPlatformInvoicePdfAsync(
        long platformInvoiceId,
        string? acceptLanguage,
        CancellationToken cancellationToken = default)
    {
        if (platformInvoiceId <= 0)
        {
            return Result<byte[]>.Failure(
                localizer[LocalizationKeys.PlatformInvoiceInvalidId],
                AppHttpStatusCodes.BadRequest);
        }

        var partnerAccess = await GetCurrentUserPartnerAccessAsync(cancellationToken);
        if (partnerAccess is null)
        {
            return Result<byte[]>.Failure(
                localizer[LocalizationKeys.CustomerProfileNotFound],
                AppHttpStatusCodes.NotFound);
        }

        var invoice = await unitOfWork.PlatformInvoiceRepository.GetByIdWithPrintContextAsync(
            platformInvoiceId,
            cancellationToken);

        if (invoice is null)
        {
            return Result<byte[]>.Failure(
                localizer[LocalizationKeys.PlatformInvoiceNotFound, platformInvoiceId],
                AppHttpStatusCodes.NotFound);
        }

        if (!partnerAccess.IsPlatform
            && !PlatformInvoiceContextResolver.ViewerMatchesResolvedParties(invoice, partnerAccess.PartnerId))
        {
            return Result<byte[]>.Failure(
                localizer[LocalizationKeys.PlatformInvoiceNotFound, platformInvoiceId],
                AppHttpStatusCodes.NotFound);
        }

        var model = await BuildPrintModelAsync(invoice, acceptLanguage, cancellationToken);

        var html = await templateRenderer.RenderAsync(
            "PlatformInvoice/platform-invoice",
            model,
            cancellationToken);
        var pdfBytes = await htmlToPdfConverter.GenerateFromHtmlAsync(html, cancellationToken);

        return Result<byte[]>.Success(
            pdfBytes,
            localizer[LocalizationKeys.FileDownloadSuccess],
            AppHttpStatusCodes.Ok);
    }

    private async Task<PlatformInvoicePrintModel> BuildPrintModelAsync(
        PlatformInvoiceEntity invoice,
        string? acceptLanguage,
        CancellationToken cancellationToken)
    {
        var currency =
            invoice.Currency?.Abbreviation
            ?? invoice.Currency?.Code
            ?? string.Empty;

        var enrichment = await PlatformInvoiceContextResolver.ResolveForPrintAsync(
            invoice,
            unitOfWork,
            localizer,
            cancellationToken);

        var barcodeSource = invoice.Id.ToString(CultureInfo.InvariantCulture);
        var writer = new BarcodeWriterSvg
        {
            Format = BarcodeFormat.CODE_128,
            Options = new ZXing.Common.EncodingOptions
            {
                Height = 60,
                Width = 240,
                Margin = 2
            }
        };
        var svgImage = writer.Write(barcodeSource);
        var barcodeBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(svgImage.Content));

        var invoicedPartnerName = PlatformInvoiceContextResolver.PartnerDisplayName(invoice.Partner);

        var invoiceDateDisplay =
            invoice.InvoiceDate.UtcDateTime.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

        return new PlatformInvoicePrintModel
        {
            PlatformInvoiceId = invoice.Id,
            AcceptLanguage = acceptLanguage,
            InvoiceDate = invoice.InvoiceDate,
            InvoiceDateDisplay = invoiceDateDisplay,
            CustomerDisplayName = enrichment.CustomerDisplayName,
            CounterpartyPartnerDisplayName = enrichment.SecondaryPartnerDisplayName,
            InvoicedPartnerDisplayName = invoicedPartnerName,
            Description = enrichment.Description,
            Amount = invoice.Amount,
            Tax = invoice.Tax,
            Total = invoice.Total,
            Currency = currency,
            InvoiceBarcodeBase64 = barcodeBase64
        };
    }

    private async Task<UserPartnerAccess?> GetCurrentUserPartnerAccessAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(currentUserContext.UserId))
        {
            return null;
        }

        var user = await unitOfWork.UserRepository.GetByIamRefWithPartnerAsync(
            currentUserContext.UserId,
            cancellationToken);
        if (user?.PartnerId is null)
        {
            return null;
        }

        var partnerTypeCode = user.Partner?.PartnerType?.Code;
        var isPlatform = string.Equals(partnerTypeCode, PlatformPartnerTypeCode, StringComparison.OrdinalIgnoreCase);
        return new UserPartnerAccess(user.PartnerId.Value, isPlatform);
    }
}
