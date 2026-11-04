using Microsoft.Extensions.Localization;
using SNEL.WalletPlatform.Application.BillingServices.Interfaces;
using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Common.Localization;
using SNEL.WalletPlatform.Application.Interfaces.UnitOfWork;
using SNEL.WalletPlatform.Domain.Billing;

namespace SNEL.WalletPlatform.Application.BillingServices.Services;

public sealed class BillingInvoiceRefResolver(
    IUnitOfWork unitOfWork,
    IStringLocalizer<SharedResource> localizer) : IBillingInvoiceRefResolver
{
    public async Task<Result<InvoiceEntity>> ResolveInvoiceByRefAsync(
        string invoiceRef,
        bool requireOutstanding,
        CancellationToken cancellationToken = default)
    {
        var normalizedInvoiceRef = invoiceRef?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedInvoiceRef))
        {
            return Result<InvoiceEntity>.Failure(
                localizer[LocalizationKeys.BillingInquiryInvoiceRefRequired],
                AppHttpStatusCodes.BadRequest);
        }

        var invoice = await unitOfWork.InvoiceRepository.GetByInvoiceRefWithBillingServiceAsync(
            normalizedInvoiceRef,
            cancellationToken);

        if (invoice is null)
        {
            return Result<InvoiceEntity>.Failure(
                localizer[LocalizationKeys.BillingInquiryInvoiceNotFound, normalizedInvoiceRef],
                AppHttpStatusCodes.NotFound);
        }

        var billingService = invoice.BillingService;
        var billerCode = billingService.Biller.Code;
        var serviceCode = billingService.Code;

        var billingServiceValidation = TryValidateBillingServiceForInvoiceRefInquiry(
            billingService,
            billerCode,
            serviceCode);
        if (billingServiceValidation is not null)
        {
            return billingServiceValidation;
        }

        if (requireOutstanding && invoice.AmountOutstanding <= 0)
        {
            return Result<InvoiceEntity>.Failure(
                localizer[LocalizationKeys.InvoiceHasNoDueAmount, invoice.InvoiceRef],
                AppHttpStatusCodes.BadRequest);
        }

        return Result<InvoiceEntity>.Success(invoice);
    }

    private Result<InvoiceEntity>? TryValidateBillingServiceForInvoiceRefInquiry(
        BillingServiceEntity billingService,
        string billerCode,
        string serviceCode)
    {
        if (!billingService.Active || billingService.Biller?.Partner.Active != true)
        {
            return Result<InvoiceEntity>.Failure(
                localizer[LocalizationKeys.BillingInquiryNotSupported, billerCode, serviceCode],
                AppHttpStatusCodes.BadRequest);
        }

        if (!billingService.SupportInquiry)
        {
            return Result<InvoiceEntity>.Failure(
                localizer[LocalizationKeys.BillingInquiryNotSupported, billerCode, serviceCode],
                AppHttpStatusCodes.BadRequest);
        }

        return null;
    }
}
