using AutoMapper;
using Microsoft.Extensions.Localization;
using SNEL.WalletPlatform.Application.BillingServices.Interfaces;
using SNEL.WalletPlatform.Application.BillingServices.Models.Internal;
using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Common.Localization;
using SNEL.WalletPlatform.Application.Customers.Mapper;
using SNEL.WalletPlatform.Application.Customers.Interfaces;
using SNEL.WalletPlatform.Application.Customers.Models.Input;
using SNEL.WalletPlatform.Application.Customers.Models.Internal;
using SNEL.WalletPlatform.Application.Customers.Models.Output;
using SNEL.WalletPlatform.Application.Interfaces.Auth;
using SNEL.WalletPlatform.Application.Interfaces.UnitOfWork;
using SNEL.WalletPlatform.Domain.Customers;
using SNEL.WalletPlatform.Application.Caching;
using SNEL.WalletPlatform.Domain.Lookups;

namespace SNEL.WalletPlatform.Application.Customers.Services;

public sealed class CustomerSubscriptionService(
    IUnitOfWork unitOfWork,
    IBillerConnectorResolver connectorResolver,
    IMapper mapper,
    IStringLocalizer<SharedResource> localizer,
    ICurrentUserContext currentUserContext) : ICustomerSubscriptionService
{
    public async Task<Result<PagedResult<CustomerSubscriptionOutputModel>>> GetCustomerSubscriptionsAsync(CustomerSubscriptionsQueryInputModel model)
    {
        var customer = await GetCurrentCustomerByPartnerAsync();
        if (customer is null)
        {
            return Result<PagedResult<CustomerSubscriptionOutputModel>>.Failure(
                localizer[LocalizationKeys.CustomerProfileNotFound],
                AppHttpStatusCodes.NotFound);
        }

        // Fetch favorites first to avoid per-subscription "is_favorite" checks.
        var favoriteSubscriptionIds = await unitOfWork.CustomerFavoritesRepository
            .GetSubscriptionFavoriteIdsAsync(customer.Id);
        var favoriteSubscriptionIdSet = favoriteSubscriptionIds.ToHashSet();

        var normalizedQuery = NormalizeQuery(model, customer.Id);
        var subscriptions = await unitOfWork.CustomerSubscriptionRepository.GetPagedAsync(normalizedQuery);

        var totalCount = await unitOfWork.CustomerSubscriptionRepository.CountAsync(normalizedQuery);
        var subscriptionList = subscriptions as IReadOnlyList<CustomerSubscriptionEntity> ?? subscriptions.ToList();
        var items = mapper.Map<CustomerSubscriptionOutputModel[]>(subscriptions);
        await CustomerSubscriptionOutputMapping.EnrichCustomerFromSubscriptionsAsync(
            items,
            subscriptionList,
            unitOfWork);

        foreach (var item in items)
        {
            item.IsFavorite = favoriteSubscriptionIdSet.Contains(item.Id);
        }

        return Result<PagedResult<CustomerSubscriptionOutputModel>>.Success(
            new PagedResult<CustomerSubscriptionOutputModel>(items, totalCount));
    }

    public async Task<Result<CustomerSubscriptionOutputModel>> GetCustomerSubscriptionByIdAsync(Guid id)
    {
        var customer = await GetCurrentCustomerByPartnerAsync();
        if (customer is null)
        {
            return Result<CustomerSubscriptionOutputModel>.Failure(
                localizer[LocalizationKeys.CustomerProfileNotFound],
                AppHttpStatusCodes.NotFound);
        }
        var subscription = await unitOfWork.CustomerSubscriptionRepository.GetByIdWithRelationsAsync(id);
        if (subscription is null || customer.Id != subscription.CustomerId)
        {
            return Result<CustomerSubscriptionOutputModel>.Failure(
                localizer[LocalizationKeys.CustomerSubscriptionNotFound, id],
                AppHttpStatusCodes.NotFound);
        }

        var favoriteSubscriptionIds = await unitOfWork.CustomerFavoritesRepository
            .GetSubscriptionFavoriteIdsAsync(customer.Id);

        var output = mapper.Map<CustomerSubscriptionOutputModel>(subscription);
        await CustomerSubscriptionOutputMapping.EnrichCustomerFromSubscriptionsAsync(
            [output],
            [subscription],
            unitOfWork);
        output.IsFavorite = favoriteSubscriptionIds.Contains(subscription.Id);

        return Result<CustomerSubscriptionOutputModel>.Success(output);
    }

    public async Task<Result<CustomerSubscriptionInfoOutputModel>> GetCustomerSubscriptionInfoAsync(CreateCustomerSubscriptionInputModel model)
    {
        var customer = await GetCurrentCustomerByPartnerAsync();
        if (customer is null)
        {
            return Result<CustomerSubscriptionInfoOutputModel>.Failure(
                localizer[LocalizationKeys.CustomerProfileNotFound],
                AppHttpStatusCodes.NotFound);
        }

        var billingRefResult = await TryBuildSubscriptionBillingRefAsync(model.AreaCode, model.SubscriptionNumber);
        if (!billingRefResult.IsSuccess)
        {
            return Result<CustomerSubscriptionInfoOutputModel>.Failure(
                billingRefResult.Message!,
                billingRefResult.StatusCode);
        }

        var billingRefValue = billingRefResult.Value!;

        var billerSubscriptionCheck = await CheckSubscriptionExistsViaBillerConnectorAsync(
            model.BillingServiceId,
            model.AreaCode ?? "",
            model.SubscriptionNumber.Trim());
        if (!billerSubscriptionCheck.IsSuccess)
        {
            return Result<CustomerSubscriptionInfoOutputModel>.Failure(
                billerSubscriptionCheck.Message!,
                billerSubscriptionCheck.StatusCode);
        }

        var isExist = await unitOfWork.CustomerSubscriptionRepository
            .ExistsAsync(customer.Id, model.BillingServiceId, billingRefValue);
        if (isExist)
        {
            var billingServiceForMessage = await unitOfWork.BillingServiceRepository.GetByIdAsync(model.BillingServiceId);
            var billingServiceCode = billingServiceForMessage?.Code ?? model.BillingServiceId.ToString();
            return Result<CustomerSubscriptionInfoOutputModel>.Failure(
                localizer[LocalizationKeys.CustomerSubscriptionAlreadyExists, billingServiceCode, billingRefValue],
                AppHttpStatusCodes.Conflict);
        }
        var billingService = await unitOfWork.BillingServiceRepository.GetByIdWithRelationsAsync(model.BillingServiceId);
        var message = localizer[LocalizationKeys.CustomerSubscriptionConfirmation, model.SubscriptionNumber, billerSubscriptionCheck.Value.Info!];
        return Result<CustomerSubscriptionInfoOutputModel>.Success(new CustomerSubscriptionInfoOutputModel(billerSubscriptionCheck.Value.Info!, model.SubscriptionNumber, message));
    }

    public async Task<Result<CustomerSubscriptionOutputModel>> CreateCustomerSubscriptionAsync(CreateCustomerSubscriptionInputModel model)
    {
        var customer = await GetCurrentCustomerByPartnerAsync();
        if (customer is null)
        {
            return Result<CustomerSubscriptionOutputModel>.Failure(
                localizer[LocalizationKeys.CustomerProfileNotFound],
                AppHttpStatusCodes.NotFound);
        }

        var billingRefResult = await TryBuildSubscriptionBillingRefAsync(model.AreaCode, model.SubscriptionNumber);
        if (!billingRefResult.IsSuccess)
        {
            return Result<CustomerSubscriptionOutputModel>.Failure(
                billingRefResult.Message!,
                billingRefResult.StatusCode);
        }

        var billingRefValue = billingRefResult.Value!;

        var billerSubscriptionCheck = await CheckSubscriptionExistsViaBillerConnectorAsync(
            model.BillingServiceId,
            model.AreaCode ?? "",
            model.SubscriptionNumber.Trim());
        if (!billerSubscriptionCheck.IsSuccess)
        {
            return Result<CustomerSubscriptionOutputModel>.Failure(
                billerSubscriptionCheck.Message!,
                billerSubscriptionCheck.StatusCode);
        }

        var isExist = await unitOfWork.CustomerSubscriptionRepository
            .ExistsAsync(customer.Id, model.BillingServiceId, billingRefValue);
        if (isExist)
        {
            var billingServiceForMessage = await unitOfWork.BillingServiceRepository.GetByIdAsync(model.BillingServiceId);
            var billingServiceCode = billingServiceForMessage?.Code ?? model.BillingServiceId.ToString();
            return Result<CustomerSubscriptionOutputModel>.Failure(
                localizer[LocalizationKeys.CustomerSubscriptionAlreadyExists, billingServiceCode, billingRefValue],
                AppHttpStatusCodes.Conflict);
        }

        var subscription = CustomerSubscriptionEntity.Create(
            customerId: customer.Id,
            billingServiceId: model.BillingServiceId,
            customerRef: billingRefValue,
            billingRefValue: billingRefValue,
            nickName: model.NickName);

        await unitOfWork.CustomerSubscriptionRepository.AddAsync(subscription);
        await unitOfWork.CompleteAsync();

        var createdSubscription = await unitOfWork.CustomerSubscriptionRepository.GetByIdAsync(subscription.Id);
        var createdOutput = mapper.Map<CustomerSubscriptionOutputModel>(createdSubscription);
        await CustomerSubscriptionOutputMapping.EnrichCustomerFromSubscriptionsAsync(
            [createdOutput],
            [createdSubscription!],
            unitOfWork);
        return Result<CustomerSubscriptionOutputModel>.Success(
            createdOutput,
            localizer[LocalizationKeys.CustomerSubscriptionCreateSuccess],
            AppHttpStatusCodes.Created);
    }

    public async Task<Result<CustomerSubscriptionOutputModel>> UpdateCustomerSubscriptionAsync(Guid id, UpdateCustomerSubscriptionInputModel model)
    {
        var customer = await GetCurrentCustomerByPartnerAsync();
        if (customer is null)
        {
            return Result<CustomerSubscriptionOutputModel>.Failure(
                localizer[LocalizationKeys.CustomerProfileNotFound],
                AppHttpStatusCodes.NotFound);
        }
        var subscription = await unitOfWork.CustomerSubscriptionRepository.GetByIdWithRelationsAsync(id);
        if (subscription is null || customer.Id != subscription.CustomerId)
        {
            return Result<CustomerSubscriptionOutputModel>.Failure(
                localizer[LocalizationKeys.CustomerSubscriptionNotFound, id],
                AppHttpStatusCodes.NotFound);
        }

        var billingRefResult = await TryBuildSubscriptionBillingRefAsync(model.AreaCode, model.SubscriptionNumber);
        if (!billingRefResult.IsSuccess)
        {
            return Result<CustomerSubscriptionOutputModel>.Failure(
                billingRefResult.Message!,
                billingRefResult.StatusCode);
        }

        var billingRefValue = billingRefResult.Value!;

        var billingRefChanged = !string.Equals(
            billingRefValue,
            subscription.BillingRefValue,
            StringComparison.Ordinal);

        if (billingRefChanged)
        {
            var billerSubscriptionCheck = await CheckSubscriptionExistsViaBillerConnectorAsync(
                model.BillingServiceId,
                model.AreaCode ?? "",
                model.SubscriptionNumber.Trim());
            if (!billerSubscriptionCheck.IsSuccess)
            {
                return Result<CustomerSubscriptionOutputModel>.Failure(
                    billerSubscriptionCheck.Message!,
                    billerSubscriptionCheck.StatusCode);
            }
        }

        var isExist = await unitOfWork.CustomerSubscriptionRepository
            .ExistsAsync(subscription.CustomerId, model.BillingServiceId, billingRefValue, [subscription.Id]);
        if (isExist)
        {
            var billingServiceForMessage = await unitOfWork.BillingServiceRepository.GetByIdAsync(model.BillingServiceId);
            var billingServiceCode = billingServiceForMessage?.Code ?? model.BillingServiceId.ToString();
            return Result<CustomerSubscriptionOutputModel>.Failure(
                localizer[LocalizationKeys.CustomerSubscriptionAlreadyExists, billingServiceCode, billingRefValue],
                AppHttpStatusCodes.Conflict);
        }

        subscription.Update(
            billingServiceId: model.BillingServiceId,
            customerRef: billingRefValue,
            billingRefValue: billingRefValue,
            nickName: model.NickName);

        unitOfWork.CustomerSubscriptionRepository.Update(subscription);
        await unitOfWork.CompleteAsync();

        var updatedSubscription = await unitOfWork.CustomerSubscriptionRepository.GetByIdAsync(id);
        var updatedOutput = mapper.Map<CustomerSubscriptionOutputModel>(updatedSubscription);
        await CustomerSubscriptionOutputMapping.EnrichCustomerFromSubscriptionsAsync(
            [updatedOutput],
            [updatedSubscription!],
            unitOfWork);
        return Result<CustomerSubscriptionOutputModel>.Success(
            updatedOutput,
            localizer[LocalizationKeys.CustomerSubscriptionUpdateSuccess],
            AppHttpStatusCodes.Ok);
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        var customer = await GetCurrentCustomerByPartnerAsync();
        if (customer is null)
        {
            return Result<bool>.Failure(
                localizer[LocalizationKeys.CustomerProfileNotFound],
                AppHttpStatusCodes.NotFound);
        }
        var subscription = await unitOfWork.CustomerSubscriptionRepository.GetByIdWithRelationsAsync(id);
        if (subscription is null || customer.Id != subscription.CustomerId)
        {
            return Result<bool>.Failure(
                localizer[LocalizationKeys.CustomerSubscriptionNotFound, id],
                AppHttpStatusCodes.NotFound);
        }

        //TODO: Check if subscription has dependencies and prevent delete (invoices, etc.)

        unitOfWork.CustomerSubscriptionRepository.Remove(subscription);
        await unitOfWork.CompleteAsync();

        return Result<bool>.Success(
            true,
            localizer[LocalizationKeys.CustomerSubscriptionDeleteSuccess],
            AppHttpStatusCodes.Ok);
    }

    private async Task<Result<string>> TryBuildSubscriptionBillingRefAsync(string? areaCodeRaw, string subscriptionNumberRaw)
    {
        var subscriptionNumber = subscriptionNumberRaw.Trim();
        if (string.IsNullOrEmpty(subscriptionNumber))
        {
            return Result<string>.Failure(
                localizer[LocalizationKeys.CustomerSubscriptionSubscriptionNumberRequired],
                AppHttpStatusCodes.BadRequest);
        }

        var areaCode = areaCodeRaw?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(areaCode))
        {
            return Result<string>.Success(subscriptionNumber);
        }
        var center = CachingFactory.GetValue<CenterEntity>(x => x.Code == areaCode).FirstOrDefault();
        if (center is null)
        {
            return Result<string>.Failure(
                localizer[LocalizationKeys.CustomerSubscriptionAreaCodeInvalid],
                AppHttpStatusCodes.BadRequest);
        }

        return Result<string>.Success($"{areaCode}-{subscriptionNumber}");
    }

    private async Task<Result<(bool Status, string? Info)>> CheckSubscriptionExistsViaBillerConnectorAsync(
        long billingServiceId,
        string areaCode,
        string subscriptionNumberTrimmed)
    {
        var billingService = await unitOfWork.BillingServiceRepository.GetByIdWithRelationsAsync(billingServiceId);
        if (billingService is null)
        {
            return Result<(bool Status, string? Info)>.Failure(
                localizer[LocalizationKeys.BillingInquiryServiceIdNotFound, billingServiceId],
                AppHttpStatusCodes.BadRequest);
        }

        var connector = connectorResolver.Resolve(billingService.Biller.Code);
        if (connector is null)
        {
            return Result<(bool Status, string? Info)>.Failure(
                localizer[LocalizationKeys.BillingInquiryConnectorNotConfigured, billingService.Biller.Code],
                AppHttpStatusCodes.BadRequest);
        }

        var request = new BillerSubscriptionExistsRequestModel(billingServiceId, areaCode, subscriptionNumberTrimmed);
        return await connector.CheckSubscriptionExistsAsync(request);
    }

    private CustomerSubscriptionsQueryInternalModel NormalizeQuery(CustomerSubscriptionsQueryInputModel model, Guid customerId)
    {
        var mapped = mapper.Map<CustomerSubscriptionsQueryInternalModel>(model);

        var pageNumber = mapped.PageNumber <= 0 ? 1 : mapped.PageNumber;
        var pageSize = mapped.PageSize <= 0 ? 20 : Math.Min(mapped.PageSize, 100);
        var search = string.IsNullOrWhiteSpace(mapped.Search) ? null : mapped.Search.Trim();
        var sortBy = string.IsNullOrWhiteSpace(mapped.SortBy) ? null : mapped.SortBy.Trim();

        return mapped with
        {
            CustomerId = customerId,
            BillingServiceId = model.BillingServiceId,
            PageNumber = pageNumber,
            PageSize = pageSize,
            Search = search,
            SortBy = sortBy
        };
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
