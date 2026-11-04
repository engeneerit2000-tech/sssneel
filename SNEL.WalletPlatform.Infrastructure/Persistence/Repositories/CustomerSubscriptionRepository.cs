using Microsoft.EntityFrameworkCore;
using SNEL.WalletPlatform.Application.Customers.Interfaces;
using SNEL.WalletPlatform.Application.Customers.Models.Internal;
using SNEL.WalletPlatform.Domain.Customers;
using SNEL.WalletPlatform.Infrastructure.Persistence.Repositories.Base;
using System.Linq.Expressions;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Repositories;

public sealed class CustomerSubscriptionRepository(ApplicationDbContext context)
    : EntityRepository<CustomerSubscriptionEntity,Guid>(context), ICustomerSubscriptionRepository
{
    private static readonly Dictionary<string, Expression<Func<CustomerSubscriptionEntity, object>>> AllowedOrderBy =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["createdAtUtc"] = x => x.CreatedAtUtc,
            ["updatedAtUtc"] = x => x.UpdatedAtUtc,
            ["nickName"] = x => x.NickName,
            ["billingRefValue"] = x => x.BillingRefValue,
            ["id"] = x => x.Id
        };

    public Task<CustomerSubscriptionEntity?> GetByIdWithRelationsAsync(Guid id)
    {
        return context.CustomerSubscriptions
            .Include(x => x.Customer)
            .Include(x => x.BillingService).ThenInclude(x => x.Biller).ThenInclude(b => b.Partner)
            .Include(x => x.BillingService).ThenInclude(x => x.BillingCategory)
            .Include(x => x.BillingService).ThenInclude(x => x.Translations)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public Task<CustomerSubscriptionEntity?> GetByIdForInquiryAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return context.CustomerSubscriptions
            .AsNoTracking()
            .Include(x => x.BillingService)
            .ThenInclude(x => x.Biller)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<CustomerSubscriptionEntity>> GetByIdsWithRelationsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
        {
            return Array.Empty<CustomerSubscriptionEntity>();
        }

        return await context.CustomerSubscriptions
            .Include(x => x.Customer)
            .Include(x => x.BillingService).ThenInclude(x => x.Biller).ThenInclude(b => b.Partner)
            .Include(x => x.BillingService).ThenInclude(x => x.BillingCategory)
            .Include(x => x.BillingService).ThenInclude(x => x.Translations)
            .Where(x => ids.Contains(x.Id))
            .ToListAsync(cancellationToken);
    }

    public Task<List<CustomerSubscriptionEntity>> GetByCustomerIdAsync(Guid customerId)
    {
        return context.CustomerSubscriptions
            .Where(x => x.CustomerId == customerId)
            .ToListAsync();
    }

    public Task<bool> ExistsAsync(Guid customerId, long billingServiceId, string billingRefValue, Guid[]? excludedIds = null)
    {
        var normalizedBillingRefValue = billingRefValue.Trim().ToLower();
        var query = context.CustomerSubscriptions
          .Where(x => x.CustomerId == customerId 
            && x.BillingRefValue.ToLower() == normalizedBillingRefValue
            && x.BillingServiceId == billingServiceId);

        if (excludedIds?.Any() == true)
        {
            query = query.Where(x => !excludedIds.Contains(x.Id));
        }
        return query.AnyAsync();
    }

    public async Task<IReadOnlyCollection<CustomerSubscriptionEntity>> GetPagedAsync(CustomerSubscriptionsQueryInternalModel model)
    {
        var orderedQuery = ApplyOrdering(
            ApplyFilters(model),
            model.SortBy,
            model.IsAscending,
            AllowedOrderBy,
            nameof(CustomerEntity.CreatedAtUtc));

        return await orderedQuery
            .Skip((model.PageNumber - 1) * model.PageSize)
            .Take(model.PageSize)
            .AsNoTracking()
            .ToListAsync();
    }

    public Task<int> CountAsync(CustomerSubscriptionsQueryInternalModel model)
    {
        return ApplyFilters(model).CountAsync();
    }

    private IQueryable<CustomerSubscriptionEntity> ApplyFilters(CustomerSubscriptionsQueryInternalModel model)
    {
        IQueryable<CustomerSubscriptionEntity> query = context.CustomerSubscriptions
            .Include(x => x.BillingService.BillingCategory)
            .Include(x => x.BillingService).ThenInclude(x => x.Biller).ThenInclude(b => b.Partner)
            .Include(x => x.BillingService.ServiceType)
            .Include(x => x.BillingService.Translations);

        if (!string.IsNullOrWhiteSpace(model.Search))
        {
            //
        }

        if(model.CustomerId.HasValue)
        {
            query = query.Where(x => x.CustomerId == model.CustomerId.Value);
        }

        if (model.BillingServiceId.HasValue)
        {
            query = query.Where(x => x.BillingServiceId == model.BillingServiceId.Value);
        }

        return query;
    }
}
