using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SNEL.WalletPlatform.Application.Invoices.Interfaces;
using SNEL.WalletPlatform.Application.Invoices.Models.Input;
using SNEL.WalletPlatform.Domain.Billing;
using SNEL.WalletPlatform.Domain.Invoicing;
using SNEL.WalletPlatform.Domain.Payments;

namespace SNEL.WalletPlatform.Application.Tests;

internal sealed class FakeInvoiceRepository(IEnumerable<InvoiceEntity> invoices)
    : FakeEntityRepository<InvoiceEntity, Guid>(invoices), IInvoiceRepository
{
    public Task<InvoiceEntity?> GetByIdWithBillingServiceAsync(Guid id) =>
        Task.FromResult(Items.FirstOrDefault(x => x.Id == id));

    public Task<IReadOnlyCollection<InvoiceEntity>> GetByBillingServiceIdAndBillingRefAsync(
        long billingServiceId,
        string billingRef,
        bool unpaidOnly = true,
        CancellationToken cancellationToken = default)
    {
        var normalizedBillingRef = billingRef.Trim();

        var query = Items.Where(x =>
            x.BillingServiceId == billingServiceId &&
            x.BillingRef == normalizedBillingRef);

        if (unpaidOnly)
        {
            query = query.Where(x => x.AmountOutstanding > 0);
        }

        var result = unpaidOnly
            ? query.OrderByDescending(x => x.IssueDate).ToArray()
            : query.OrderBy(x => x.AmountOutstanding > 0 ? 0 : 1)
                .ThenByDescending(x => x.IssueDate)
                .ToArray();

        return Task.FromResult<IReadOnlyCollection<InvoiceEntity>>(result);
    }

    public Task<IReadOnlyCollection<InvoiceEntity>> GetInquiryPageAsync(
        InvoiceInquiryQueryInternalModel model,
        CancellationToken cancellationToken = default)
    {
        var query = ApplyInquiryFilters(model);
        var sortBy = model.SortBy?.Trim().ToLowerInvariant();

        var ordered = (sortBy, model.IsAscending) switch
        {
            ("invoiceref", true) => query.OrderBy(invoice => invoice.InvoiceRef),
            ("invoiceref", false) => query.OrderByDescending(invoice => invoice.InvoiceRef),
            ("amounttotal", true) => query.OrderBy(invoice => invoice.AmountTotal),
            ("amounttotal", false) => query.OrderByDescending(invoice => invoice.AmountTotal),
            ("amountpaid", true) => query.OrderBy(invoice => invoice.AmountPaid + GetPlatformPaidAmount(invoice)),
            ("amountpaid", false) => query.OrderByDescending(invoice => invoice.AmountPaid + GetPlatformPaidAmount(invoice)),
            ("amountoutstanding", true) => query.OrderBy(invoice => invoice.AmountTotal - invoice.AmountPaid - GetPlatformPaidAmount(invoice)),
            ("amountoutstanding", false) => query.OrderByDescending(invoice => invoice.AmountTotal - invoice.AmountPaid - GetPlatformPaidAmount(invoice)),
            ("currencyid", true) => query.OrderBy(invoice => invoice.CurrencyId),
            ("currencyid", false) => query.OrderByDescending(invoice => invoice.CurrencyId),
            ("duedate", true) => query.OrderBy(invoice => invoice.DueDate),
            ("duedate", false) => query.OrderByDescending(invoice => invoice.DueDate),
            ("status", true) => query.OrderBy(invoice => invoice.Status),
            ("status", false) => query.OrderByDescending(invoice => invoice.Status),
            ("createdatutc", true) => query.OrderBy(invoice => invoice.CreatedAtUtc),
            ("createdatutc", false) => query.OrderByDescending(invoice => invoice.CreatedAtUtc),
            ("updatedatutc", true) => query.OrderBy(invoice => invoice.UpdatedAtUtc),
            ("updatedatutc", false) => query.OrderByDescending(invoice => invoice.UpdatedAtUtc),
            ("id", true) => query.OrderBy(invoice => invoice.Id),
            ("id", false) => query.OrderByDescending(invoice => invoice.Id),
            (_, true) => query.OrderBy(invoice => invoice.IssueDate),
            _ => query.OrderByDescending(invoice => invoice.IssueDate)
        };

        var offset = ((long)model.PageNumber - 1L) * model.PageSize;
        var items = offset > int.MaxValue
            ? Array.Empty<InvoiceEntity>()
            : ordered.ThenBy(invoice => invoice.Id)
                .Skip((int)offset)
                .Take(model.PageSize)
                .ToArray();

        return Task.FromResult<IReadOnlyCollection<InvoiceEntity>>(items);
    }

    public Task<(
        int TotalCount,
        IReadOnlyDictionary<string, decimal> TotalOutstanding)> GetInquirySummaryAsync(
        InvoiceInquiryQueryInternalModel model,
        CancellationToken cancellationToken = default)
    {
        var items = ApplyInquiryFilters(model).ToArray();
        var totalOutstanding = items
            .GroupBy(invoice => invoice.Currency.Code, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.Sum(GetAdjustedOutstanding),
                StringComparer.OrdinalIgnoreCase);

        return Task.FromResult<(
            int,
            IReadOnlyDictionary<string, decimal>)>((items.Length, totalOutstanding));
    }

    private IEnumerable<InvoiceEntity> ApplyInquiryFilters(InvoiceInquiryQueryInternalModel model)
    {
        var query = Items.Where(invoice =>
            invoice.BillingServiceId == model.BillingServiceId &&
            invoice.BillingRef == model.BillingRef);

        if (model.UnpaidOnly)
        {
            query = query.Where(invoice => GetAdjustedOutstanding(invoice) > 0);
        }

        if (!string.IsNullOrWhiteSpace(model.Search))
        {
            query = query.Where(invoice =>
                invoice.InvoiceRef.Contains(model.Search, StringComparison.OrdinalIgnoreCase) ||
                invoice.Status.Contains(model.Search, StringComparison.OrdinalIgnoreCase));
        }

        return query;
    }

    private static decimal GetPlatformPaidAmount(InvoiceEntity invoice) =>
        invoice.PaymentOrderItems?
            .Where(item =>
                item.ItemType == OrderItemType.PayBill &&
                item.Status == PaymentOrderItemStatus.Paid)
            .Sum(item => item.Amount) ?? 0m;

    private static decimal GetAdjustedOutstanding(InvoiceEntity invoice) =>
        invoice.AmountTotal - invoice.AmountPaid - GetPlatformPaidAmount(invoice);

    public Task<InvoiceEntity?> GetByInvoiceRefAsync(string invoiceRef)
    {
        var normalized = invoiceRef.Trim().ToLowerInvariant();
        return Task.FromResult(Items.FirstOrDefault(x => x.InvoiceRef.ToLowerInvariant() == normalized));
    }

    public Task<InvoiceEntity?> GetByInvoiceRefWithBillingServiceAsync(
        string invoiceRef,
        CancellationToken cancellationToken = default)
    {
        var normalized = invoiceRef.Trim().ToLowerInvariant();
        return Task.FromResult(Items.FirstOrDefault(x => x.InvoiceRef.ToLowerInvariant() == normalized));
    }

    public Task<bool> ExistsByInvoiceRefAsync(string invoiceRef)
    {
        var normalized = invoiceRef.Trim().ToLowerInvariant();
        return Task.FromResult(Items.Any(x => x.InvoiceRef.ToLowerInvariant() == normalized));
    }

    public Task<IReadOnlyCollection<InvoiceEntity>> GetCustomerOpenInvoicesAsync(string billingRef)
    {
        var normalizedBillingRef = billingRef.Trim().ToLowerInvariant();

        var result = Items
            .Where(x =>
                x.BillingRef.ToLowerInvariant() == normalizedBillingRef &&
                (x.Status == InvoiceStatus.Issued.ToString() ||
                 x.Status == InvoiceStatus.PartiallyPaid.ToString()))
            .OrderByDescending(x => x.IssueDate)
            .ToArray();

        return Task.FromResult<IReadOnlyCollection<InvoiceEntity>>(result);
    }

    public Task<int> CountAsync(InvoicesQueryInternalModel normalizedQuery)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyCollection<InvoiceEntity>> GetPagedAsync(InvoicesQueryInternalModel normalizedQuery)
    {
        throw new NotImplementedException();
    }

    public Task<List<InvoiceEntity>> GetByIDsWithBillingServiceWithBillarPartnerAsync(List<Guid> IDs)
    {
        var idSet = IDs.ToHashSet();
        return Task.FromResult(Items.Where(x => idSet.Contains(x.Id)).ToList());
    }
}
