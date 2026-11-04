using Microsoft.EntityFrameworkCore;
using SNEL.WalletPlatform.Application.Accounting.Models.Input;
using SNEL.WalletPlatform.Application.Invoices.Interfaces;
using SNEL.WalletPlatform.Application.Invoices.Models.Input;
using SNEL.WalletPlatform.Domain.Accounting;
using SNEL.WalletPlatform.Domain.Billing;
using SNEL.WalletPlatform.Domain.Payments;
using SNEL.WalletPlatform.Domain.Invoicing;
using SNEL.WalletPlatform.Infrastructure.Persistence.Repositories.Base;
using System.Linq.Expressions;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Repositories;

public sealed class InvoiceRepository(ApplicationDbContext context)
    : EntityRepository<InvoiceEntity, Guid>(context), IInvoiceRepository
{
    private static readonly Dictionary<string, Expression<Func<InvoiceEntity, object>>> AllowedOrderBy =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["createdAtUtc"] = x => x.CreatedAtUtc,
            ["updatedAtUtc"] = x => x.UpdatedAtUtc,
            ["id"] = x => x.Id
        };

    private static readonly Dictionary<string, Expression<Func<InvoiceEntity, object>>> InquiryAllowedOrderBy =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["invoiceRef"] = x => x.InvoiceRef,
            ["amountTotal"] = x => x.AmountTotal,
            ["amountPaid"] = x => x.AmountPaid +
                (x.PaymentOrderItems!
                    .Where(item =>
                        item.ItemType == OrderItemType.PayBill &&
                        item.Status == PaymentOrderItemStatus.Paid)
                    .Sum(item => (decimal?)item.Amount) ?? 0m),
            ["amountOutstanding"] = x => x.AmountTotal - x.AmountPaid -
                (x.PaymentOrderItems!
                    .Where(item =>
                        item.ItemType == OrderItemType.PayBill &&
                        item.Status == PaymentOrderItemStatus.Paid)
                    .Sum(item => (decimal?)item.Amount) ?? 0m),
            ["currencyId"] = x => x.CurrencyId,
            ["issueDate"] = x => x.IssueDate,
            ["dueDate"] = x => x.DueDate!,
            ["status"] = x => x.Status,
            ["createdAtUtc"] = x => x.CreatedAtUtc,
            ["updatedAtUtc"] = x => x.UpdatedAtUtc,
            ["id"] = x => x.Id
        };

    public Task<InvoiceEntity?> GetByIdWithBillingServiceAsync(Guid id)
    {
        return context.Invoices
            .Include(x => x.BillingService).ThenInclude(x => x.Biller).ThenInclude(b => b.Partner)
            .Include(x => x.BillingService).ThenInclude(x => x.BillingCategory)
            .Include(x => x.BillingService).ThenInclude(x => x.Translations)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public Task<InvoiceEntity?> GetByInvoiceRefAsync(string invoiceRef)
    {
        var normalized = invoiceRef.Trim().ToLower();
        return context.Invoices.FirstOrDefaultAsync(x => x.InvoiceRef.ToLower() == normalized);
    }

    public Task<InvoiceEntity?> GetByInvoiceRefWithBillingServiceAsync(
        string invoiceRef,
        CancellationToken cancellationToken = default)
    {
        var normalized = invoiceRef.Trim().ToLower();
        return context.Invoices
            .Include(x => x.Currency)
            .Include(x => x.BillingService)
                .ThenInclude(s => s.Biller)
                .ThenInclude(b => b.Partner)
            .FirstOrDefaultAsync(x => x.InvoiceRef.ToLower() == normalized, cancellationToken);
    }

    public async Task<IReadOnlyCollection<InvoiceEntity>> GetCustomerOpenInvoicesAsync(string billingRef)
    {
        var normalizedBillingRef = billingRef.Trim().ToLower();
        return await context.Invoices
            .Include(x => x.Currency)
            .Where(x =>
                x.BillingRef.ToLower() == normalizedBillingRef &&
                (x.Status == InvoiceStatus.Issued.ToString() ||
                 x.Status == InvoiceStatus.PartiallyPaid.ToString()))
            .OrderByDescending(x => x.IssueDate)
            .ToListAsync();
    }

    public Task<bool> ExistsByInvoiceRefAsync(string invoiceRef)
    {
        var normalized = invoiceRef.Trim().ToLower();
        return context.Invoices.AnyAsync(x => x.InvoiceRef.ToLower() == normalized);
    }

    public async Task<IReadOnlyCollection<InvoiceEntity>> GetByBillingServiceIdAndBillingRefAsync(
        long billingServiceId,
        string billingRef,
        bool unpaidOnly = true,
        CancellationToken cancellationToken = default)
    {
        var normalizedBillingRef = billingRef.Trim();

        var query = context.Invoices
            .AsNoTracking()
            .Include(x => x.Currency)
            .Where(x =>
                x.BillingServiceId == billingServiceId &&
                x.BillingRef == normalizedBillingRef);

        if (unpaidOnly)
        {
            query = query.Where(x => x.AmountOutstanding > 0);
            return await query
                .OrderByDescending(x => x.IssueDate)
                .ToListAsync(cancellationToken);
        }

        return await query
            .OrderBy(x => x.AmountOutstanding > 0 ? 0 : 1)
            .ThenByDescending(x => x.IssueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<InvoiceEntity>> GetInquiryPageAsync(
        InvoiceInquiryQueryInternalModel model,
        CancellationToken cancellationToken = default)
    {
        var offset = ((long)model.PageNumber - 1L) * model.PageSize;
        if (offset > int.MaxValue)
        {
            return Array.Empty<InvoiceEntity>();
        }

        var orderedQuery = ApplyOrdering(
            ApplyInquiryFilters(model),
            model.SortBy,
            model.IsAscending,
            InquiryAllowedOrderBy,
            nameof(InvoiceEntity.IssueDate));

        return await orderedQuery
            .ThenBy(invoice => invoice.Id)
            .Skip((int)offset)
            .Take(model.PageSize)
            .Include(invoice => invoice.Currency)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<(
        int TotalCount,
        IReadOnlyDictionary<string, decimal> TotalOutstanding)> GetInquirySummaryAsync(
        InvoiceInquiryQueryInternalModel model,
        CancellationToken cancellationToken = default)
    {
        var summary = await ApplyInquiryFilters(model)
            .GroupBy(invoice => invoice.Currency.Code)
            .Select(group => new
            {
                CurrencyCode = group.Key,
                Count = group.Count(),
                Amount = group.Sum(invoice =>
                    invoice.AmountTotal - invoice.AmountPaid -
                    (invoice.PaymentOrderItems!
                        .Where(item =>
                            item.ItemType == OrderItemType.PayBill &&
                            item.Status == PaymentOrderItemStatus.Paid)
                        .Sum(item => (decimal?)item.Amount) ?? 0m))
            })
            .ToListAsync(cancellationToken);

        return (
            summary.Sum(row => row.Count),
            summary.ToDictionary(
                row => row.CurrencyCode,
                row => row.Amount,
                StringComparer.OrdinalIgnoreCase));
    }

    public async Task<IReadOnlyCollection<InvoiceEntity>> GetPagedAsync(InvoicesQueryInternalModel model)
    {
        var orderedQuery = ApplyOrdering(
            ApplyFilters(model),
            model.SortBy,
            model.IsAscending,
            AllowedOrderBy,
            nameof(InvoiceEntity.CreatedAtUtc));

        return await orderedQuery
            .Skip((model.PageNumber - 1) * model.PageSize)
            .Take(model.PageSize)
            .AsNoTracking()
            .ToListAsync();
    }

    public Task<List<InvoiceEntity>> GetByIDsWithBillingServiceWithBillarPartnerAsync(List<Guid>  IDs)
    {
        return context.Invoices
            .Include(i => i.BillingService)
                .ThenInclude(s => s.Biller)
                .ThenInclude(b => b.Partner)
            .Include(i => i.BillingService)
                .ThenInclude(s => s.Currencies)
            .Where(x => IDs.Contains(x.Id))
            .ToListAsync();
    }

    public Task<int> CountAsync(InvoicesQueryInternalModel model)
    {
        return ApplyFilters(model).CountAsync();
    }

    private IQueryable<InvoiceEntity> ApplyFilters(InvoicesQueryInternalModel model)
    {
        IQueryable<InvoiceEntity> query = context.Invoices
            .Include(x => x.Currency)
            .Include(x => x.PaymentOrderItems)
            .Where(x => x.BillingServiceId == model.BillingServiceId
                && x.BillingRef == model.BillingRefValue
                && x.AmountOutstanding > 0
                && (x.PaymentOrderItems == null || !x.PaymentOrderItems.Any() || x.PaymentOrderItems.Any(o => o.Status != PaymentOrderItemStatus.Paid)));

        if (!string.IsNullOrWhiteSpace(model.Search))
        {
        }

        return query;
    }

    private IQueryable<InvoiceEntity> ApplyInquiryFilters(InvoiceInquiryQueryInternalModel model)
    {
        var query = context.Invoices.Where(invoice =>
            invoice.BillingServiceId == model.BillingServiceId &&
            invoice.BillingRef == model.BillingRef);

        if (model.UnpaidOnly)
        {
            query = query.Where(invoice =>
                invoice.AmountTotal - invoice.AmountPaid -
                (invoice.PaymentOrderItems!
                    .Where(item =>
                        item.ItemType == OrderItemType.PayBill &&
                        item.Status == PaymentOrderItemStatus.Paid)
                    .Sum(item => (decimal?)item.Amount) ?? 0m) > 0m);
        }

        if (!string.IsNullOrWhiteSpace(model.Search))
        {
            var normalizedSearch = model.Search.ToLower();
            query = query.Where(invoice =>
                invoice.InvoiceRef.ToLower().Contains(normalizedSearch) ||
                invoice.Status.ToLower().Contains(normalizedSearch));
        }

        return query;
    }
}
