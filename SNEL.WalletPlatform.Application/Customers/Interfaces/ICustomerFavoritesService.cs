using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Customers.Models.Input;
using SNEL.WalletPlatform.Application.Customers.Models.Output;

namespace SNEL.WalletPlatform.Application.Customers.Interfaces;

public interface ICustomerFavoritesService
{
    Task<Result<CustomerFavoriteOutputModel>> CreateCustomerFavoriteAsync(
        CreateCustomerFavoriteInputModel request);

    Task<Result<CustomerFavoritesGroupedOutputModel>> GetCustomerFavoritesGroupedByTypeAsync();

    Task<Result<CustomerFavoriteOutputModel>> SetSubscriptionFavoriteAsync(
        SetSubscriptionFavoriteInputModel request);
}

