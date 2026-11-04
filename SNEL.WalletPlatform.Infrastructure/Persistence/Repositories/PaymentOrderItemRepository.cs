using Microsoft.EntityFrameworkCore;
using SNEL.WalletPlatform.Application.Payments.Interfaces;
using SNEL.WalletPlatform.Application.Payments.Models.Input;
using SNEL.WalletPlatform.Domain.Payments;
using SNEL.WalletPlatform.Infrastructure.Persistence.Repositories.Base;
using System.Linq.Expressions;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Repositories;

public sealed class PaymentOrderItemRepository(ApplicationDbContext context)
    : EntityRepository<PaymentOrderItemEntity, Guid>(context), IPaymentOrderItemRepository
{
    private static readonly Dictionary<string, Expression<Func<PaymentOrderItemEntity, object>>> AllowedOrderBy =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["createdAtUtc"] = x => x.CreatedAtUtc,
            ["updatedAtUtc"] = x => x.UpdatedAtUtc,
            ["id"] = x => x.Id
        };
    public  Task<List<PaymentOrderItemEntity>> GetByInvoiceIDAndStatusAsync(Guid InvoiceID, PaymentOrderItemStatus status)
    {
        var items = Context.PaymentOrderItems
            .Where(x => x.InvoiceId == InvoiceID && x.Status == status)
            ;
        return items.ToListAsync();

    }


    public Task<List<PaymentOrderItemEntity>> GetByInvoicesIDsAndStatusAsync(List<Guid> InvoicesIDs, PaymentOrderItemStatus status)
    {
        var items = Context.PaymentOrderItems
            .Where(x => x.InvoiceId.HasValue && InvoicesIDs.Contains(x.InvoiceId.Value) && x.Status == status)
            ;
        return items.ToListAsync();
    }
    public async Task<IReadOnlyCollection<PaymentOrderItemEntity>> GetByInvoicesAndTypeAndStatusAsync(
        IReadOnlyCollection<Guid> invoiceIds,
        OrderItemType itemType,
        PaymentOrderItemStatus status,
        CancellationToken cancellationToken = default)
    {
        return await context.PaymentOrderItems
            .Where(poi =>
                poi.InvoiceId != null &&
                invoiceIds.Contains(poi.InvoiceId.Value) &&
                poi.ItemType == itemType &&
                poi.Status == status)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyDictionary<Guid, decimal>> GetAmountTotalsByInvoicesAndTypeAndStatusAsync(
        IReadOnlyCollection<Guid> invoiceIds,
        OrderItemType itemType,
        PaymentOrderItemStatus status,
        CancellationToken cancellationToken = default)
    {
        if (invoiceIds.Count == 0)
        {
            return new Dictionary<Guid, decimal>();
        }

        return await context.PaymentOrderItems
            .AsNoTracking()
            .Where(poi =>
                poi.InvoiceId.HasValue &&
                invoiceIds.Contains(poi.InvoiceId.Value) &&
                poi.ItemType == itemType &&
                poi.Status == status)
            .GroupBy(poi => poi.InvoiceId!.Value)
            .Select(group => new
            {
                InvoiceId = group.Key,
                Amount = group.Sum(poi => poi.Amount)
            })
            .ToDictionaryAsync(row => row.InvoiceId, row => row.Amount, cancellationToken);
    }

    public async Task<List<PaymentOrderItemEntity>> GetPaidPaymentOrderItemsForApplyAsync()
    {
        return await 
            Context.Payments.Where(x=>x.Direction== PaymentDirectionEnum.OUT).SelectMany(x=>x.PaymentOrder!.Items) 
            //Context.PaymentOrderItems
            .Include(x => x.BillingService).ThenInclude(x => x.Biller).ThenInclude(b => b.Partner)
            .Include(x => x.BillingService).ThenInclude(x => x.Translations)
            .Include(x => x.Currency)
            .Include(x => x.Invoice)
            .Where(poi => poi.Status == PaymentOrderItemStatus.Paid)
            .ToListAsync();
    }
    public async Task<List<PaymentOrderItemEntity>> GetPaidPaymentOrderItemsForBillingServiceCodeForApplyAsync(string billingServiceCode)
    {
        return await
            Context.Payments.Where(x => x.Direction == PaymentDirectionEnum.OUT).SelectMany(x => x.PaymentOrder!.Items)
            //Context.PaymentOrderItems
            .Include(x => x.BillingService).ThenInclude(x => x.Biller).ThenInclude(b => b.Partner)
            .Include(x => x.BillingService).ThenInclude(x => x.Translations)
            .Include(x => x.Currency)
            .Include(x => x.Invoice)
            .Where(poi => poi.Status == PaymentOrderItemStatus.Paid && poi.BillingService.Code == billingServiceCode)
            .ToListAsync();
    }
    public async Task<List<PaymentOrderItemEntity>> GetByPaymentIdForApplyAsync(Guid paymentId)
    {
        return await context
            .Payments
            .Where(x => x.Id == paymentId && x.Direction== PaymentDirectionEnum.OUT)
            .SelectMany(x => x.PaymentOrder!.Items)
            .Include(x=>x.BillingService).ThenInclude(x=>x.Biller).ThenInclude(b => b.Partner)
            .Include(x=>x.BillingService).ThenInclude(x=>x.Currencies)
            .Include(x=>x.Currency)
            .Include(x=>x.Invoice).ThenInclude(x=>x.Currency)
            .Include(x => x.Invoice).ThenInclude(x => x.BillingService).ThenInclude(x => x.Biller).ThenInclude(b => b.Partner)
            .Include(x => x.Invoice).ThenInclude(x => x.BillingService).ThenInclude(x => x.Currencies)
            .Include(x => x.Invoice).ThenInclude(x => x.BillingService).ThenInclude(x => x.Translations)
            .Include(x => x.Invoice).ThenInclude(x => x.PaymentOrderItems)
            .Include(x=>x.PaymentOrder).ThenInclude(x=>x.Customer)
            .ToListAsync();
    }

    public async Task<ICollection<PaymentOrderItemEntity>> GetByPaymentOrderIdAsync(Guid paymentOrderId)
    {
        return await context.PaymentOrderItems
            .Where(poi => poi.PaymentOrderId == paymentOrderId)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<PaymentOrderItemEntity>> GetPaymentRateListAsync(
        PaymentRateListInputModel model,
        CancellationToken cancellationToken = default)
    {
        var orderedQuery = ApplyOrdering(
            ApplyPaymentRateListFilter(model),
            model.SortBy,
            model.IsAscending,
            AllowedOrderBy,
            nameof(PaymentOrderItemEntity.CreatedAtUtc));

        return await orderedQuery
            .Skip((model.PageNumber - 1) * model.PageSize)
            .Take(model.PageSize)
            .ToListAsync(cancellationToken);
    }
    public async Task<int> CountPaymentRateListAsync(
        PaymentRateListInputModel model,
        CancellationToken cancellationToken = default)
    {
        return await ApplyPaymentRateListFilter(model).CountAsync(cancellationToken);
    }
    private IQueryable<PaymentOrderItemEntity> ApplyPaymentRateListFilter(PaymentRateListInputModel model)
    {
        var query = Context.PaymentOrderItems
            .Include(x => x.BillingService).ThenInclude(x => x.Biller)
            .Include(x => x.PaymentOrder).ThenInclude(x => x.Payments)
            .Where(x => (x.PaymentOrder!.Payments!.FirstOrDefault()!.Direction == PaymentDirectionEnum.OUT) &&
                        x.CurrencyId == model.CurrencyId &&
                        (model.BillerId == null ? true : x.BillingService.BillerId == model.BillerId) &&
                        (model.BillingServiceId == null ? true : x.BillingServiceId == model.BillingServiceId));

        return query;
    }
}
