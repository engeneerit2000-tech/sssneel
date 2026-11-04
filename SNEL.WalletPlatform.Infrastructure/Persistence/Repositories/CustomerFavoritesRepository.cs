using Microsoft.EntityFrameworkCore;
using SNEL.WalletPlatform.Application.Customers.Interfaces;
using SNEL.WalletPlatform.Application.Customers.Models.Internal;
using SNEL.WalletPlatform.Domain.Customers;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Repositories;

public sealed class CustomerFavoritesRepository(ApplicationDbContext context) : ICustomerFavoritesRepository
{
    public Task<bool> ExistsAsync(Guid customerId, int favoriteTypeId, Guid favoriteId)
    {
        return context.CustomerFavorites.AnyAsync(x =>
            x.CustomerId == customerId &&
            x.FavoriteTypeId == favoriteTypeId &&
            x.FavoriteId == favoriteId);
    }

    public Task<bool> FavoriteTypeExistsAsync(int favoriteTypeId)
    {
        return context.FavoriteTypes.AnyAsync(x =>
            x.Id == favoriteTypeId &&
            x.Active);
    }

    public Task<bool> SubscriptionExistsAsync(Guid subscriptionId)
    {
        return context.CustomerSubscriptions.AnyAsync(x => x.Id == subscriptionId);
    }

    public Task<bool> SubscriptionBelongsToCustomerAsync(Guid customerId, Guid subscriptionId)
    {
        return context.CustomerSubscriptions.AnyAsync(x =>
            x.Id == subscriptionId &&
            x.CustomerId == customerId);
    }

    public async Task AddAsync(
        CustomerFavoritesEntity entity,
        CancellationToken cancellationToken = default)
    {
        await context.CustomerFavorites.AddAsync(entity, cancellationToken);
    }

    public Task<int?> GetFavoriteTypeIdByCodeAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Task.FromResult<int?>(null);
        }

        var normalized = code.Trim().ToUpperInvariant();
        return context.FavoriteTypes
            .AsNoTracking()
            .Where(x => x.Active && x.Code.ToUpper() == normalized)
            .Select(x => (int?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task DeleteAsync(
        Guid customerId,
        int favoriteTypeId,
        Guid favoriteId,
        CancellationToken cancellationToken = default)
    {
        var entity = await context.CustomerFavorites
            .FirstOrDefaultAsync(x =>
                x.CustomerId == customerId &&
                x.FavoriteTypeId == favoriteTypeId &&
                x.FavoriteId == favoriteId,
                cancellationToken);

        if (entity is null)
        {
            return;
        }

        context.CustomerFavorites.Remove(entity);
    }

    public async Task<IReadOnlyCollection<Guid>> GetSubscriptionFavoriteIdsAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        var normalizedCode = "SUBSCRIPTION";

        return await context.CustomerFavorites
            .AsNoTracking()
            .Where(f => f.CustomerId == customerId)
            .Join(
                context.FavoriteTypes.AsNoTracking().Where(t => t.Active),
                f => f.FavoriteTypeId,
                t => t.Id,
                (f, t) => new { f.FavoriteId, t.Code })
            .Where(x => x.Code.ToUpper() == normalizedCode)
            .Select(x => x.FavoriteId)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<CustomerFavoritesByTypeInternalModel>> GetFavoritesGroupedByTypeAsync(
        Guid customerId,
        string languageCode,
        CancellationToken cancellationToken = default)
    {
        var langCode = string.IsNullOrWhiteSpace(languageCode)
            ? "en"
            : languageCode.Trim().ToLowerInvariant();

        // Fetch flat rows then group in-memory to keep the EF query simpler.
        var rows = await (
            from fav in context.CustomerFavorites.AsNoTracking()
            where fav.CustomerId == customerId
            join type in context.FavoriteTypes.AsNoTracking()
                on fav.FavoriteTypeId equals type.Id
            join translation in context.FavoriteTypeTranslations.AsNoTracking()
                    .Where(t => t.LanguageCode == langCode)
                on type.Id equals translation.EntityId into translationsJoin
            from translation in translationsJoin.DefaultIfEmpty()
            select new
            {
                fav.FavoriteTypeId,
                FavoriteTypeName = translation != null ? translation.Name : type.Name,
                fav.FavoriteId
            })
            .ToListAsync(cancellationToken);

        return rows
            .GroupBy(x => x.FavoriteTypeId)
            .Select(g => new CustomerFavoritesByTypeInternalModel(
                FavoriteTypeId: g.Key,
                FavoriteTypeName: g.First().FavoriteTypeName,
                FavoriteIds: g.Select(x => x.FavoriteId).Distinct().ToList()))
            .ToList();
    }
}

