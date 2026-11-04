using System.Globalization;
using AutoMapper;
using Microsoft.Extensions.Localization;
using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Common.Localization;
using SNEL.WalletPlatform.Application.Interfaces.UnitOfWork;
using SNEL.WalletPlatform.Application.Invoices.Interfaces;
using SNEL.WalletPlatform.Application.Invoices.Mapper;
using SNEL.WalletPlatform.Application.Invoices.Models.Input;
using SNEL.WalletPlatform.Application.Invoices.Models.Output;
using SNEL.WalletPlatform.Application.Invoicing.Models;
using SNEL.WalletPlatform.Application.Reporting.Interfaces;
using SNEL.WalletPlatform.Domain.Customers;

namespace SNEL.WalletPlatform.Application.Invoices.Services;

public sealed class InvoiceService(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IStringLocalizer<SharedResource> localizer,
    ITemplateRednerer templateRenderer,
    IHtmlToPdfGenerator htmlToPdfConverter) : IInvoiceService
{
    public async Task<Result<PagedResult<InvoiceOutputModel>>> GetCustomerSubscriptionInvoicesAsync(InvoicesQueryInputModel model)
    {
        var customerSubscription = await unitOfWork.CustomerSubscriptionRepository
            .GetByIdWithRelationsAsync(model.CustomerSubscriptionId);

        if (customerSubscription is null)
        {
            return Result<PagedResult<InvoiceOutputModel>>.Failure(
                localizer[LocalizationKeys.BillingInquirySubscriptionNotFound, model.CustomerSubscriptionId],
                AppHttpStatusCodes.NotFound);
        }
        var normalizedQuery = NormalizeQuery(model, customerSubscription);
        var invoices = await unitOfWork.InvoiceRepository.GetPagedAsync(normalizedQuery);

        var totalCount = await unitOfWork.InvoiceRepository.CountAsync(normalizedQuery);
        var items = mapper.Map<InvoiceOutputModel[]>(invoices);

        return Result<PagedResult<InvoiceOutputModel>>.Success(
            new PagedResult<InvoiceOutputModel>(items, totalCount));
    }

    private InvoicesQueryInternalModel NormalizeQuery(InvoicesQueryInputModel model, CustomerSubscriptionEntity customerSubscription)
    {
        var mapped = mapper.Map<InvoicesQueryInternalModel>(model);

        mapped.PageNumber = mapped.PageNumber <= 0 ? 1 : mapped.PageNumber;
        mapped.PageSize = mapped.PageSize <= 0 ? 20 : Math.Min(mapped.PageSize, 100);
        mapped.SortBy = string.IsNullOrWhiteSpace(mapped.SortBy) ? null : mapped.SortBy.Trim();
        mapped.BillingServiceId = customerSubscription.BillingServiceId;
        mapped.BillingRefValue = customerSubscription.BillingRefValue;

        return mapped;
    }

    public async Task<Result<byte[]>> GetInvoicePdfAsync(Guid invoiceId, CancellationToken cancellationToken)
    {
        var modelResult = await BuildInvoicePrintModelAsync(invoiceId, cancellationToken);

        if (!modelResult.IsSuccess)
            return Result<byte[]>.Failure(modelResult.Message, modelResult.StatusCode);
        var html = await templateRenderer.RenderAsync("Invoice/invoice", modelResult.Value, cancellationToken);
        var pdfBytes = await htmlToPdfConverter.GenerateFromHtmlAsync(html);

        return Result<byte[]>.Success(
            pdfBytes,
            localizer[LocalizationKeys.FileDownloadSuccess],
            AppHttpStatusCodes.Ok);
    }

    public async Task<Result<string>> GetInvoiceHtmlPreviewAsync(Guid invoiceId, CancellationToken cancellationToken)
    {
        var modelResult = await BuildInvoicePrintModelAsync(invoiceId, cancellationToken);

        if (!modelResult.IsSuccess)
            return Result<string>.Failure(modelResult.Message, modelResult.StatusCode);

        var html = await templateRenderer.RenderAsync("Invoice/invoice", modelResult.Value, cancellationToken);

        return Result<string>.Success(html);
    }

    private async Task<Result<InvoicePrintModel>> BuildInvoicePrintModelAsync(Guid invoiceId, CancellationToken cancellationToken)
    {
        var invoice = await unitOfWork.InvoiceRepository.GetByIdWithBillingServiceAsync(invoiceId);

        if (invoice is null)
        {
            return Result<InvoicePrintModel>.Failure(
                localizer[LocalizationKeys.InvoiceNotFound, invoiceId],
                AppHttpStatusCodes.NotFound);
        }

        var customerOpenInvoices =
            await unitOfWork.InvoiceRepository.GetCustomerOpenInvoicesAsync(invoice.BillingRef);

        var info = InvoiceInfoModel.Parse(invoice.InvoiceInfo);
        var languageCode = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        var input = new InvoicePrintMapInput(invoice, info, customerOpenInvoices, languageCode);
        var model = mapper.Map<InvoicePrintModel>(input);

        return Result<InvoicePrintModel>.Success(model);
    }
}
