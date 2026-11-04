using AutoMapper;
using Microsoft.Extensions.Localization;
using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Common.Localization;
using SNEL.WalletPlatform.Application.Common.Security;
using SNEL.WalletPlatform.Application.Customers.Interfaces;
using SNEL.WalletPlatform.Application.Customers.Models.Input;
using SNEL.WalletPlatform.Application.Customers.Mapper;
using SNEL.WalletPlatform.Application.Customers.Models.Output;
using SNEL.WalletPlatform.Application.Interfaces.Auth;
using SNEL.WalletPlatform.Application.Interfaces.UnitOfWork;
using SNEL.WalletPlatform.Domain.Customers;

namespace SNEL.WalletPlatform.Application.Customers.Services;

public sealed class CustomerFavoritesService(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IStringLocalizer<SharedResource> localizer,
    ICurrentUserContext currentUserContext) : ICustomerFavoritesService
{
    public async Task<Result<CustomerFavoriteOutputModel>> CreateCustomerFavoriteAsync(
        CreateCustomerFavoriteInputModel request)
    {
        var currentCustomer = await GetCurrentCustomerByPartnerAsync();

        if (currentCustomer is null)
        {
            return Result<CustomerFavoriteOutputModel>.Failure(
                localizer[LocalizationKeys.CustomerProfileNotFound],
                AppHttpStatusCodes.NotFound);
        }

        var customerId = currentCustomer.Id;

        if (!await unitOfWork.CustomerFavoritesRepository.FavoriteTypeExistsAsync(request.FavoriteTypeId))
        {
            return Result<CustomerFavoriteOutputModel>.Failure(
                localizer[LocalizationKeys.CustomerFavoritesFavoriteTypeNotFound],
                AppHttpStatusCodes.NotFound);
        }

        if (!await unitOfWork.CustomerFavoritesRepository.SubscriptionExistsAsync(request.FavoriteId))
        {
            return Result<CustomerFavoriteOutputModel>.Failure(
                localizer[LocalizationKeys.CustomerFavoritesSubscriptionNotFound],
                AppHttpStatusCodes.NotFound);
        }

        if (!await unitOfWork.CustomerFavoritesRepository.SubscriptionBelongsToCustomerAsync(
                customerId,
                request.FavoriteId))
        {
            return Result<CustomerFavoriteOutputModel>.Failure(
                localizer[LocalizationKeys.CustomerFavoritesSubscriptionNotBelongToCustomer],
                AppHttpStatusCodes.NotFound);
        }

        var exists = await unitOfWork.CustomerFavoritesRepository
            .ExistsAsync(customerId, request.FavoriteTypeId, request.FavoriteId);

        if (exists)
        {
            return Result<CustomerFavoriteOutputModel>.Failure(
                localizer[LocalizationKeys.CustomerFavoritesAlreadyExists],
                AppHttpStatusCodes.Conflict);
        }

        var favorite = new CustomerFavoritesEntity
        {
            CustomerId = customerId,
            FavoriteTypeId = request.FavoriteTypeId,
            FavoriteId = request.FavoriteId,
        };

        await unitOfWork.CustomerFavoritesRepository.AddAsync(favorite);

        return Result<CustomerFavoriteOutputModel>.Success(
            new CustomerFavoriteOutputModel
            {
                CustomerId = favorite.CustomerId,
                FavoriteTypeId = favorite.FavoriteTypeId,
                FavoriteId = favorite.FavoriteId
            },
            localizer[LocalizationKeys.CustomerFavoritesCreateSuccess],
            AppHttpStatusCodes.Created);
    }

    public async Task<Result<CustomerFavoritesGroupedOutputModel>> GetCustomerFavoritesGroupedByTypeAsync()
    {
        var currentCustomer = await GetCurrentCustomerByPartnerAsync();

        if (currentCustomer is null)
        {
            return Result<CustomerFavoritesGroupedOutputModel>.Failure(
                localizer[LocalizationKeys.CustomerProfileNotFound],
                AppHttpStatusCodes.NotFound);
        }

        var internalGroups = await unitOfWork.CustomerFavoritesRepository
            .GetFavoritesGroupedByTypeAsync(
                currentCustomer.Id,
                currentUserContext.LanguageCode);

        var favoritesByType = new List<CustomerFavoritesByTypeOutputModel>();

        foreach (var group in internalGroups)
        {
            if (group.FavoriteIds.Count == 0)
            {
                favoritesByType.Add(new CustomerFavoritesByTypeOutputModel(
                    group.FavoriteTypeId,
                    group.FavoriteTypeName,
                    Array.Empty<CustomerSubscriptionOutputModel>()));
                continue;
            }

            var subscriptionEntities = await unitOfWork.CustomerSubscriptionRepository
                .GetByIdsWithRelationsAsync(group.FavoriteIds);

            var entityList = subscriptionEntities as IReadOnlyList<CustomerSubscriptionEntity> ?? subscriptionEntities.ToList();
            var subscriptionOutputs = mapper.Map<CustomerSubscriptionOutputModel[]>(subscriptionEntities);
            await CustomerSubscriptionOutputMapping.EnrichCustomerFromSubscriptionsAsync(
                subscriptionOutputs,
                entityList,
                unitOfWork);
            foreach (var sub in subscriptionOutputs)
            {
                sub.IsFavorite = true;
            }

            // Preserve order of favorite_ids when possible
            var idToSubscription = subscriptionOutputs.ToDictionary(s => s.Id, s => s);
            var ordered = group.FavoriteIds
                .Select(id => idToSubscription.GetValueOrDefault(id))
                .Where(s => s is not null)
                .Cast<CustomerSubscriptionOutputModel>()
                .ToList();

            favoritesByType.Add(new CustomerFavoritesByTypeOutputModel(
                group.FavoriteTypeId,
                group.FavoriteTypeName,
                ordered));
        }

        return Result<CustomerFavoritesGroupedOutputModel>.Success(
            new CustomerFavoritesGroupedOutputModel(favoritesByType),
            AppHttpStatusCodes.Ok);
    }

    public async Task<Result<CustomerFavoriteOutputModel>> SetSubscriptionFavoriteAsync(
        SetSubscriptionFavoriteInputModel request)
    {
        var currentCustomer = await GetCurrentCustomerByPartnerAsync();

        if (currentCustomer is null)
        {
            return Result<CustomerFavoriteOutputModel>.Failure(
                localizer[LocalizationKeys.CustomerProfileNotFound],
                AppHttpStatusCodes.NotFound);
        }

        var subscriptionFavoriteTypeId = await unitOfWork.CustomerFavoritesRepository
            .GetFavoriteTypeIdByCodeAsync("SUBSCRIPTION");

        if (subscriptionFavoriteTypeId is null)
        {
            return Result<CustomerFavoriteOutputModel>.Failure(
                localizer[LocalizationKeys.CustomerFavoritesFavoriteTypeNotFound],
                AppHttpStatusCodes.NotFound);
        }

        var customerId = currentCustomer.Id;

        if (request.IsFavorite)
        {
            var exists = await unitOfWork.CustomerFavoritesRepository
                .ExistsAsync(customerId, subscriptionFavoriteTypeId.Value, request.FavoriteId);

            if (exists)
            {
                return Result<CustomerFavoriteOutputModel>.Success(
                    new CustomerFavoriteOutputModel
                    {
                        CustomerId = customerId,
                        FavoriteTypeId = subscriptionFavoriteTypeId.Value,
                        FavoriteId = request.FavoriteId
                    },
                    localizer[LocalizationKeys.CustomerFavoritesFavoriteAlreadySetSuccess],
                    AppHttpStatusCodes.Ok);
            }

            if (!await unitOfWork.CustomerFavoritesRepository.SubscriptionExistsAsync(request.FavoriteId))
            {
                return Result<CustomerFavoriteOutputModel>.Failure(
                    localizer[LocalizationKeys.CustomerFavoritesSubscriptionNotFound],
                    AppHttpStatusCodes.NotFound);
            }

            if (!await unitOfWork.CustomerFavoritesRepository.SubscriptionBelongsToCustomerAsync(customerId, request.FavoriteId))
            {
                return Result<CustomerFavoriteOutputModel>.Failure(
                    localizer[LocalizationKeys.CustomerFavoritesSubscriptionNotBelongToCustomer],
                    AppHttpStatusCodes.NotFound);
            }

            var favorite = new CustomerFavoritesEntity
            {
                CustomerId = customerId,
                FavoriteTypeId = subscriptionFavoriteTypeId.Value,
                FavoriteId = request.FavoriteId,
            };
            await unitOfWork.CustomerFavoritesRepository.AddAsync(favorite);
            await unitOfWork.CompleteAsync();

            return Result<CustomerFavoriteOutputModel>.Success(
                new CustomerFavoriteOutputModel
                {
                    CustomerId = favorite.CustomerId,
                    FavoriteTypeId = favorite.FavoriteTypeId,
                    FavoriteId = favorite.FavoriteId
                },
                localizer[LocalizationKeys.CustomerFavoritesFavoriteSetSuccess],
                AppHttpStatusCodes.Created);
        }

        // is_favorite == false
        var favoriteExists = await unitOfWork.CustomerFavoritesRepository
            .ExistsAsync(customerId, subscriptionFavoriteTypeId.Value, request.FavoriteId);

        if (!favoriteExists)
        {
            return Result<CustomerFavoriteOutputModel>.Success(
                new CustomerFavoriteOutputModel
                {
                    CustomerId = customerId,
                    FavoriteTypeId = subscriptionFavoriteTypeId.Value,
                    FavoriteId = request.FavoriteId
                },
                localizer[LocalizationKeys.CustomerFavoritesFavoriteAlreadyUnsetSuccess],
                AppHttpStatusCodes.Ok);
        }

        await unitOfWork.CustomerFavoritesRepository.DeleteAsync(
            customerId,
            subscriptionFavoriteTypeId.Value,
            request.FavoriteId);

        await unitOfWork.CompleteAsync();

        return Result<CustomerFavoriteOutputModel>.Success(
            new CustomerFavoriteOutputModel
            {
                CustomerId = customerId,
                FavoriteTypeId = subscriptionFavoriteTypeId.Value,
                FavoriteId = request.FavoriteId
            },
            localizer[LocalizationKeys.CustomerFavoritesFavoriteUnsetSuccess],
            AppHttpStatusCodes.Ok);
    }

    private async Task<CustomerEntity?> GetCurrentCustomerByPartnerAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(currentUserContext.UserId))
            return null;

        var user = await unitOfWork.UserRepository.GetByIamRefWithPartnerAsync(currentUserContext.UserId, cancellationToken);
        if (user?.PartnerId is null)
            return null;

        return await unitOfWork.CustomerRepository.GetByPartnerIdAsync(user.PartnerId.Value, cancellationToken);
    }
}
