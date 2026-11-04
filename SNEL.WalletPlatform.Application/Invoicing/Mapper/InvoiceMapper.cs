using System.Globalization;
using System.Text.Json;
using AutoMapper;
using SNEL.WalletPlatform.Application.Invoicing.Models;
using SNEL.WalletPlatform.Domain.Billing;
using SNEL.WalletPlatform.Domain.Invoicing;

namespace SNEL.WalletPlatform.Application.Invoicing.Mapper;

/// <summary>
/// Maps between Oracle invoice row model and Invoice entity (like CustomerMapper).
/// </summary>
public sealed class InvoiceMapper : Profile
{
    public InvoiceMapper()
    {
        CreateMap<OracleInvoiceMapInput, InvoiceEntity>()
            .ConstructUsing((src, _) => MapToEntity(src));
    }

    private static InvoiceEntity MapToEntity(OracleInvoiceMapInput input)
    {
        var r = input.Row;
        var invoiceRef = (r.InvoiceNum ?? "").Trim();
        var billingRef = $"{r.RegionCode}-{r.CustomerNum?.Trim()}";

        var isUsd = string.Equals(r.InvoiceCurrencyCode, "USD", StringComparison.OrdinalIgnoreCase);
        var amountTotal = isUsd
            ? (r.AmountUsd ?? 0)
            : (r.AmountFc ?? 0);
        var amountPaid = isUsd
            ? (r.PaidUsd ?? 0)
            : (r.PaidFc ?? 0);
        var amountOutstanding = amountTotal - amountPaid;
        if (amountOutstanding < 0)
            amountOutstanding = 0;

        var issueDate = r.InvoiceDate.HasValue
            ? new DateTimeOffset(r.InvoiceDate.Value, TimeSpan.Zero)
            : DateTimeOffset.UtcNow;
        var dueDate = issueDate.AddDays(30);

        var statusEnum = amountPaid >= amountTotal
            ? InvoiceStatus.Paid
            : amountPaid > 0
                ? InvoiceStatus.PartiallyPaid
                : InvoiceStatus.Issued;

        var status = statusEnum.ToString();
        var invoiceInfo = r.ToInvoiceInfoJson();

        var invoiceUpdateDate = r.EffectiveInvoiceUpdateDate.HasValue ? r.EffectiveInvoiceUpdateDate.Value: (DateTimeOffset?)null;
        var originCustomerId = r.CustomerId?.ToString(CultureInfo.InvariantCulture);

        return InvoiceEntity.Create(
            input.BillingServiceId,
            billingRef,
            r.InvoiceId,
            invoiceRef,
            amountOutstanding,
            amountPaid,
            amountTotal,
            input.CurrencyId,
            issueDate,
            dueDate,
            status,
            invoiceInfo,
            invoiceUpdateDate,
            originCustomerId);
    }
}

