using SNEL.WalletPlatform.Domain.Customers;
using SNEL.WalletPlatform.Application.Customers.Models.Internal;

namespace SNEL.WalletPlatform.Application.Customers.Interfaces;

public interface ICustomerFavoritesRepository
{
    Task<bool> ExistsAsync(Guid customerId, int favoriteTypeId, Guid favoriteId);
    Task<bool> FavoriteTypeExistsAsync(int favoriteTypeId);
    Task<bool> SubscriptionExistsAsync(Guid subscriptionId);
    Task<bool> SubscriptionBelongsToCustomerAsync(Guid customerId, Guid subscriptionId);
    Task AddAsync(CustomerFavoritesEntity entity, CancellationToken cancellationToken = default);

    Task<int?> GetFavoriteTypeIdByCodeAsync(string code, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid customerId, int favoriteTypeId, Guid favoriteId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Guid>> GetSubscriptionFavoriteIdsAsync(
        Guid customerId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<CustomerFavoritesByTypeInternalModel>> GetFavoritesGroupedByTypeAsync(
        Guid customerId,
        string languageCode,
        CancellationToken cancellationToken = default);
}

