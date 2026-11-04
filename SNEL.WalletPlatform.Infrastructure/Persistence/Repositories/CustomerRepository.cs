using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SNEL.WalletPlatform.Application.Customers.Interfaces;
using SNEL.WalletPlatform.Application.Customers.Models.Internal;
using SNEL.WalletPlatform.Domain.Customers;
using SNEL.WalletPlatform.Domain.Users;
using SNEL.WalletPlatform.Infrastructure.Persistence.Repositories.Base;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Repositories;

public sealed class CustomerRepository(ApplicationDbContext context)
    : EntityRepository<CustomerEntity, Guid>(context), ICustomerRepository
{
    private static readonly Dictionary<string, Expression<Func<CustomerEntity, object>>> AllowedOrderBy =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["createdAtUtc"] = x => x.CreatedAtUtc,
            ["updatedAtUtc"] = x => x.UpdatedAtUtc,
            ["partnerId"] = x => x.PartnerId,
            ["status"] = x => x.Status,
            ["id"] = x => x.Id
        };

    public ValueTask<CustomerEntity?> GetByIdAsync(Guid id)
    {
        return new ValueTask<CustomerEntity?>(
            context.Customers
                .FirstOrDefaultAsync(x => x.Id == id));
    }

    public async Task<IReadOnlyCollection<CustomerEntity>> GetPagedAsync(CustomerQueryInternalModel query)
    {
        var orderedQuery = ApplyOrdering(
            ApplyFilters(query.Search, query.Status),
            query.SortBy,
            query.IsAscending,
            AllowedOrderBy,
            nameof(CustomerEntity.CreatedAtUtc));

        return await orderedQuery
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .AsNoTracking()
            .ToListAsync();
    }

    public Task<int> CountAsync(CustomerQueryInternalModel query)
    {
        return ApplyFilters(query.Search, query.Status).CountAsync();
    }

    public Task<CustomerEntity?> GetByPartnerIdAsync(long partnerId, CancellationToken cancellationToken = default)
    {
        return context.Customers
            .FirstOrDefaultAsync(x => x.PartnerId == partnerId, cancellationToken);
    }

    public Task UpdateAsync(CustomerEntity customer)
    {
        context.Customers.Update(customer);
        return Task.CompletedTask;
    }

    public Task<CustomerEntity?> GetByIdWithDeleteRelationsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return context.Customers
            .Include(x => x.Partner)
            .Include(x => x.RegistrationHistories)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<bool> HasPaymentsAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return context.Payments.AnyAsync(x => x.CustomerId == customerId, cancellationToken);
    }

    public Task<bool> HasSubscriptionsAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return context.CustomerSubscriptions.AnyAsync(x => x.CustomerId == customerId, cancellationToken);
    }

    public Task<bool> HasFavoritesAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return context.CustomerFavorites.AnyAsync(x => x.CustomerId == customerId, cancellationToken);
    }

    public async Task<IReadOnlyList<CustomerEntity>> GetActiveBatchForNotificationsAsync(
        Guid? afterCustomerId,
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        var query = context.Customers
            .AsNoTracking()
            .Where(c => c.Status == CustomerStatus.Active);

        if (afterCustomerId.HasValue)
            query = query.Where(c => c.Id > afterCustomerId.Value);

        return await query
            .OrderBy(c => c.Id)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CustomerEntity>> GetActiveBatchWithMultipleSubscriptionsForNotificationsAsync(
        Guid? afterCustomerId,
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        // Hard-coded queryable audience: active customers with more than one subscription.
        var multiSubscriptionCustomerIds = context.CustomerSubscriptions
            .AsNoTracking()
            .GroupBy(s => s.CustomerId)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key);

        var query = context.Customers
            .AsNoTracking()
            .Where(c => c.Status == CustomerStatus.Active)
            .Where(c => multiSubscriptionCustomerIds.Contains(c.Id));

        if (afterCustomerId.HasValue)
            query = query.Where(c => c.Id > afterCustomerId.Value);

        return await query
            .OrderBy(c => c.Id)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    private IQueryable<CustomerEntity> ApplyFilters(string? search, CustomerStatus? status)
    {
        IQueryable<CustomerEntity> query = context.Customers
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var tokens = search
                .Trim()
                .ToLowerInvariant()
                .Split((char[]?)null!, StringSplitOptions.RemoveEmptyEntries)
                .Distinct()
                .ToArray();

            if (tokens.Length == 0)
            {
                tokens = [search.Trim().ToLowerInvariant()];
            }

            foreach (var token in tokens)
            {
                query = query.Where(c =>
                    (c.Address != null && c.Address.ToLower().Contains(token)) ||
                    (c.Area != null && (
                        c.Area.Code.ToLower().Contains(token) ||
                        c.Area.Name.ToLower().Contains(token) ||
                        c.Area.Translations.Any(tr => tr.Name.ToLower().Contains(token)) ||
                        c.Area.City.Code.ToLower().Contains(token) ||
                        c.Area.City.Name.ToLower().Contains(token) ||
                        c.Area.City.Translations.Any(tr => tr.Name.ToLower().Contains(token)) ||
                        c.Area.City.Region.Code.ToLower().Contains(token) ||
                        c.Area.City.Region.Name.ToLower().Contains(token) ||
                        c.Area.City.Region.Translations.Any(tr => tr.Name.ToLower().Contains(token)))) ||
                    context.Users.Any(u =>
                        u.PartnerId == c.PartnerId &&
                        u.Status != UserStatus.Inactive &&
                        ((u.MobileNo != null && u.MobileNo.ToLower().Contains(token)) ||
                            (u.Email != null && u.Email.ToLower().Contains(token)) ||
                            u.FirstName.ToLower().Contains(token) ||
                            (u.SecondName != null && u.SecondName.ToLower().Contains(token)) ||
                            (u.ThirdName != null && u.ThirdName.ToLower().Contains(token)) ||
                            u.LastName.ToLower().Contains(token) ||
                            (u.FirstName + " " + (u.SecondName ?? "") + " " + (u.ThirdName ?? "") + " " +
                                u.LastName)
                                .ToLower().Contains(token))));
            }
        }

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        return query;
    }

}
