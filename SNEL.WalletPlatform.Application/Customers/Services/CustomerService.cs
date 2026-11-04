using AutoMapper;
using Microsoft.Extensions.Localization;
using SNEL.WalletPlatform.Application.Caching;
using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Common.Localization;
using SNEL.WalletPlatform.Application.Common.Security;
using SNEL.WalletPlatform.Application.Customers.Interfaces;
using SNEL.WalletPlatform.Application.Customers.Models.Input;
using SNEL.WalletPlatform.Application.Customers.Models.Internal;
using SNEL.WalletPlatform.Application.Customers.Models.Output;
using SNEL.WalletPlatform.Application.Interfaces.Auth;
using SNEL.WalletPlatform.Application.Interfaces.IAM;
using SNEL.WalletPlatform.Application.Interfaces.UnitOfWork;
using SNEL.WalletPlatform.Domain.Accounting;
using SNEL.WalletPlatform.Domain.Accounting.Enums;
using SNEL.WalletPlatform.Domain.Customers;
using SNEL.WalletPlatform.Domain.Lookups;
using SNEL.WalletPlatform.Domain.Partners;
using SNEL.WalletPlatform.Domain.Users;

namespace SNEL.WalletPlatform.Application.Customers.Services;

public sealed class CustomerService(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IStringLocalizer<SharedResource> localizer,
    IIdentityUserManager identityUserManager,
    ICurrentUserContext currentUserContext) : ICustomerService
{
    public async Task<Result<PagedResult<CustomerOutputModel>>> GetCustomersAsync(CustomersQueryInputModel model)
    {
        var normalizedQuery = NormalizeQuery(model);
        var customers = await unitOfWork.CustomerRepository.GetPagedAsync(normalizedQuery);

        var totalCount = await unitOfWork.CustomerRepository.CountAsync(normalizedQuery);
        var users = await unitOfWork.UserRepository.GetByPartnerIdsWithNotificationsDependenciesAsync(
            customers.Select(c => c.PartnerId).Distinct().ToList());
        var usersByPartnerId = users
            .Where(u => u.PartnerId.HasValue)
            .GroupBy(u => u.PartnerId!.Value)
            .ToDictionary(g => g.Key, g => g.First());
        var items = customers
            .Select(customer => usersByPartnerId.TryGetValue(customer.PartnerId, out var user)
                ? MapCustomerOutput(customer, user)
                : null)
            .Where(x => x is not null)
            .Cast<CustomerOutputModel>()
            .ToArray();

        return Result<PagedResult<CustomerOutputModel>>.Success(new PagedResult<CustomerOutputModel>(items, totalCount));
    }

    public async Task<Result<CustomerOutputModel>> GetCustomerByIdAsync(Guid id)
    {
        var customer = await unitOfWork.CustomerRepository.GetByIdAsync(id);
        if (customer is null)
            return Result<CustomerOutputModel>.Failure(localizer[LocalizationKeys.CustomerNotFound, id], AppHttpStatusCodes.NotFound);

        var user = await GetUserByPartnerIdAsync(customer.PartnerId);
        return user is null
            ? Result<CustomerOutputModel>.Failure(localizer[LocalizationKeys.CustomerNotFound, id], AppHttpStatusCodes.NotFound)
            : Result<CustomerOutputModel>.Success(MapCustomerOutput(customer, user));
    }

    public async Task<Result<CustomerOutputModel>> GetMyProfileAsync()
    {
        if (string.IsNullOrWhiteSpace(currentUserContext.UserId))
        {
            return Result<CustomerOutputModel>.Failure(localizer[LocalizationKeys.CustomerProfileNotFound], AppHttpStatusCodes.Unauthorized);
        }

        var customer = await GetCurrentCustomerByPartnerAsync();
        if (customer is null)
            return Result<CustomerOutputModel>.Failure(localizer[LocalizationKeys.CustomerProfileNotFound], AppHttpStatusCodes.Unauthorized);

        var user = await GetUserByPartnerIdAsync(customer.PartnerId);
        return user is null
            ? Result<CustomerOutputModel>.Failure(localizer[LocalizationKeys.CustomerProfileNotFound], AppHttpStatusCodes.Unauthorized)
            : Result<CustomerOutputModel>.Success(MapCustomerOutput(customer, user));
    }

    public async Task<Result<CustomerOutputModel>> UpdateMyProfileAsync(UpdateMyCustomerProfileInputModel model)
    {
        if (string.IsNullOrWhiteSpace(currentUserContext.UserId))
        {
            return Result<CustomerOutputModel>.Failure(localizer[LocalizationKeys.CustomerProfileNotFound], AppHttpStatusCodes.Unauthorized);
        }

        var customer = await GetCurrentCustomerByPartnerAsync();
        if (customer is null)
        {
            return Result<CustomerOutputModel>.Failure(localizer[LocalizationKeys.CustomerProfileNotFound], AppHttpStatusCodes.Unauthorized);
        }

        var areaValidation = await ValidateAreaIdAsync(model.AreaId);
        if (areaValidation is not null)
        {
            return areaValidation;
        }

        var user = await GetUserByPartnerIdAsync(customer.PartnerId);
        if (user is null)
        {
            return Result<CustomerOutputModel>.Failure(localizer[LocalizationKeys.CustomerProfileNotFound], AppHttpStatusCodes.Unauthorized);
        }

        var genderValidation = await ValidateGenderIdForMyProfileAsync(model.GenderId, user.GenderId);
        if (genderValidation is not null)
        {
            return genderValidation;
        }

        var partner = await unitOfWork.PartnerRepository.GetByIdAsync(customer.PartnerId);
        if (partner is null)
        {
            return Result<CustomerOutputModel>.Failure(
                localizer[LocalizationKeys.CustomerProfileNotFound],
                AppHttpStatusCodes.Unauthorized);
        }

        var normalizedEmail = model.Email is null
            ? null
            : (string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim());

        if (normalizedEmail is not null &&
            !string.Equals(user.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase) &&
            await unitOfWork.UserRepository.ExistsByEmailAsync(normalizedEmail))
        {
            return Result<CustomerOutputModel>.Failure(
                localizer[LocalizationKeys.UserEmailAlreadyExists, normalizedEmail],
                AppHttpStatusCodes.Conflict);
        }

        if (normalizedEmail is not null &&
            !string.Equals(user.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase) &&
            await identityUserManager.FindByEmailAsync(normalizedEmail) is { } existingByEmail)
        {
            if (string.IsNullOrWhiteSpace(user.IamRef) ||
                !string.Equals(existingByEmail.Id, user.IamRef, StringComparison.OrdinalIgnoreCase))
            {
                return Result<CustomerOutputModel>.Failure(
                    localizer[LocalizationKeys.IdentityEmailAlreadyExists, normalizedEmail],
                    AppHttpStatusCodes.Conflict);
            }
        }

        var emailForKeycloak = model.Email is null
            ? (string.IsNullOrWhiteSpace(user.Email) ? null : user.Email.Trim())
            : normalizedEmail;

        if (!string.IsNullOrWhiteSpace(user.IamRef))
        {
            try
            {
                await identityUserManager.UpdateUserAsync(new UpdateIdentityUserRequest(
                    Id: user.IamRef,
                    Username: user.Username,
                    Email: emailForKeycloak,
                    FirstName: model.FirstName.Trim(),
                    LastName: model.LastName.Trim(),                    
                    Enabled: user.Status == UserStatus.Active));
            }
            catch (InvalidOperationException ex)
            {
                return Result<CustomerOutputModel>.Failure(ex.Message, AppHttpStatusCodes.BadRequest);
            }
        }

        var effectiveGenderId = model.GenderId ?? user.GenderId;

        ApplyMyCustomerProfileUpdates(customer, user, model, effectiveGenderId);

        var partnerDisplayAndLegal = $"{model.FirstName.Trim()} {model.LastName.Trim()}";
        partner.UpdateProfile(partnerDisplayAndLegal, partnerDisplayAndLegal, partner.Active);

        await unitOfWork.CustomerRepository.UpdateAsync(customer);
        unitOfWork.UserRepository.Update(user);
        unitOfWork.PartnerRepository.Update(partner);
        await unitOfWork.CompleteAsync();

        return Result<CustomerOutputModel>.Success(
            MapCustomerOutput(customer, user),
            localizer[LocalizationKeys.CustomerProfileUpdatedSuccessfully],
            AppHttpStatusCodes.Ok);
    }

    public async Task<Result<CustomerOutputModel>> CreateCustomerAsync(CreateCustomerInputModel model)
    {
        var areaValidation = await ValidateAreaIdAsync(model.AreaId);
        if (areaValidation is not null)
        {
            return areaValidation;
        }

        var normalizedPhone = model.MobileNo.Trim();
        var normalizedEmail = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim();
        var normalizedNationalNo = string.IsNullOrWhiteSpace(model.NationalNo) ? null : model.NationalNo.Trim();

        if (await unitOfWork.UserRepository.ExistsByMobileNoAsync(normalizedPhone))
        {
            return Result<CustomerOutputModel>.Failure(
                localizer[LocalizationKeys.UserPhoneNumberAlreadyExists, normalizedPhone],
                AppHttpStatusCodes.Conflict);
        }

        if (!string.IsNullOrWhiteSpace(normalizedEmail) &&
            await unitOfWork.UserRepository.ExistsByEmailAsync(normalizedEmail))
        {
            return Result<CustomerOutputModel>.Failure(
                localizer[LocalizationKeys.UserEmailAlreadyExists, normalizedEmail],
                AppHttpStatusCodes.Conflict);
        }

        if (!string.IsNullOrWhiteSpace(normalizedNationalNo) &&
            await unitOfWork.UserRepository.ExistsByNationalNoAsync(normalizedNationalNo))
        {
            return Result<CustomerOutputModel>.Failure(
                localizer[LocalizationKeys.UserNationalNoAlreadyExists, normalizedNationalNo],
                AppHttpStatusCodes.Conflict);
        }
        var area = CachingFactory.GetValue<AreaEntity>(x => x.Id == model.AreaId!.Value).FirstOrDefault();
        if (model.AreaId.HasValue &&
            area is null)
        {
            return Result<CustomerOutputModel>.Failure(
                localizer[LocalizationKeys.AreaInvalid, model.AreaId.Value],
                AppHttpStatusCodes.BadRequest);
        }

        var username = normalizedPhone;

        if (await identityUserManager.FindByUsernameAsync(username) is not null)
        {
            return Result<CustomerOutputModel>.Failure(
                localizer[LocalizationKeys.IdentityUsernameAlreadyExists, username],
                AppHttpStatusCodes.Conflict);
        }

        if (!string.IsNullOrWhiteSpace(normalizedEmail) &&
            await identityUserManager.FindByEmailAsync(normalizedEmail) is not null)
        {
            return Result<CustomerOutputModel>.Failure(
                localizer[LocalizationKeys.IdentityEmailAlreadyExists, normalizedEmail],
                AppHttpStatusCodes.Conflict);
        }

        IdentityUserModel? identityUser = null;

        await using var tx = await unitOfWork.BeginTransactionAsync();
        try
        {
            identityUser = await identityUserManager.CreateUserAsync(new CreateIdentityUserRequest(
                Username: username,
                Password: model.Password,
                Email: normalizedEmail,
                FirstName: model.FirstName.Trim(),
                LastName: model.LastName.Trim(),
                Enabled: true));

            await identityUserManager.AssignRealmRolesAsync(identityUser.Id, [IdentityRoleConstants.CustomerAdmin]);
         

            var user = UserEntity.Create(
                username: username,
                firstName: model.FirstName,
                secondName: model.SecondName,
                thirdName: model.ThirdName,
                lastName: model.LastName,
                genderId: model.GenderId ?? 1,
                mobileNo: normalizedPhone,
                email: normalizedEmail ?? string.Empty,
                nationalNo: normalizedNationalNo ?? string.Empty,
                iamRef: identityUser.Id,
                language: model.Language,
                notificationChannels: ResolveNotificationChannels(model.NotificationChannels));

            var partner = PartnerEntity.Create(
                PartnerTypeConstants.Customer,
                $"{user.FirstName} {user.LastName}", 
                $"{user.FirstName} {user.LastName}",
                true);

            user.LinkPartner(partner);
            await unitOfWork.UserRepository.AddAsync(user);
            await unitOfWork.PartnerRepository.AddAsync(partner);

            var customer = CustomerEntity.Create(
                partner,
                areaId: model.AreaId,
                address: model.Address);

            var currencies = CachingFactory.GetValue<CurrencyEntity>();
            foreach (var item in currencies)
            {
                var account = AccountEntity.Create(
                    (int)AccountTypesEnum.CUSTOMER_LIABILITY,
                    partner,
                    item.Id,
                    order: item.Order,
                    balance: 0);
                await unitOfWork.AccountRepository.AddAsync(account);
            }
            await unitOfWork.CustomerRepository.AddAsync(customer);
            await unitOfWork.CompleteAsync();
            await unitOfWork.CommitTransactionAsync();// tx.CommitAsync();

            return Result<CustomerOutputModel>.Success(
                MapCustomerOutput(customer, user),
                localizer[LocalizationKeys.CustomerCreateSuccess],
                AppHttpStatusCodes.Created);
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync();// tx.RollbackAsync();

            if (identityUser is not null)
            {
                try
                {
                    await identityUserManager.DeleteUserAsync(identityUser.Id);
                }
                catch
                {
                    // Best-effort compensation. Original exception should be preserved.
                }
            }

            throw;
        }

    }

    public async Task<Result<CustomerOutputModel>> UpdateCustomerAsync(Guid id, UpdateCustomerInputModel model)
    {
        var customer = await unitOfWork.CustomerRepository.GetByIdAsync(id);
        if (customer is null)
        {
            return Result<CustomerOutputModel>.Failure(localizer[LocalizationKeys.CustomerNotFound, id], AppHttpStatusCodes.NotFound);
        }

        var areaValidation = await ValidateAreaIdAsync(model.AreaId);
        if (areaValidation is not null)
        {
            return areaValidation;
        }
        var genders = CachingFactory.GetValue<GenderEntity>(x => x.Id == model.GenderId).FirstOrDefault();
        if (genders is null)
        {
            return Result<CustomerOutputModel>.Failure(
                localizer[LocalizationKeys.GenderInvalid, model.GenderId],
                AppHttpStatusCodes.BadRequest);
        }

        var user = await GetUserByPartnerIdAsync(customer.PartnerId);
        if (user is null)
        {
            return Result<CustomerOutputModel>.Failure(localizer[LocalizationKeys.CustomerNotFound, id], AppHttpStatusCodes.NotFound);
        }

        var partner = await unitOfWork.PartnerRepository.GetByIdAsync(customer.PartnerId);
        if (partner is null)
        {
            return Result<CustomerOutputModel>.Failure(localizer[LocalizationKeys.CustomerNotFound, id], AppHttpStatusCodes.NotFound);
        }

        var normalizedMobile = string.IsNullOrWhiteSpace(model.MobileNo)
            ? (user.MobileNo ?? string.Empty).Trim()
            : model.MobileNo.Trim();

        var normalizedNationalNo = model.NationalNo is null
            ? null
            : (string.IsNullOrWhiteSpace(model.NationalNo) ? null : model.NationalNo.Trim());

        if (!string.Equals(user.MobileNo, normalizedMobile, StringComparison.OrdinalIgnoreCase) &&
            await unitOfWork.UserRepository.ExistsByMobileNoAsync(normalizedMobile))
        {
            return Result<CustomerOutputModel>.Failure(
                localizer[LocalizationKeys.UserPhoneNumberAlreadyExists, normalizedMobile],
                AppHttpStatusCodes.Conflict);
        }

        var normalizedEmail = model.Email is null
            ? null
            : (string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim());

        if (normalizedEmail is not null &&
            !string.Equals(user.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase) &&
            await unitOfWork.UserRepository.ExistsByEmailAsync(normalizedEmail))
        {
            return Result<CustomerOutputModel>.Failure(
                localizer[LocalizationKeys.UserEmailAlreadyExists, normalizedEmail],
                AppHttpStatusCodes.Conflict);
        }

        if (normalizedNationalNo is not null &&
            !string.Equals(user.NationalNo, normalizedNationalNo, StringComparison.OrdinalIgnoreCase) &&
            await unitOfWork.UserRepository.ExistsByNationalNoAsync(normalizedNationalNo))
        {
            return Result<CustomerOutputModel>.Failure(
                localizer[LocalizationKeys.UserNationalNoAlreadyExists, normalizedNationalNo],
                AppHttpStatusCodes.Conflict);
        }

        if (!string.Equals(user.Username, normalizedMobile, StringComparison.OrdinalIgnoreCase) &&
            await identityUserManager.FindByUsernameAsync(normalizedMobile) is { } existingByUsername)
        {
            if (string.IsNullOrWhiteSpace(user.IamRef) ||
                !string.Equals(existingByUsername.Id, user.IamRef, StringComparison.OrdinalIgnoreCase))
            {
                return Result<CustomerOutputModel>.Failure(
                    localizer[LocalizationKeys.IdentityUsernameAlreadyExists, normalizedMobile],
                    AppHttpStatusCodes.Conflict);
            }
        }

        if (normalizedEmail is not null &&
            !string.Equals(user.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase) &&
            await identityUserManager.FindByEmailAsync(normalizedEmail) is { } existingByEmail)
        {
            if (string.IsNullOrWhiteSpace(user.IamRef) ||
                !string.Equals(existingByEmail.Id, user.IamRef, StringComparison.OrdinalIgnoreCase))
            {
                return Result<CustomerOutputModel>.Failure(
                    localizer[LocalizationKeys.IdentityEmailAlreadyExists, normalizedEmail],
                    AppHttpStatusCodes.Conflict);
            }
        }

        var emailForKeycloak = model.Email is null
            ? (string.IsNullOrWhiteSpace(user.Email) ? null : user.Email.Trim())
            : normalizedEmail;

        if (!string.IsNullOrWhiteSpace(user.IamRef))
        {
            try
            {
                await identityUserManager.UpdateUserAsync(new UpdateIdentityUserRequest(
                    Id: user.IamRef,
                    Username: normalizedMobile,
                    Email: emailForKeycloak,
                    FirstName: model.FirstName.Trim(),
                    LastName: model.LastName.Trim(),
                    Enabled: user.Status == UserStatus.Active));
            }
            catch (InvalidOperationException ex)
            {
                return Result<CustomerOutputModel>.Failure(ex.Message, AppHttpStatusCodes.BadRequest);
            }
        }

        ApplyCustomerProfileUpdates(customer, user, model, normalizedMobile);

        var partnerDisplayAndLegal = $"{model.FirstName.Trim()} {model.LastName.Trim()}";
        partner.UpdateProfile(partnerDisplayAndLegal, partnerDisplayAndLegal, partner.Active);

        await unitOfWork.CustomerRepository.UpdateAsync(customer);
        unitOfWork.UserRepository.Update(user);
        unitOfWork.PartnerRepository.Update(partner);
        await unitOfWork.CompleteAsync();

        return Result<CustomerOutputModel>.Success(
            MapCustomerOutput(customer, user),
            localizer[LocalizationKeys.CustomerUpdateSuccess],
            AppHttpStatusCodes.Ok);
    }

    public async Task<Result<bool>> UpdateCustomerStatusAsync(Guid id, UpdateCustomerStatusInputModel model)
    {
        var customer = await unitOfWork.CustomerRepository.GetByIdAsync(id);
        if (customer is null)
        {
            return Result<bool>.Failure(localizer[LocalizationKeys.CustomerNotFound, id], AppHttpStatusCodes.NotFound);
        }

        customer.ChangeStatus(model.Status);
        await unitOfWork.CustomerRepository.UpdateAsync(customer);
        await unitOfWork.CompleteAsync();

        return Result<bool>.Success(
            true,
            localizer[LocalizationKeys.CustomerStatusUpdateSuccess],
            AppHttpStatusCodes.Ok);
    }

    public async Task<Result<bool>> DeleteCustomerAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await unitOfWork.CustomerRepository.GetByIdWithDeleteRelationsAsync(id, cancellationToken);
        if (customer is null)
        {
            return Result<bool>.Failure(
                localizer[LocalizationKeys.CustomerNotFound, id],
                AppHttpStatusCodes.NotFound);
        }

        var hasReferences =
            await unitOfWork.CustomerRepository.HasPaymentsAsync(customer.Id, cancellationToken)
            || await unitOfWork.CustomerRepository.HasSubscriptionsAsync(customer.Id, cancellationToken)
            || await unitOfWork.CustomerRepository.HasFavoritesAsync(customer.Id, cancellationToken)
            || await unitOfWork.PartnerRepository.HasJournalLinesOnAccountsAsync(customer.PartnerId, cancellationToken);

        if (hasReferences)
        {
            return Result<bool>.Failure(
                localizer[LocalizationKeys.CustomerDeleteBlockedByReferences],
                AppHttpStatusCodes.Conflict);
        }

        var user = await GetUserByPartnerIdAsync(customer.PartnerId, cancellationToken);
        var iamRef = user?.IamRef;

        await using var tx = await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var accounts = await unitOfWork.AccountRepository.GetByPartnerIdWithRelationsAsync(customer.PartnerId, cancellationToken);
            if (accounts.Count > 0)
            {
                unitOfWork.AccountRepository.RemoveRange(accounts);
            }

            unitOfWork.CustomerRepository.Remove(customer);
            if (user is not null)
            {
                unitOfWork.UserRepository.Remove(user);
            }

            if (customer.Partner is not null)
            {
                unitOfWork.PartnerRepository.Remove(customer.Partner);
            }

            await unitOfWork.CompleteAsync();
            await unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }

        if (!string.IsNullOrWhiteSpace(iamRef))
        {
            try
            {
                await identityUserManager.DeleteUserAsync(iamRef);
            }
            catch
            {
                // Best-effort cleanup; DB state is already committed.
            }
        }

        return Result<bool>.Success(true, localizer[LocalizationKeys.CustomerDeleteSuccess]);
    }

    private CustomerQueryInternalModel NormalizeQuery(CustomersQueryInputModel model)
    {
        var mapped = mapper.Map<CustomerQueryInternalModel>(model);

        var pageNumber = mapped.PageNumber <= 0 ? 1 : mapped.PageNumber;
        var pageSize = mapped.PageSize <= 0 ? 20 : Math.Min(mapped.PageSize, 100);
        var search = string.IsNullOrWhiteSpace(mapped.Search) ? null : mapped.Search.Trim();
        var sortBy = string.IsNullOrWhiteSpace(mapped.SortBy) ? null : mapped.SortBy.Trim();

        return mapped with
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            Search = search,
            SortBy = sortBy
        };
    }

    private IReadOnlyCollection<NotificationChannel> ResolveNotificationChannels(IReadOnlyCollection<int>? channels)
    {
        if (channels is null || channels.Count == 0)
        {
            return [NotificationChannel.Push, NotificationChannel.SMS, NotificationChannel.Email];
        }

        return channels
            .Distinct()
            .Select(value => Enum.IsDefined(typeof(NotificationChannel), value)
                ? (NotificationChannel)value
                : throw new ArgumentException(localizer[LocalizationKeys.NotificationChannelInvalid, value]))
            .ToArray();
    }

    private async Task<Result<CustomerOutputModel>?> ValidateAreaIdAsync(int? areaId)
    {
        if (!areaId.HasValue)
        {
            return null;
        }
        var areas = CachingFactory.GetValue<AreaEntity>(x => x.Id == areaId.Value).FirstOrDefault();
        if (areas is not null)
        {
            return null;
        }

        return Result<CustomerOutputModel>.Failure(
            localizer[LocalizationKeys.AreaInvalid, areaId.Value],
            AppHttpStatusCodes.BadRequest);
    }

    private async Task<Result<CustomerOutputModel>?> ValidateGenderIdForMyProfileAsync(int? genderId, int currentGenderId)
    {
        var effectiveId = genderId ?? currentGenderId;
        var gender = CachingFactory.GetValue<GenderEntity>(x => x.Id == effectiveId).FirstOrDefault();
        if (gender is not null)
        {
            return null;
        }

        return Result<CustomerOutputModel>.Failure(
            localizer[LocalizationKeys.GenderInvalid, effectiveId],
            AppHttpStatusCodes.BadRequest);
    }

    private void ApplyCustomerProfileUpdates(
        CustomerEntity customer,
        UserEntity user,
        UpdateCustomerInputModel model,
        string username)
    {
        user.UpdateProfile(
            username,
            model.FirstName,
            model.SecondName,
            model.ThirdName,
            model.LastName,
            model.GenderId,
            username,
            model.Email,
            model.NationalNo,
            language: model.Language);
        customer.UpdateProfile(model.AreaId, model.Address);

        if (model.NotificationChannels is { Count: > 0 })
        {
            user.SetNotificationPreferences(ResolveNotificationChannels(model.NotificationChannels));
        }
    }

    private void ApplyMyCustomerProfileUpdates(
        CustomerEntity customer,
        UserEntity user,
        UpdateMyCustomerProfileInputModel model,
        int genderId)
    {
        user.UpdateProfile(
            user.Username,
            model.FirstName,
            model.SecondName,
            model.ThirdName,
            model.LastName,
            genderId,
            user.MobileNo,
            model.Email,
            user.NationalNo,
            language: model.Language);
        customer.UpdateProfile(model.AreaId, model.Address);

        if (model.NotificationChannels is { Count: > 0 })
        {
            user.SetNotificationPreferences(ResolveNotificationChannels(model.NotificationChannels));
        }
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

    private async Task<UserEntity?> GetUserByPartnerIdAsync(long partnerId, CancellationToken cancellationToken = default)
    {
        var users = await unitOfWork.UserRepository.GetByPartnerIdsWithNotificationsDependenciesAsync([partnerId]);
        return users.FirstOrDefault(u => u.PartnerId == partnerId);
    }

    private static CustomerOutputModel MapCustomerOutput(CustomerEntity customer, UserEntity user) =>
        CustomerOutputModelFactory.FromCustomerAndUser(customer, user);
}

