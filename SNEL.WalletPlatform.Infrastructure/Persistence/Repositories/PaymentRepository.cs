using Microsoft.EntityFrameworkCore;
using SNEL.WalletPlatform.Application.Common.Models.Input;
using SNEL.WalletPlatform.Application.Payments.Interfaces;
using SNEL.WalletPlatform.Application.Payments.Models.Input;
using SNEL.WalletPlatform.Application.Payments.Models.Internal;
using SNEL.WalletPlatform.Domain.Payments;
using SNEL.WalletPlatform.Infrastructure.Persistence.Repositories.Base;
using SNEL.WalletPlatform.Infrastructure.Persistence.UnitOfWork;
using System.Linq.Expressions;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Repositories;

public sealed class PaymentRepository(ApplicationDbContext context)
    : EntityRepository<PaymentEntity,Guid>(context), IPaymentRepository
{
    private static readonly Dictionary<string, Expression<Func<PaymentEntity, object>>> AllowedOrderBy =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["createdAtUtc"] = x => x.CreatedAtUtc,
            ["updatedAtUtc"] = x => x.UpdatedAtUtc,
            ["id"] = x => x.Id
        };

    private static readonly Dictionary<string, Expression<Func<PaymentEntity, object>>> CompletedTopupReportRowsOrderBy =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["createdAtUtc"] = x => x.CreatedAtUtc,
            ["updatedAtUtc"] = x => x.UpdatedAtUtc,
            ["amount"] = x => x.Amount,
            ["paymentRef"] = x => x.PaymentRef,
            ["id"] = x => x.Id
        };

    public async Task<PaymentEntity> GetByPaymentRefWithTransactionsAsync(string paymentTransactionReference)
    {
        var payment = await Context.Payments
            .Include(p => p.PaymentTxns)
            .FirstOrDefaultAsync(p => p.PaymentRef == paymentTransactionReference);

        return payment;
    }

    public Task<PaymentEntity?> GetByIdWithApplyContextAsync(Guid paymentId)
    {
        return ApplyContextQuery()
            .FirstOrDefaultAsync(x => x.Id == paymentId);
    }

    public Task<PaymentEntity?> GetByIdWithHistoryContextAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default)
    {
        return ApplyHistoryIncludes(Context.Payments.AsNoTracking())
            .FirstOrDefaultAsync(x => x.Id == paymentId, cancellationToken);
    }

    public Task<PaymentEntity?> GetByIdWithPspContextAsync(Guid paymentId)
    {
        return Context.Payments
            .Include(x => x.PaymentOrder).ThenInclude(x => x!.Items).ThenInclude(x => x.BillingService).ThenInclude(x => x!.Biller).ThenInclude(b => b.Partner)
            .Include(x => x.PaymentOrder).ThenInclude(x => x!.Items).ThenInclude(x => x.BillingService).ThenInclude(x => x!.Translations)
            .Include(x => x.PaymentOrder).ThenInclude(x => x!.Items).ThenInclude(x => x.Invoice).ThenInclude(x => x!.BillingService).ThenInclude(x => x!.Biller).ThenInclude(b => b.Partner)
            .Include(x => x.PaymentOrder).ThenInclude(x => x!.Items).ThenInclude(x => x.Invoice).ThenInclude(x => x!.BillingService).ThenInclude(x => x!.Translations)
            .Include(x => x.PaymentMethod).ThenInclude(x => x!.PaymentProvider).ThenInclude(pp => pp.Partner)
            .Include(x => x.PaymentMethod).ThenInclude(x => x!.PaymentMethodCurrencies)
            .Include(x => x.Currency)
            .Include(x => x.PaymentOrder).ThenInclude(x => x!.Customer)
            .FirstOrDefaultAsync(x => x.Id == paymentId);
    }
    public Task<PaymentEntity?> GetByIdWithPrintContextAsync(Guid paymentId)
    {
        return Context.Payments
            .Include(x => x.Currency)
            .Include(x => x.Customer).ThenInclude(c => c!.Partner)
            .Include(x => x.PaymentMethod)
                .ThenInclude(x => x!.PaymentProvider).ThenInclude(pp => pp.Partner)
            .Include(x => x.PaymentOrder)
                .ThenInclude(x => x!.Customer).ThenInclude(c => c!.Partner)
            .Include(x => x.PaymentOrder)
                .ThenInclude(x => x!.Items.OrderBy(i => i.CreatedAtUtc))
                .ThenInclude(x => x.Invoice)
                    .ThenInclude(x => x!.Currency)
            .Include(x => x.PaymentOrder)
                .ThenInclude(x => x!.Items.OrderBy(i => i.CreatedAtUtc))
                .ThenInclude(x => x.Invoice)
                    .ThenInclude(x => x!.BillingService)
                        .ThenInclude(x => x!.Biller).ThenInclude(b => b.Partner)
            .Include(x => x.PaymentOrder)
                .ThenInclude(x => x!.Items.OrderBy(i => i.CreatedAtUtc))
                .ThenInclude(x => x.Invoice)
                    .ThenInclude(x => x!.BillingService)
                        .ThenInclude(x => x!.Translations)
            .Include(x => x.PaymentOrder)
                .ThenInclude(x => x!.Items.OrderBy(i => i.CreatedAtUtc))
                .ThenInclude(x => x.BillingService)
                    .ThenInclude(x => x!.Translations)
            .FirstOrDefaultAsync(x => x.Id == paymentId);
    }

    public async Task<IReadOnlyList<PaymentEntity>> GetByIdsWithApplyContextAsync(
        IReadOnlyCollection<Guid> paymentIds)
    {
        var normalizedPaymentIds = paymentIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToArray();

        if (normalizedPaymentIds.Length == 0)
            return Array.Empty<PaymentEntity>();

        return await ApplyContextQuery()
            .Where(x => normalizedPaymentIds.Contains(x.Id))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<PaymentEntity>> GetCompletedPaymentsWithApplyContextAsync()
    {
        return await ApplyContextQuery()
            .Where(payment =>
                payment.Status == PaymentStatus.Completed && payment.Direction== PaymentDirectionEnum.OUT &&
                payment.PaymentOrder != null &&
                payment.PaymentOrder.Items.Any(item =>
                    (item.ItemType == OrderItemType.PayBill ||
                     item.ItemType == OrderItemType.MeterCharge) &&
                    item.Status == PaymentOrderItemStatus.Paid &&
                    ((item.InvoiceId != null &&
                      item.Invoice != null &&
                      item.Invoice.BillingService != null &&
                      item.Invoice.BillingService.Biller != null &&
                      item.Invoice.BillingService.Biller.Code != null) ||
                     (item.BillingService != null &&
                      item.BillingService.Biller != null &&
                      item.BillingService.Biller.Code != null))))
            .ToListAsync();
    }

    public Task<PaymentEntity?> GetByPaymentRefWithApplyContextAsync(string paymentRef)
    {
        var normalizedPaymentRef = paymentRef.Trim().ToLower();

        return ApplyContextQuery()
            .FirstOrDefaultAsync(x => x.PaymentRef != null && x.PaymentRef!.ToLower() == normalizedPaymentRef);
    }

    public Task<PaymentEntity?> GetByExternalRefWithApplyContextAsync(string externalRef)
    {
        var normalizedExternalRef = externalRef.Trim().ToLower();

        return ApplyContextQuery()
            .FirstOrDefaultAsync(x => x.ExternalRef != null && x.ExternalRef!.ToLower() == normalizedExternalRef);
    }

    public async Task<IReadOnlyList<PaymentEntity>> GetPaymentHistoryByCustomerIdAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Payments
            .AsNoTracking()
            .Include(p => p.PaymentOrder!).ThenInclude(po => po!.Items).ThenInclude(i => i.BillingService).ThenInclude(bs => bs!.Translations)
            .Include(p => p.PaymentOrder!).ThenInclude(po => po!.Currency)
            .Include(p => p.Currency)
            .Include(p => p.PaymentMethod)
            .Include(p => p.PaymentMethod).ThenInclude(pm => pm!.PaymentProvider).ThenInclude(pp => pp.Partner)
            .Where(p => p.PaymentOrder != null
                && p.PaymentOrder.CustomerId == customerId
                && p.Direction == PaymentDirectionEnum.OUT)
            .OrderByDescending(p => p.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PaymentEntity>> GetTopUpPaymentHistoryByCustomerIdAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Payments
            .AsNoTracking()
            .Include(p => p.PaymentOrder!).ThenInclude(po => po!.Items).ThenInclude(i => i.BillingService).ThenInclude(bs => bs!.Translations)
            .Include(p => p.PaymentOrder!).ThenInclude(po => po!.Currency)
            .Include(p => p.Currency)
            .Include(p => p.PaymentMethod)
            .Include(p => p.PaymentMethod).ThenInclude(pm => pm!.PaymentProvider).ThenInclude(pp => pp.Partner)
            .Where(p => p.PaymentOrder != null
                && p.PaymentOrder.CustomerId == customerId
                && p.Direction == PaymentDirectionEnum.IN)
            .OrderByDescending(p => p.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PaymentEntity>> GetTopupRateListAsync(
        TopupRateListInputModel model,
        CancellationToken cancellationToken = default)
    {
        var orderedQuery = ApplyOrdering(
            ApplyTopupRateListFilter(model),
            model.SortBy,
            model.IsAscending,
            AllowedOrderBy,
            nameof(PaymentEntity.CreatedAtUtc));

        return await orderedQuery
            .Skip((model.PageNumber - 1) * model.PageSize)
            .Take(model.PageSize)
            .ToListAsync(cancellationToken);
    }
    public async Task<int> CountTopupRateListAsync(
        TopupRateListInputModel model,
        CancellationToken cancellationToken = default)
    {
        return await ApplyTopupRateListFilter(model).CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PaymentEntity>> GetCompletedTopupReportRowsPageAsync(
        IQueryable<PaymentEntity> query,
        string? sortBy,
        bool isAscending,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var orderedQuery = ApplyOrdering(
            query,
            sortBy,
            isAscending,
            CompletedTopupReportRowsOrderBy,
            nameof(PaymentEntity.CreatedAtUtc));

        return await orderedQuery
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountCompletedTopupReportRowsAsync(
        IQueryable<PaymentEntity> query,
        CancellationToken cancellationToken = default) =>
        query.CountAsync(cancellationToken);

    public async Task<IReadOnlyList<PaymentEntity>> GetPaymentHistoryByCustomerIdAsync(
        PaymentHistoryQueryInternalModel model,
        CancellationToken cancellationToken = default)
    {
        var orderedQuery = ApplyOrdering(
            ApplyPaymentHistoryFilters(model),
            model.SortBy,
            model.IsAscending,
            AllowedOrderBy,
            nameof(PaymentEntity.CreatedAtUtc));

        return await orderedQuery
            .Skip((model.PageNumber - 1) * model.PageSize)
            .Take(model.PageSize)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountPaymentHistoryByCustomerIdAsync(
        PaymentHistoryQueryInternalModel model,
        CancellationToken cancellationToken = default)
    {
        return ApplyPaymentHistoryFilters(model).CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PaymentEntity>> GetPaymentHistoryByPartnerIdAsync(
        PaymentHistoryQueryInternalModel model,
        long partnerId,
        CancellationToken cancellationToken = default)
    {
        var orderedQuery = ApplyOrdering(
            ApplyPaymentHistoryFilters(model)
                .Where(p => p.PaymentOrder != null
                            && p.PaymentOrder.Customer != null
                            && p.PaymentOrder.Customer.PartnerId == partnerId),
            model.SortBy,
            model.IsAscending,
            AllowedOrderBy,
            nameof(PaymentEntity.CreatedAtUtc));

        return await orderedQuery
            .Skip((model.PageNumber - 1) * model.PageSize)
            .Take(model.PageSize)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountPaymentHistoryByPartnerIdAsync(
        PaymentHistoryQueryInternalModel model,
        long partnerId,
        CancellationToken cancellationToken = default)
    {
        return ApplyPaymentHistoryFilters(model)
            .Where(p => p.PaymentOrder != null
                        && p.PaymentOrder.Customer != null
                        && p.PaymentOrder.Customer.PartnerId == partnerId)
            .CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PaymentEntity>> GetPaymentHistoryByProviderPartnerIdAsync(
        PaymentHistoryQueryInternalModel model,
        long partnerId,
        CancellationToken cancellationToken = default)
    {
        var orderedQuery = ApplyOrdering(
            ApplyPaymentHistoryFilters(model, filterByCustomer: false)
                .Where(p => p.PaymentMethod != null
                            && p.PaymentMethod.PaymentProvider != null
                            && p.PaymentMethod.PaymentProvider.PartnerId == partnerId),
            model.SortBy,
            model.IsAscending,
            AllowedOrderBy,
            nameof(PaymentEntity.CreatedAtUtc));

        return await orderedQuery
            .Skip((model.PageNumber - 1) * model.PageSize)
            .Take(model.PageSize)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountPaymentHistoryByProviderPartnerIdAsync(
        PaymentHistoryQueryInternalModel model,
        long partnerId,
        CancellationToken cancellationToken = default)
    {
        return ApplyPaymentHistoryFilters(model, filterByCustomer: false)
            .Where(p => p.PaymentMethod != null
                        && p.PaymentMethod.PaymentProvider != null
                        && p.PaymentMethod.PaymentProvider.PartnerId == partnerId)
            .CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PaymentEntity>> GetPaymentHistoryByScopeAsync(
        PaymentHistoryQueryInternalModel model,
        long? billerPartnerId,
        CancellationToken cancellationToken = default)
    {
        var query = ApplyPaymentHistoryFilters(model, filterByCustomer: false);
        if (billerPartnerId.HasValue)
        {
            query = query.Where(p => p.PaymentOrder != null
                                     && p.PaymentOrder.Items.Any(x => x.BillingService != null && x.BillingService.Biller.PartnerId == billerPartnerId));
        }

        var orderedQuery = ApplyOrdering(
            query,
            model.SortBy,
            model.IsAscending,
            AllowedOrderBy,
            nameof(PaymentEntity.CreatedAtUtc));

        return await orderedQuery
            .Skip((model.PageNumber - 1) * model.PageSize)
            .Take(model.PageSize)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountPaymentHistoryByScopeAsync(
        PaymentHistoryQueryInternalModel model,
        long? billerPartnerId,
        CancellationToken cancellationToken = default)
    {
        var query = ApplyPaymentHistoryFilters(model, filterByCustomer: false);
        if (billerPartnerId.HasValue)
        {
            query = query.Where(p => p.PaymentOrder != null
                                     && p.PaymentOrder.Items.Any(x => x.BillingService != null && x.BillingService.Biller.PartnerId == billerPartnerId));
        }

        return query.CountAsync(cancellationToken);
    }
    public async Task<IReadOnlyList<PaymentEntity>> GetTopUpPaymentHistoryByCustomerIdAsync(
        TopUpPaymentHistoryQueryInternalModel model,
        CancellationToken cancellationToken = default)
    {
        var orderedQuery = ApplyOrdering(
            ApplyTopUpPaymentHistoryFilters(model),
            model.SortBy,
            model.IsAscending,
            AllowedOrderBy,
            nameof(PaymentEntity.CreatedAtUtc));

        return await orderedQuery
            .Skip((model.PageNumber - 1) * model.PageSize)
            .Take(model.PageSize)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountTopUpPaymentHistoryByCustomerIdAsync(
        TopUpPaymentHistoryQueryInternalModel model,
        CancellationToken cancellationToken = default)
    {
        return ApplyTopUpPaymentHistoryFilters(model).CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PaymentEntity>> GetTopUpPaymentHistoryByPartnerIdAsync(
        TopUpPaymentHistoryQueryInternalModel model,
        long partnerId,
        CancellationToken cancellationToken = default)
    {
        var orderedQuery = ApplyOrdering(
            ApplyTopUpPaymentHistoryFilters(model)
                .Where(p => p.PaymentOrder != null
                            && p.PaymentOrder.Customer != null
                            && p.PaymentOrder.Customer.PartnerId == partnerId),
            model.SortBy,
            model.IsAscending,
            AllowedOrderBy,
            nameof(PaymentEntity.CreatedAtUtc));

        return await orderedQuery
            .Skip((model.PageNumber - 1) * model.PageSize)
            .Take(model.PageSize)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountTopUpPaymentHistoryByPartnerIdAsync(
        TopUpPaymentHistoryQueryInternalModel model,
        long partnerId,
        CancellationToken cancellationToken = default)
    {
        return ApplyTopUpPaymentHistoryFilters(model)
            .Where(p => p.PaymentOrder != null
                        && p.PaymentOrder.Customer != null
                        && p.PaymentOrder.Customer.PartnerId == partnerId)
            .CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PaymentEntity>> GetTopUpPaymentHistoryByProviderPartnerIdAsync(
        TopUpPaymentHistoryQueryInternalModel model,
        long partnerId,
        CancellationToken cancellationToken = default)
    {
        var orderedQuery = ApplyOrdering(
            ApplyTopUpPaymentHistoryFilters(model, filterByCustomer: false)
                .Where(p => p.PaymentMethod != null
                            && p.PaymentMethod.PaymentProvider != null
                            && p.PaymentMethod.PaymentProvider.PartnerId == partnerId),
            model.SortBy,
            model.IsAscending,
            AllowedOrderBy,
            nameof(PaymentEntity.CreatedAtUtc));

        return await orderedQuery
            .Skip((model.PageNumber - 1) * model.PageSize)
            .Take(model.PageSize)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountTopUpPaymentHistoryByProviderPartnerIdAsync(
        TopUpPaymentHistoryQueryInternalModel model,
        long partnerId,
        CancellationToken cancellationToken = default)
    {
        return ApplyTopUpPaymentHistoryFilters(model, filterByCustomer: false)
            .Where(p => p.PaymentMethod != null
                        && p.PaymentMethod.PaymentProvider != null
                        && p.PaymentMethod.PaymentProvider.PartnerId == partnerId)
            .CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PaymentEntity>> GetTopUpPaymentHistoryByScopeAsync(
        TopUpPaymentHistoryQueryInternalModel model,
        long? providerPartnerId,
        CancellationToken cancellationToken = default)
    {
        var query = ApplyTopUpPaymentHistoryFilters(model, filterByCustomer: false);
        if (providerPartnerId.HasValue)
        {
            query = query.Where(p => p.PaymentMethod != null
                                     && p.PaymentMethod.PaymentProvider != null
                                     && p.PaymentMethod.PaymentProvider.PartnerId == providerPartnerId.Value);
        }

        var orderedQuery = ApplyOrdering(
            query,
            model.SortBy,
            model.IsAscending,
            AllowedOrderBy,
            nameof(PaymentEntity.CreatedAtUtc));

        return await orderedQuery
            .Skip((model.PageNumber - 1) * model.PageSize)
            .Take(model.PageSize)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountTopUpPaymentHistoryByScopeAsync(
        TopUpPaymentHistoryQueryInternalModel model,
        long? providerPartnerId,
        CancellationToken cancellationToken = default)
    {
        var query = ApplyTopUpPaymentHistoryFilters(model, filterByCustomer: false);
        if (providerPartnerId.HasValue)
        {
            query = query.Where(p => p.PaymentMethod != null
                                     && p.PaymentMethod.PaymentProvider != null
                                     && p.PaymentMethod.PaymentProvider.PartnerId == providerPartnerId.Value);
        }

        return query.CountAsync(cancellationToken);
    }

    private IQueryable<PaymentEntity> ApplyTopUpPaymentHistoryFilters(
        TopUpPaymentHistoryQueryInternalModel model,
        bool filterByCustomer = true)
    {
        var normalizedSearch = string.IsNullOrWhiteSpace(model.Search) ? null : model.Search.Trim().ToLowerInvariant();

        var query = Context.Payments
            .AsNoTracking()
            .Include(p => p.PaymentOrder!).ThenInclude(po => po!.Items).ThenInclude(i => i.BillingService).ThenInclude(bs => bs!.Translations)
            .Include(p => p.PaymentOrder!).ThenInclude(po => po!.Currency)
            .Include(p => p.Currency).Include(p => p.PaymentMethod)
            .Include(x => x.PaymentMethod).ThenInclude(x => x!.PaymentProvider).ThenInclude(pp => pp.Partner)
            .Where(p => p.PaymentOrder != null
                && p.Direction == PaymentDirectionEnum.IN);

        if (filterByCustomer)
        {
            query = query.Where(p => p.PaymentOrder!.CustomerId == model.CustomerId);
        }
        if (model.StatusId != null)
        {
            query = query.Where(p => p.Status == (PaymentStatus)model.StatusId);
        }
        else if (model.OnlyActive)
        {
            query = query.Where(p =>
                p.Status == PaymentStatus.Processing ||
                p.Status == PaymentStatus.Completed);
        }
        if (model.PaymentProviderCode != null)
        {
            query = query.Where(p => p.PaymentMethod!.PaymentProvider!.Code == model.PaymentProviderCode);
        }
        if (model.PaymentMethodCode != null)
        {
            query = query.Where(p => p.PaymentMethod!.Code == model.PaymentMethodCode);
        }
        if (normalizedSearch is not null)
        {
            query = query.Where(p =>
                (p.PaymentRef != null && p.PaymentRef.ToLower().Contains(normalizedSearch))
                || (p.ExternalRef != null && p.ExternalRef.ToLower().Contains(normalizedSearch))
                || (p.PaymentMethod != null
                    && ((p.PaymentMethod.Code != null && p.PaymentMethod.Code.ToLower().Contains(normalizedSearch))
                        || (p.PaymentMethod.Name != null && p.PaymentMethod.Name.ToLower().Contains(normalizedSearch))))
                || (p.Currency != null
                    && ((p.Currency.Code != null && p.Currency.Code.ToLower().Contains(normalizedSearch))
                        || (p.Currency.Name != null && p.Currency.Name.ToLower().Contains(normalizedSearch))
                        || (p.Currency.Abbreviation != null && p.Currency.Abbreviation.ToLower().Contains(normalizedSearch))))
                || p.PaymentOrder!.Items.Any(i =>
                    (i.BillingRefValue != null && i.BillingRefValue.ToLower().Contains(normalizedSearch))
                    || (i.Invoice != null
                        && i.Invoice.BillingRef != null
                        && i.Invoice.BillingRef.ToLower().Contains(normalizedSearch))));
        }

        return query;
    }
    private IQueryable<PaymentEntity> ApplyTopupRateListFilter(TopupRateListInputModel model)
    {
        var query = Context.Payments
            .Include(x => x.Customer)
            .Include(x => x.Currency)
            .Include(x => x.PaymentMethod).ThenInclude(x => x.PaymentProvider)
            .Where(x => (x.Direction == PaymentDirectionEnum.IN) &&
                        x.CurrencyId == model.CurrencyId &&
                        (model.PaymentProviderId == null ? true : x.PaymentMethod!.PaymentProviderId == model.PaymentProviderId) &&
                        (model.PaymentMethodId == null ? true : x.PaymentMethodId == model.PaymentMethodId));
        
        return query;
    }
    
    private IQueryable<PaymentEntity> ApplyPaymentHistoryFilters(
        PaymentHistoryQueryInternalModel model,
        bool filterByCustomer = true)
    {
        var normalizedSearch = string.IsNullOrWhiteSpace(model.Search) ? null : model.Search.Trim().ToLowerInvariant();
        var normalizedBillerCode = string.IsNullOrWhiteSpace(model.BillerCode) ? null : model.BillerCode.Trim().ToLowerInvariant();
        var normalizedBillingServiceCode = string.IsNullOrWhiteSpace(model.BillingServiceCode) ? null : model.BillingServiceCode.Trim().ToLowerInvariant();
        var normalizedBillingRefValue = string.IsNullOrWhiteSpace(model.BillingRefValue) ? null : model.BillingRefValue.Trim().ToLowerInvariant();

        var query = ApplyHistoryIncludes(Context.Payments.AsNoTracking())
            .Where(p => p.PaymentOrder != null
                && p.Direction == PaymentDirectionEnum.OUT);

        if (filterByCustomer)
        {
            query = query.Where(p => p.PaymentOrder!.CustomerId == model.CustomerId);
        }
        if (model.StatusId != null)
        {
            query = query.Where(p => p.Status == (PaymentStatus)model.StatusId);
        }
        else if (model.OnlyActive)
        {
            query = query.Where(p =>
                p.Status == PaymentStatus.Processing ||
                p.Status == PaymentStatus.Completed);
        }
        if (normalizedSearch is not null)
        {
            query = query.Where(p =>
                (p.PaymentRef != null && p.PaymentRef.ToLower().Contains(normalizedSearch))
                || (p.ExternalRef != null && p.ExternalRef.ToLower().Contains(normalizedSearch))
                || (p.PaymentMethod != null
                    && ((p.PaymentMethod.Code != null && p.PaymentMethod.Code.ToLower().Contains(normalizedSearch))
                        || (p.PaymentMethod.Name != null && p.PaymentMethod.Name.ToLower().Contains(normalizedSearch))))
                || (p.Currency != null
                    && ((p.Currency.Code != null && p.Currency.Code.ToLower().Contains(normalizedSearch))
                        || (p.Currency.Name != null && p.Currency.Name.ToLower().Contains(normalizedSearch))
                        || (p.Currency.Abbreviation != null && p.Currency.Abbreviation.ToLower().Contains(normalizedSearch))))
                || p.PaymentOrder!.Items.Any(i =>
                    (i.BillingRefValue != null && i.BillingRefValue.ToLower().Contains(normalizedSearch))
                    || (i.Invoice != null
                        && i.Invoice.BillingRef != null
                        && i.Invoice.BillingRef.ToLower().Contains(normalizedSearch))));
        }
        if (normalizedBillerCode is not null)
        {
            query = query.Where(p => p.PaymentOrder!.Items.Any(i =>
                (i.BillingService != null
                 && i.BillingService.Biller != null
                 && i.BillingService.Biller.Code != null
                 && i.BillingService.Biller.Code.ToLower() == normalizedBillerCode)
                || (i.Invoice != null
                    && i.Invoice.BillingService != null
                    && i.Invoice.BillingService.Biller != null
                    && i.Invoice.BillingService.Biller.Code != null
                    && i.Invoice.BillingService.Biller.Code.ToLower() == normalizedBillerCode)));
        }

        if (normalizedBillingServiceCode is not null)
        {
            query = query.Where(p => p.PaymentOrder!.Items.Any(i =>
                (i.BillingService != null
                 && i.BillingService.Code != null
                 && i.BillingService.Code.ToLower() == normalizedBillingServiceCode)
                || (i.Invoice != null
                    && i.Invoice.BillingService != null
                    && i.Invoice.BillingService.Code != null
                    && i.Invoice.BillingService.Code.ToLower() == normalizedBillingServiceCode)));
        }

        if (normalizedBillingRefValue is not null)
        {
            query = query.Where(p => p.PaymentOrder!.Items.Any(i =>
                (i.BillingRefValue != null && i.BillingRefValue.ToLower().Contains(normalizedBillingRefValue))
                || (i.Invoice != null
                    && i.Invoice.BillingRef != null
                    && i.Invoice.BillingRef.ToLower().Contains(normalizedBillingRefValue))));
        }

        return query;
    }

    public async Task<IReadOnlyList<PaymentEntity>> GetPaymentHistoryByProviderPartnerTypeAsync(
        string partnerTypeCode,
        CancellationToken cancellationToken = default)
    {
        return await ProviderPartnerTypeHistoryQuery(partnerTypeCode, PaymentDirectionEnum.OUT)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PaymentEntity>> GetTopUpPaymentHistoryByProviderPartnerTypeAsync(
        string partnerTypeCode,
        CancellationToken cancellationToken = default)
    {
        return await ProviderPartnerTypeHistoryQuery(partnerTypeCode, PaymentDirectionEnum.IN)
            .ToListAsync(cancellationToken);
    }

    public async Task<PaymentAccessContextInternalModel?> GetPaymentAccessContextAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default)
    {
        var projection = await Context.Payments
            .AsNoTracking()
            .Where(p => p.Id == paymentId)
            .Select(p => new
            {
                CustomerId = p.PaymentOrder != null ? p.PaymentOrder.CustomerId : (Guid?)null,
                CustomerPartnerId = p.PaymentOrder != null && p.PaymentOrder.Customer != null
                    ? (long?)p.PaymentOrder.Customer.PartnerId
                    : null,
                ProviderPartnerId = p.PaymentMethod != null && p.PaymentMethod.PaymentProvider != null
                    ? (long?)p.PaymentMethod.PaymentProvider.PartnerId
                    : null,
                BillerPartnerIds = p.PaymentOrder == null
                    ? new List<long>()
                    : p.PaymentOrder.Items
                        .Where(i => i.BillingService != null && i.BillingService.Biller != null)
                        .Select(i => i.BillingService!.Biller.PartnerId)
                        .Distinct()
                        .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (projection is null)
            return null;

        return new PaymentAccessContextInternalModel(
            PaymentId: paymentId,
            CustomerId: projection.CustomerId,
            CustomerPartnerId: projection.CustomerPartnerId,
            ProviderPartnerId: projection.ProviderPartnerId,
            BillerPartnerIds: projection.BillerPartnerIds);
    }

    public Task<string?> GetProviderPartnerTypeCodeAsync(Guid paymentId, CancellationToken cancellationToken = default)
    {
        return Context.Payments
            .AsNoTracking()
            .Where(p => p.Id == paymentId)
            .Select(p => p.PaymentMethod != null
                         && p.PaymentMethod.PaymentProvider != null
                         && p.PaymentMethod.PaymentProvider.Partner != null
                         && p.PaymentMethod.PaymentProvider.Partner.PartnerType != null
                ? p.PaymentMethod.PaymentProvider.Partner.PartnerType.Code
                : null)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private IQueryable<PaymentEntity> ProviderPartnerTypeHistoryQuery(string partnerTypeCode, PaymentDirectionEnum direction)
    {
        var normalizedCode = partnerTypeCode.Trim().ToUpperInvariant();

        return Context.Payments
            .AsNoTracking()
            .Include(p => p.PaymentOrder!).ThenInclude(po => po!.Items).ThenInclude(i => i.BillingService).ThenInclude(bs => bs!.Translations)
            .Include(p => p.PaymentOrder!).ThenInclude(po => po!.Currency)
            .Include(p => p.Currency)
            .Include(p => p.PaymentMethod)
            .Include(p => p.PaymentMethod).ThenInclude(pm => pm!.PaymentProvider).ThenInclude(pp => pp.Partner).ThenInclude(pt => pt.PartnerType)
            .Where(p => p.Direction == direction
                        && p.PaymentMethod != null
                        && p.PaymentMethod.PaymentProvider != null
                        && p.PaymentMethod.PaymentProvider.Partner != null
                        && p.PaymentMethod.PaymentProvider.Partner.PartnerType != null
                        && p.PaymentMethod.PaymentProvider.Partner.PartnerType.Code == normalizedCode)
            .OrderByDescending(p => p.CreatedAtUtc);
    }

    private static IQueryable<PaymentEntity> ApplyHistoryIncludes(IQueryable<PaymentEntity> query)
    {
        return query
            .Include(p => p.PaymentOrder!).ThenInclude(po => po!.Items).ThenInclude(i => i.BillingService).ThenInclude(bs => bs!.Translations)
            .Include(p => p.PaymentOrder!).ThenInclude(po => po!.Items).ThenInclude(i => i.BillingService).ThenInclude(bs => bs!.Biller)
            .Include(p => p.PaymentOrder!).ThenInclude(po => po!.Items).ThenInclude(i => i.Invoice).ThenInclude(inv => inv!.BillingService).ThenInclude(bs => bs!.Biller)
            .Include(p => p.PaymentOrder!).ThenInclude(po => po!.Currency)
            .Include(p => p.Currency)
            .Include(p => p.PaymentMethod)
            .Include(x => x.PaymentMethod).ThenInclude(x => x!.PaymentProvider).ThenInclude(pp => pp.Partner);
    }

    private IQueryable<PaymentEntity> ApplyContextQuery()
    {
        return context.Payments
            .Include(x => x.PaymentOrder).ThenInclude(x => x.Payments.OrderBy(payment => payment.CreatedAtUtc))
            .Include(x => x.PaymentOrder).ThenInclude(x => x.Items.OrderBy(item => item.CreatedAtUtc)).ThenInclude(x => x.Invoice).ThenInclude(x => x.Currency)
            .Include(x => x.PaymentOrder).ThenInclude(x => x.Items.OrderBy(item => item.CreatedAtUtc)).ThenInclude(x => x.Invoice).ThenInclude(x => x.PaymentOrderItems.OrderBy(poi => poi.CreatedAtUtc)).ThenInclude(poi => poi.PaymentOrder).ThenInclude(po => po.Payments.OrderBy(payment => payment.CreatedAtUtc))
            .Include(x => x.PaymentOrder).ThenInclude(x => x.Items.OrderBy(item => item.CreatedAtUtc)).ThenInclude(x => x.Invoice).ThenInclude(x => x.BillingService).ThenInclude(x => x.Biller).ThenInclude(b => b.Partner)
            .Include(x => x.PaymentOrder).ThenInclude(x => x.Items.OrderBy(item => item.CreatedAtUtc)).ThenInclude(x => x.Invoice).ThenInclude(x => x.BillingService).ThenInclude(x => x.Translations)
            .Include(x => x.PaymentOrder).ThenInclude(x => x.Items.OrderBy(item => item.CreatedAtUtc)).ThenInclude(x => x.BillingService).ThenInclude(x => x.Biller).ThenInclude(b => b.Partner)
            .Include(x => x.PaymentOrder).ThenInclude(x => x.Items.OrderBy(item => item.CreatedAtUtc)).ThenInclude(x => x.BillingService).ThenInclude(x => x.Translations)
            .Include(x => x.PaymentOrder).ThenInclude(x => x.Items.OrderBy(item => item.CreatedAtUtc)).ThenInclude(x => x.BillingService).ThenInclude(x => x.Currencies)
            .Include(x => x.PaymentOrder).ThenInclude(x => x.Customer);
    }

    public Task<PaymentEntity?> GetByIdWithConfirmContextAsync(Guid paymentId)
    {
        return Context.Payments
            .Include(x=>x.PaymentOrder).ThenInclude(x=>x.Items).ThenInclude(x=>x.BillingService).ThenInclude(x=>x!.Biller).ThenInclude(b => b.Partner)
            .Include(x => x.PaymentOrder).ThenInclude(x => x.Items).ThenInclude(x => x.BillingService).ThenInclude(x => x!.Currencies)
            .Include(x => x.PaymentOrder).ThenInclude(x => x.Items).ThenInclude(x => x.Invoice).ThenInclude(x => x!.BillingService).ThenInclude(x => x!.Biller).ThenInclude(b => b.Partner)
            .Include(x => x.PaymentOrder).ThenInclude(x => x!.Customer)
            .Include(x => x.PaymentMethod).ThenInclude(x => x!.PaymentProvider).ThenInclude(pp => pp.Partner)
            .Include(x => x.PaymentMethod).ThenInclude(x => x!.PaymentMethodCurrencies)
            .Include(x => x.Currency)
            .FirstOrDefaultAsync(x => x.Id == paymentId);
    }

    public async Task<IReadOnlyList<Guid>> GetCanBeExpiredPaymentIdsCreatedBeforeUtcAsync(
        DateTimeOffset cutoffUtc,
        int take,
        CancellationToken cancellationToken = default)
    {
        if (take <= 0)
            return Array.Empty<Guid>();

        return await Context.Payments
            .AsNoTracking()
            .Where(p => PaymentStatusExtensions.ExpirableStatuses.Contains(p.Status) && p.CreatedAtUtc < cutoffUtc)
            .OrderBy(p => p.CreatedAtUtc)
            .Select(p => p.Id)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Guid>> GetInPaymentIdsByStatusesAsync(
        IReadOnlyCollection<PaymentStatus> statuses,
        DateTimeOffset createdBeforeUtc,
        int take,
        CancellationToken cancellationToken = default)
    {
        if (take <= 0 || statuses.Count == 0)
            return Array.Empty<Guid>();

        var normalizedStatuses = statuses.Distinct().ToArray();

        var query_base = Context.Payments.Include(p => p.PaymentMethod).ThenInclude(pm => pm!.PaymentMethodCurrencies)
            .Where(p =>
                p.Direction == PaymentDirectionEnum.IN
                && normalizedStatuses.Contains(p.Status)                
                );
        var query = query_base.Where(p => 
                (   p.PaymentMethod != null && 
                    p.PaymentMethod.PaymentMethodCurrencies.FirstOrDefault(pm => pm.CurrencyId == p.CurrencyId) != null && 
                    p.PaymentMethod.PaymentMethodCurrencies.FirstOrDefault(pm => pm.CurrencyId == p.CurrencyId).MinimumAgeMinutes != null && 
                    p.CreatedAtUtc < DateTimeOffset.UtcNow.AddMinutes(-(p.PaymentMethod.PaymentMethodCurrencies.FirstOrDefault(pm => pm.CurrencyId == p.CurrencyId).MinimumAgeMinutes ?? 0))
                ) ||
                (
                    (p.PaymentMethod == null ||
                    p.PaymentMethod.PaymentMethodCurrencies.FirstOrDefault(pm => pm.CurrencyId == p.CurrencyId) == null ||
                    p.PaymentMethod.PaymentMethodCurrencies.FirstOrDefault(pm => pm.CurrencyId == p.CurrencyId).MinimumAgeMinutes == null) &&
                    p.CreatedAtUtc < createdBeforeUtc
                )
                );

        //var query1 = query_base.Where(p =>
        //        (p.PaymentMethod != null &&
        //            p.PaymentMethod.PaymentMethodCurrencies.FirstOrDefault(pm => pm.CurrencyId == p.CurrencyId) != null &&
        //            p.PaymentMethod.PaymentMethodCurrencies.FirstOrDefault(pm => pm.CurrencyId == p.CurrencyId).MinimumAgeMinutes != null &&
        //            p.CreatedAtUtc < DateTimeOffset.UtcNow.AddMinutes(-(p.PaymentMethod.PaymentMethodCurrencies.FirstOrDefault(pm => pm.CurrencyId == p.CurrencyId).MinimumAgeMinutes ?? 0))
        //        )).ToList();
        //var query2 = query_base.Where(p =>
        //        (
        //            (p.PaymentMethod == null ||
        //            p.PaymentMethod.PaymentMethodCurrencies.FirstOrDefault(pm => pm.CurrencyId == p.CurrencyId) == null ||
        //            p.PaymentMethod.PaymentMethodCurrencies.FirstOrDefault(pm => pm.CurrencyId == p.CurrencyId).MinimumAgeMinutes == null) &&
        //            p.CreatedAtUtc < createdBeforeUtc
        //        )).ToList();
        return await query.AsNoTracking()
            .OrderBy(p => p.CreatedAtUtc)
            .Select(p => p.Id)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PaymentEntity>> GetPaymentsForReviewAsync(
        PaymentReviewQueryInputModel model,
        CancellationToken cancellationToken = default)
    {
        var orderedQuery = ApplyOrdering(
            ApplyPaymentsForReviewFilter(model),
            model.SortBy,
            model.IsAscending,
            AllowedOrderBy,
            nameof(PaymentEntity.CreatedAtUtc));

        return await orderedQuery
            .Skip((model.PageNumber - 1) * model.PageSize)
            .Take(model.PageSize)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountPaymentsForReviewAsync(
        PaymentReviewQueryInputModel model,
        CancellationToken cancellationToken = default)
    {
        return ApplyPaymentsForReviewFilter(model).CountAsync(cancellationToken);
    }

    private IQueryable<PaymentEntity> ApplyPaymentsForReviewFilter(PaymentReviewQueryInputModel model)
    {
        var normalizedSearch = string.IsNullOrWhiteSpace(model.Search) ? null : model.Search.Trim().ToLowerInvariant();

        var query = Context.Payments
            .Include(x => x.PaymentOrder)
            .ThenInclude(x => x.Items).ThenInclude(x => x.BillingService)
            .AsNoTracking()
            .Where(x => x.NeedsReview && x.Direction == PaymentDirectionEnum.OUT);
        if(model.BillerId.HasValue)
        {
            query = query.Where(x => x.PaymentOrder != null && x.PaymentOrder.Items.Any(i => i.BillingService != null && i.BillingService.BillerId == model.BillerId.Value));
        }   
        if (normalizedSearch is not null)
        {
            query = query.Where(x =>
                (x.PaymentRef != null && x.PaymentRef.ToLower().Contains(normalizedSearch)) ||
                (x.ExternalRef != null && x.ExternalRef.ToLower().Contains(normalizedSearch)) ||
                (x.PhoneNumber != null && x.PhoneNumber.ToLower().Contains(normalizedSearch)));
        }

        return query;
    }
}
