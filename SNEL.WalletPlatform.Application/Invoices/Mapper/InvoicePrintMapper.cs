using AutoMapper;
using SNEL.WalletPlatform.Application.Invoices.Models.Output;
using SNEL.WalletPlatform.Application.Invoicing.Models;
using SNEL.WalletPlatform.Domain.Billing;
using System.Globalization;
using System.Linq;
using System.Text;
using ZXing;
using ZXing.QrCode;
using ZXing.Rendering;

namespace SNEL.WalletPlatform.Application.Invoices.Mapper;

/// <summary>
/// Input for mapping an invoice and its JSON invoice_info to the print model.
/// </summary>
public sealed record InvoicePrintMapInput(
    InvoiceEntity Entity,
    InvoiceInfoModel? Info,
    IReadOnlyCollection<InvoiceEntity> CustomerOpenInvoices,
    string LanguageCode);

/// <summary>
/// Maps from invoice entity and parsed invoice_info JSON to <see cref="InvoicePrintModel"/>.
/// </summary>
public sealed class InvoicePrintMapper : Profile
{
    public InvoicePrintMapper()
    {
        CreateMap<InvoicePrintMapInput, InvoicePrintModel>()
            .ConvertUsing(input => MapToPrintModel(input));
    }

    private static InvoicePrintModel MapToPrintModel(InvoicePrintMapInput input)
    {
        var entity = input.Entity;
        var info = input.Info;
        var invoiceRef = entity.InvoiceRef?.Trim() ?? string.Empty;
        var invoiceDate = entity.IssueDate.UtcDateTime;
        var currencyCode = (entity.Currency?.Abbreviation ?? string.Empty).Trim().ToUpperInvariant();
        var isCdf = string.Equals(currencyCode, "CDF", StringComparison.OrdinalIgnoreCase);
        var totalAmountFc = isCdf ? entity.AmountTotal : info?.AmountFc;
        var totalAmountUs = isCdf ? info?.AmountUsd : entity.AmountTotal;

        string? invoiceBarcodeBase64 = null;
        var invoiceNum = invoiceRef;
        var pa = (info?.Pa ?? info?.CustomerNum ?? string.Empty).Trim();
        var directionCode = ExtractDirectionCode(entity.BillingRef);
        var totalAmount = isCdf
            ? totalAmountFc
            : totalAmountUs ?? totalAmountFc;
        var amountText = FormatQrAmount(totalAmount);

        if (!string.IsNullOrWhiteSpace(invoiceNum)
            && !string.IsNullOrWhiteSpace(pa)
            && !string.IsNullOrWhiteSpace(directionCode)
            && !string.IsNullOrWhiteSpace(currencyCode))
        {
            var qrPayload = string.Join('\n', invoiceNum, pa, directionCode, amountText, currencyCode);
            var writer = new BarcodeWriterSvg
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new QrCodeEncodingOptions
                {
                    Height = 150,
                    Width = 150,
                    Margin = 2,
                    CharacterSet = "UTF-8"
                }
            };

            var svgImage = writer.Write(qrPayload);
            var svgContent = svgImage.Content;
            invoiceBarcodeBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(svgContent));
        }

        var customerOpenInvoices = input.CustomerOpenInvoices?
            .Select(x => new CustomerOpenInvoicePrintModel
            {
                InvoiceRef = x.InvoiceRef,
                IssueDate = x.IssueDate.UtcDateTime,
                CurrencyAbbreviation = x.Currency.Abbreviation,
                AmountOutstanding = x.AmountOutstanding
            })
            .ToArray() ?? Array.Empty<CustomerOpenInvoicePrintModel>();

        var customerOpenInvoicesTotalOutstanding = customerOpenInvoices.Sum(x => x.AmountOutstanding);

        return new InvoicePrintModel
        {
            LanguageCode = input.LanguageCode,
            InvoiceNum = invoiceRef,
            InvoiceTitle = ResolveInvoiceTitle(invoiceRef, input.LanguageCode),
            InvoiceDate = invoiceDate,
            PrintDate = DateTime.UtcNow.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
            CustomerName = info?.CustomerName ?? string.Empty,
            CenterDesignation = info?.CenterDesignation,
            Pa = info?.CustomerNum ?? string.Empty,
            PointOfSaleName = info?.PointOfSaleName,
            CustomerTypeName = info?.CustomerTypeName,
            SubstationCode = info?.SubstationCode,
            TariffType = info?.TariffType,
            ClientPropertyNumber = info?.ClientPropertyNumber,
            ClientStreet = info?.ClientStreet,
            ClientCommune = info?.ClientCommune,
            MeterNumber = info?.MeterNumber,
            CalculationCoefficient = info?.CalculationCoefficient,
            PreviousBillingIndex = info?.PreviousBillingIndex,
            CurrentBillingIndex = info?.CurrentBillingIndex,
            Consumption = info?.Consumption,
            DiscountCode = info?.DiscountCode,
            BilledConsumption = info?.BilledConsumption,
            BillingYear = entity.IssueDate.UtcDateTime.Year,
            BillingMonth = entity.IssueDate.UtcDateTime.Month,
            AmountExcludingTaxFc = info?.AmountExcludingTaxFc,
            AmountExcludingTaxUs = info?.AmountExcludingTaxUsd,
            ProvincialTax = info?.ProvincialTax,
            Vat = info?.Vat,
            AmountFc = isCdf ? entity.AmountTotal : info?.AmountFc,
            InvoiceCurrencyCode = entity.Currency?.Abbreviation,
            SubtotalFc = info?.SubtotalFc,
            TaxAmountFc = ResolveTaxAmountFc(info),
            TotalAmountFc = totalAmountFc,
            TotalAmountUs = totalAmountUs,
            Lines = [],
            CustomerOpenInvoices = customerOpenInvoices,
            CustomerOpenInvoicesTotalAmountOutstanding = customerOpenInvoicesTotalOutstanding,
            InvoiceBarcodeBase64 = invoiceBarcodeBase64
        };
    }

    private static decimal? ResolveTaxAmountFc(InvoiceInfoModel? info)
    {
        if (info is null)
            return null;

        if (!info.Vat.HasValue && !info.ProvincialTax.HasValue)
            return null;

        return (info.Vat ?? 0) + (info.ProvincialTax ?? 0);
    }

    private static string? ExtractDirectionCode(string? billingRef)
    {
        if (string.IsNullOrWhiteSpace(billingRef))
            return null;

        var trimmed = billingRef.Trim();
        var dashIndex = trimmed.IndexOf('-');
        return dashIndex > 0 ? trimmed[..dashIndex].Trim() : trimmed;
    }

    private static string FormatQrAmount(decimal? amount) =>
        amount.HasValue
            ? decimal.Truncate(amount.Value).ToString("0", CultureInfo.InvariantCulture)
            : "0";

    private static string ResolveInvoiceTitle(string invoiceRef, string? languageCode)
    {
        var isFrench = string.Equals(languageCode, "fr", StringComparison.OrdinalIgnoreCase);

        if (invoiceRef.StartsWith("NDC", StringComparison.OrdinalIgnoreCase))
            return isFrench ? "NOTE DE DEBIT CREDIT" : "DEBIT/CREDIT NOTE";

        return isFrench
            ? "FACTURE D'ENERGIE ELECTRIQUE BASSE TENSION"
            : "LOW VOLTAGE ELECTRICITY INVOICE";
    }
}
