using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Common.Localization;
using SNEL.WalletPlatform.Application.Common.Security;
using SNEL.WalletPlatform.Application.Interfaces.Auth;
using SNEL.WalletPlatform.Application.Interfaces.IAM;
using SNEL.WalletPlatform.Application.Interfaces.UnitOfWork;
using SNEL.WalletPlatform.Application.Users.Interfaces;
using SNEL.WalletPlatform.Application.Users.Models.Input;
using SNEL.WalletPlatform.Application.Users.Models.Internal;
using SNEL.WalletPlatform.Application.Users.Models.Output;
using Microsoft.Extensions.Localization;
using SNEL.WalletPlatform.Domain.Customers;
using SNEL.WalletPlatform.Domain.Partners;
using SNEL.WalletPlatform.Domain.Users;

namespace SNEL.WalletPlatform.Application.Users.Services;

public sealed class UserManagementService(
    IUnitOfWork unitOfWork,
    IIdentityUserManager identityUserManager,
    ICurrentUserContext currentUserContext,
    IUserManagementAccessPolicy accessPolicy,
    IUserPartnerResolver partnerResolver,
    IUserRoleResolver roleResolver,
    IStringLocalizer<SharedResource> localizer) : IUserManagementService
{
    private const string SnelBillerCode = "SNEL";
    public async Task<Result<PagedResult<ManagedUserOutputModel>>> GetUsersAsync(
        UsersQueryInputModel model,
        CancellationToken cancellationToken = default)
    {
        var scope = await accessPolicy.GetActorScopeAsync(cancellationToken);
        if (!scope.IsSuccess)
        {
            return Result<PagedResult<ManagedUserOutputModel>>.Failure(scope.Message!, scope.StatusCode);
        }
        var actorScope = scope.Value!;

        var normalizedPageNumber = model.PageNumber <= 0 ? 1 : model.PageNumber;
        var normalizedPageSize = model.PageSize <= 0 ? 20 : Math.Min(model.PageSize, 100);
        var users = await unitOfWork.UserRepository.GetPagedWithPartnerAsync(
            normalizedPageNumber,
            normalizedPageSize,
            model.Search,
            model.Status,
            actorScope.ScopePartnerId,
            model.SortBy,
            model.IsAscending,
            cancellationToken);

        var total = await unitOfWork.UserRepository.CountWithPartnerAsync(
            model.Search,
            model.Status,
            actorScope.ScopePartnerId,
            cancellationToken);

        var items = new List<ManagedUserOutputModel>(users.Count);
        foreach (var user in users)
        {
            var role = await roleResolver.ResolveManagedRoleAsync(user.IamRef);
            var customer = user.PartnerId.HasValue
                ? await unitOfWork.CustomerRepository.GetByPartnerIdAsync(user.PartnerId.Value, cancellationToken)
                : null;
            items.Add(MapOutput(user, role, customer));
        }

        return Result<PagedResult<ManagedUserOutputModel>>.Success(
            new PagedResult<ManagedUserOutputModel>(items, total));
    }

    public async Task<Result<ManagedUserOutputModel>> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var scope = await accessPolicy.GetActorScopeAsync(cancellationToken);
        if (!scope.IsSuccess)
        {
            return Result<ManagedUserOutputModel>.Failure(scope.Message!, scope.StatusCode);
        }
        var actorScope = scope.Value!;

        var user = await unitOfWork.UserRepository.GetByIdWithPartnerAsync(id, cancellationToken);
        if (user is null)
        {
            return Result<ManagedUserOutputModel>.Failure(localizer[LocalizationKeys.UserNotFound], AppHttpStatusCodes.NotFound);
        }

        if (!accessPolicy.CanAccessUser(actorScope, user))
        {
            return Result<ManagedUserOutputModel>.Failure(localizer[LocalizationKeys.UserManagementScopeOnlyOwnUsers], AppHttpStatusCodes.Forbidden);
        }

        var role = await roleResolver.ResolveManagedRoleAsync(user.IamRef);
        var customer = user.PartnerId.HasValue
            ? await unitOfWork.CustomerRepository.GetByPartnerIdAsync(user.PartnerId.Value, cancellationToken)
            : null;
        return Result<ManagedUserOutputModel>.Success(
            MapOutput(user, role, customer),
            localizer[LocalizationKeys.AuthPasswordUpdatedSuccessfully],
            AppHttpStatusCodes.Ok);
    }

    public async Task<Result<ManagedUserOutputModel>> CreateUserAsync(
        CreateManagedUserInputModel model,
        CancellationToken cancellationToken = default)
    {
        var scope = await accessPolicy.GetActorScopeAsync(cancellationToken);
        if (!scope.IsSuccess)
        {
            return Result<ManagedUserOutputModel>.Failure(scope.Message!, scope.StatusCode);
        }
        var actorScope = scope.Value!;

        var targetPartnerResult = await partnerResolver.ResolveTargetPartnerAsync(actorScope, model.PartnerId, cancellationToken);
        if (!targetPartnerResult.IsSuccess)
        {
            return Result<ManagedUserOutputModel>.Failure(targetPartnerResult.Message!, targetPartnerResult.StatusCode);
        }

        var targetPartner = targetPartnerResult.Value!;
        var normalizedRole = ResolveRoleFromPartnerType(targetPartner.PartnerTypeId);
        if (normalizedRole is null)
        {
            return Result<ManagedUserOutputModel>.Failure(localizer[LocalizationKeys.UserManagementInvalidRole], AppHttpStatusCodes.BadRequest);
        }

        if (!accessPolicy.CanManageRole(actorScope, normalizedRole))
        {
            return Result<ManagedUserOutputModel>.Failure(localizer[LocalizationKeys.UserManagementRoleNotAllowed], AppHttpStatusCodes.Forbidden);
        }

        if (model.MobileNo.Any(char.IsWhiteSpace))
        {
            return Result<ManagedUserOutputModel>.Failure(
                localizer[LocalizationKeys.UserMobileNoMustNotContainSpaces],
                AppHttpStatusCodes.BadRequest);
        }

        var normalizedMobile = model.MobileNo.Trim();
        if (normalizedMobile.Length is < 3 or > 30)
        {
            return Result<ManagedUserOutputModel>.Failure(
                localizer[LocalizationKeys.UserMobileNoInvalidLength],
                AppHttpStatusCodes.BadRequest);
        }
        var normalizedEmail = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim();
        var normalizedNationalNo = string.IsNullOrWhiteSpace(model.NationalNo) ? null : model.NationalNo.Trim();

        if (await unitOfWork.UserRepository.ExistsByMobileNoAsync(normalizedMobile))
        {
            return Result<ManagedUserOutputModel>.Failure(
                localizer[LocalizationKeys.IdentityUsernameAlreadyExists, normalizedMobile],
                AppHttpStatusCodes.Conflict);        }

        if (!string.IsNullOrWhiteSpace(normalizedEmail) &&
            await unitOfWork.UserRepository.ExistsByEmailAsync(normalizedEmail))
        {
            return Result<ManagedUserOutputModel>.Failure(localizer[LocalizationKeys.UserEmailAlreadyExists, normalizedEmail], AppHttpStatusCodes.Conflict);
        }

        if (!string.IsNullOrWhiteSpace(normalizedNationalNo) &&
            await unitOfWork.UserRepository.ExistsByNationalNoAsync(normalizedNationalNo))
        {
            return Result<ManagedUserOutputModel>.Failure(localizer[LocalizationKeys.UserNationalNoAlreadyExists, normalizedNationalNo], AppHttpStatusCodes.Conflict);
        }

        if (await identityUserManager.FindByUsernameAsync(normalizedMobile) is not null)
        {
            return Result<ManagedUserOutputModel>.Failure(localizer[LocalizationKeys.IdentityUsernameAlreadyExists, normalizedMobile], AppHttpStatusCodes.Conflict);
        }

        if (!string.IsNullOrWhiteSpace(normalizedEmail) &&
            await identityUserManager.FindByEmailAsync(normalizedEmail) is not null)
        {
            return Result<ManagedUserOutputModel>.Failure(localizer[LocalizationKeys.IdentityEmailAlreadyExists, normalizedEmail], AppHttpStatusCodes.Conflict);
        }

        IdentityUserModel? identityUser = null;
        await using var tx = await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            identityUser = await identityUserManager.CreateUserAsync(new CreateIdentityUserRequest(
                Username: normalizedMobile,
                Password: model.Password,
                Email: normalizedEmail,
                FirstName: model.FirstName.Trim(),
                LastName: model.LastName.Trim(),
                Enabled: true));
            var realmRoles = await ResolveRealmRolesAsync(targetPartner.PartnerTypeId, targetPartner.Id, cancellationToken);
            await identityUserManager.AssignRealmRolesAsync(identityUser.Id, realmRoles);

            var user = UserEntity.Create(
                username: normalizedMobile,
                firstName: model.FirstName,
                secondName: model.SecondName,
                thirdName: model.ThirdName,
                lastName: model.LastName,
                genderId: model.GenderId ?? 1,
                mobileNo: normalizedMobile,
                email: normalizedEmail ?? string.Empty,
                nationalNo: normalizedNationalNo ?? string.Empty,
                iamRef: identityUser.Id,
                language: model.Language,
                notificationChannels: [NotificationChannel.Push, NotificationChannel.SMS, NotificationChannel.Email]);

            user.LinkPartner(targetPartner);
            await unitOfWork.UserRepository.AddAsync(user);

            await unitOfWork.CompleteAsync();
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            var customer = await unitOfWork.CustomerRepository.GetByPartnerIdAsync(targetPartner.Id, cancellationToken);
            return Result<ManagedUserOutputModel>.Success(
                MapOutput(user, normalizedRole, customer),
                localizer[LocalizationKeys.UserCreateSuccess],
                AppHttpStatusCodes.Created);
        }
        catch (InvalidOperationException ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            if (identityUser is not null)
            {
                try { await identityUserManager.DeleteUserAsync(identityUser.Id); } catch { }
            }

            return Result<ManagedUserOutputModel>.Failure(ex.Message, AppHttpStatusCodes.BadRequest);
        }
        catch (HttpRequestException ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            if (identityUser is not null)
            {
                try { await identityUserManager.DeleteUserAsync(identityUser.Id); } catch { }
            }

            return Result<ManagedUserOutputModel>.Failure(ex.Message, AppHttpStatusCodes.BadRequest);
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            if (identityUser is not null)
            {
                try { await identityUserManager.DeleteUserAsync(identityUser.Id); } catch { }
            }

            throw;
        }
    }

    public async Task<Result<ManagedUserOutputModel>> UpdateUserAsync(
        Guid id,
        UpdateManagedUserInputModel model,
        CancellationToken cancellationToken = default)
    {
        var scope = await accessPolicy.GetActorScopeAsync(cancellationToken);
        if (!scope.IsSuccess)
        {
            return Result<ManagedUserOutputModel>.Failure(scope.Message!, scope.StatusCode);
        }
        var actorScope = scope.Value!;

        var user = await unitOfWork.UserRepository.GetByIdWithPartnerAsync(id, cancellationToken);
        if (user is null)
        {
            return Result<ManagedUserOutputModel>.Failure(localizer[LocalizationKeys.UserNotFound], AppHttpStatusCodes.NotFound);
        }

        if (!accessPolicy.CanAccessUser(actorScope, user))
        {
            return Result<ManagedUserOutputModel>.Failure(localizer[LocalizationKeys.UserManagementScopeOnlyOwnUsers], AppHttpStatusCodes.Forbidden);
        }

        if (string.IsNullOrWhiteSpace(user.IamRef))
        {
            return Result<ManagedUserOutputModel>.Failure(localizer[LocalizationKeys.UserIdentityRefMissing], AppHttpStatusCodes.BadRequest);
        }

        PartnerEntity targetPartner;
        if (!actorScope.IsPlatformAdmin)
        {
            if (user.Partner is null)
            {
                return Result<ManagedUserOutputModel>.Failure(
                    localizer[LocalizationKeys.PartnerNotFound],
                    AppHttpStatusCodes.NotFound);
            }

            targetPartner = user.Partner;
        }
        else
        {
            var effectivePartnerId = model.PartnerId > 0 ? model.PartnerId : user.PartnerId ?? 0;
            if (effectivePartnerId <= 0)
            {
                return Result<ManagedUserOutputModel>.Failure(
                    localizer[LocalizationKeys.UserManagementPartnerIdRequired],
                    AppHttpStatusCodes.BadRequest);
            }

            var targetPartnerResult = await partnerResolver.ResolveTargetPartnerAsync(
                actorScope,
                effectivePartnerId,
                cancellationToken);
            if (!targetPartnerResult.IsSuccess)
            {
                return Result<ManagedUserOutputModel>.Failure(targetPartnerResult.Message!, targetPartnerResult.StatusCode);
            }

            targetPartner = targetPartnerResult.Value!;
        }
        var normalizedRole = ResolveRoleFromPartnerType(targetPartner.PartnerTypeId);
        if (normalizedRole is null)
        {
            return Result<ManagedUserOutputModel>.Failure(localizer[LocalizationKeys.UserManagementInvalidRole], AppHttpStatusCodes.BadRequest);
        }

        if (!accessPolicy.CanManageRole(actorScope, normalizedRole))
        {
            return Result<ManagedUserOutputModel>.Failure(localizer[LocalizationKeys.UserManagementRoleNotAllowed], AppHttpStatusCodes.Forbidden);
        }

        if (model.MobileNo.Any(char.IsWhiteSpace))
        {
            return Result<ManagedUserOutputModel>.Failure(
                localizer[LocalizationKeys.UserMobileNoMustNotContainSpaces],
                AppHttpStatusCodes.BadRequest);
        }

        var normalizedMobile = model.MobileNo.Trim();
        if (normalizedMobile.Length is < 3 or > 30)
        {
            return Result<ManagedUserOutputModel>.Failure(
                localizer[LocalizationKeys.UserMobileNoInvalidLength],
                AppHttpStatusCodes.BadRequest);
        }
        var normalizedEmail = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim();
        var normalizedNationalNo = string.IsNullOrWhiteSpace(model.NationalNo) ? null : model.NationalNo.Trim();

        if (!string.Equals(user.MobileNo, normalizedMobile, StringComparison.OrdinalIgnoreCase) &&
            await unitOfWork.UserRepository.ExistsByMobileNoAsync(normalizedMobile))
        {
            return Result<ManagedUserOutputModel>.Failure(
                localizer[LocalizationKeys.IdentityUsernameAlreadyExists, normalizedMobile],
                AppHttpStatusCodes.Conflict);
        }

        if (!string.IsNullOrWhiteSpace(normalizedEmail) &&
            !string.Equals(user.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase) &&
            await unitOfWork.UserRepository.ExistsByEmailAsync(normalizedEmail))
        {
            return Result<ManagedUserOutputModel>.Failure(localizer[LocalizationKeys.UserEmailAlreadyExists, normalizedEmail], AppHttpStatusCodes.Conflict);
        }

        await identityUserManager.UpdateUserAsync(new UpdateIdentityUserRequest(
            Id: user.IamRef!,
            Username: user.Username,
            Email: normalizedEmail,
            FirstName: model.FirstName.Trim(),
            LastName: model.LastName.Trim(),
            Enabled: model.Status == UserStatus.Active));
        var realmRoles = await ResolveRealmRolesAsync(targetPartner.PartnerTypeId, targetPartner.Id, cancellationToken);
        await identityUserManager.ReplaceRealmRolesAsync(user.IamRef!, realmRoles);

        user.UpdateProfile(
            username: user.Username,
            firstName: model.FirstName,
            secondName: model.SecondName,
            thirdName: model.ThirdName,
            lastName: model.LastName,
            genderId: model.GenderId ?? user.GenderId,
            mobileNo: normalizedMobile,
            email: normalizedEmail,
            nationalNo: normalizedNationalNo,
            language: model.Language);
        user.ChangeStatus(model.Status);
        user.LinkPartner(targetPartner);

        unitOfWork.UserRepository.Update(user);
        await unitOfWork.CompleteAsync();

        var customer = user.PartnerId.HasValue
            ? await unitOfWork.CustomerRepository.GetByPartnerIdAsync(user.PartnerId.Value, cancellationToken)
            : null;
        return Result<ManagedUserOutputModel>.Success(
            MapOutput(user, normalizedRole, customer),
            localizer[LocalizationKeys.UserUpdateSuccess],
            AppHttpStatusCodes.Ok);
    }

    public async Task<Result<ManagedUserOutputModel>> ChangeUserPasswordAsync(
        Guid id,
        ChangeManagedUserPasswordInputModel model,
        CancellationToken cancellationToken = default)
    {
        var scope = await accessPolicy.GetActorScopeAsync(cancellationToken);
        if (!scope.IsSuccess)
        {
            return Result<ManagedUserOutputModel>.Failure(scope.Message!, scope.StatusCode);
        }
        var actorScope = scope.Value!;

        var user = await unitOfWork.UserRepository.GetByIdWithPartnerAsync(id, cancellationToken);
        if (user is null)
        {
            return Result<ManagedUserOutputModel>.Failure(localizer[LocalizationKeys.UserNotFound], AppHttpStatusCodes.NotFound);
        }

        if (!accessPolicy.CanAccessUser(actorScope, user))
        {
            return Result<ManagedUserOutputModel>.Failure(localizer[LocalizationKeys.UserManagementScopeOnlyOwnUsers], AppHttpStatusCodes.Forbidden);
        }

        if (string.IsNullOrWhiteSpace(user.IamRef))
        {
            return Result<ManagedUserOutputModel>.Failure(localizer[LocalizationKeys.UserIdentityRefMissing], AppHttpStatusCodes.BadRequest);
        }
        var password = model.NewPassword.Trim();
        await identityUserManager.SetPasswordAsync(user.IamRef, password);

        var role = await roleResolver.ResolveManagedRoleAsync(user.IamRef);
        var customer = user.PartnerId.HasValue
            ? await unitOfWork.CustomerRepository.GetByPartnerIdAsync(user.PartnerId.Value, cancellationToken)
            : null;
        return Result<ManagedUserOutputModel>.Success(
            MapOutput(user, role, customer),
            localizer[LocalizationKeys.AuthMobileUpdatedSuccessfully],
            AppHttpStatusCodes.Ok);
    }

    public async Task<Result<ManagedUserOutputModel>> ChangeCustomerMobileAsync(
        Guid id,
        ChangeCustomerMobileInputModel model,
        CancellationToken cancellationToken = default)
    {
        var scope = await accessPolicy.GetActorScopeAsync(cancellationToken);
        if (!scope.IsSuccess)
        {
            return Result<ManagedUserOutputModel>.Failure(scope.Message!, scope.StatusCode);
        }
        var actorScope = scope.Value!;

        var user = await unitOfWork.UserRepository.GetByIdWithPartnerAsync(id, cancellationToken);
        if (user is null)
        {
            return Result<ManagedUserOutputModel>.Failure(localizer[LocalizationKeys.UserNotFound], AppHttpStatusCodes.NotFound);
        }

        if (!accessPolicy.CanAccessUser(actorScope, user))
        {
            return Result<ManagedUserOutputModel>.Failure(localizer[LocalizationKeys.UserManagementScopeOnlyOwnUsers], AppHttpStatusCodes.Forbidden);
        }

        if (!await IsCustomerUserAsync(user, cancellationToken))
        {
            return Result<ManagedUserOutputModel>.Failure(
                localizer[LocalizationKeys.UserManagementTargetMustBeCustomer],
                AppHttpStatusCodes.Forbidden);
        }

        if (string.IsNullOrWhiteSpace(user.IamRef))
        {
            return Result<ManagedUserOutputModel>.Failure(localizer[LocalizationKeys.UserIdentityRefMissing], AppHttpStatusCodes.BadRequest);
        }

        var normalizedMobile = model.MobileNo.Trim();
        if (!string.Equals(user.MobileNo, normalizedMobile, StringComparison.OrdinalIgnoreCase) &&
            await unitOfWork.UserRepository.ExistsByMobileNoAsync(normalizedMobile))
        {
            return Result<ManagedUserOutputModel>.Failure(
                localizer[LocalizationKeys.UserPhoneNumberAlreadyExists, normalizedMobile],
                AppHttpStatusCodes.Conflict);
        }

        if (await identityUserManager.FindByUsernameAsync(normalizedMobile) is { } existingIdentity
            && !string.Equals(existingIdentity.Id, user.IamRef, StringComparison.OrdinalIgnoreCase))
        {
            return Result<ManagedUserOutputModel>.Failure(
                localizer[LocalizationKeys.IdentityUsernameAlreadyExists, normalizedMobile],
                AppHttpStatusCodes.Conflict);
        }

        var normalizedEmail = string.IsNullOrWhiteSpace(user.Email) ? null : user.Email.Trim();
        await identityUserManager.UpdateUserAsync(new UpdateIdentityUserRequest(
            Id: user.IamRef!,
            Username: normalizedMobile,
            Email: normalizedEmail,
            FirstName: user.FirstName,
            LastName: user.LastName,
            Enabled: user.Status == UserStatus.Active));

        user.UpdateMobileNo(normalizedMobile, normalizedMobile);
        unitOfWork.UserRepository.Update(user);
        await unitOfWork.CompleteAsync();

        var role = await roleResolver.ResolveManagedRoleAsync(user.IamRef);
        var customer = user.PartnerId.HasValue
            ? await unitOfWork.CustomerRepository.GetByPartnerIdAsync(user.PartnerId.Value, cancellationToken)
            : null;
        return Result<ManagedUserOutputModel>.Success(MapOutput(user, role, customer));
    }

    private async Task<bool> IsCustomerUserAsync(UserEntity user, CancellationToken cancellationToken)
    {
        if (user.Partner?.PartnerTypeId == PartnerTypeConstants.Customer)
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(user.IamRef))
        {
            return false;
        }

        var role = await roleResolver.ResolveManagedRoleAsync(user.IamRef);
        return string.Equals(role, IdentityRoleConstants.Customer, StringComparison.OrdinalIgnoreCase);
    }

    public async Task<Result<bool>> DeleteUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var scope = await accessPolicy.GetActorScopeAsync(cancellationToken);
        if (!scope.IsSuccess)
        {
            return Result<bool>.Failure(scope.Message!, scope.StatusCode);
        }
        var actorScope = scope.Value!;

        var user = await unitOfWork.UserRepository.GetByIdWithDeleteDependenciesAsync(id, cancellationToken);
        if (user is null)
        {
            return Result<bool>.Failure(localizer[LocalizationKeys.UserNotFound], AppHttpStatusCodes.NotFound);
        }

        if (!accessPolicy.CanAccessUser(actorScope, user))
        {
            return Result<bool>.Failure(localizer[LocalizationKeys.UserManagementScopeOnlyOwnUsers], AppHttpStatusCodes.Forbidden);
        }

        if (string.Equals(user.IamRef, currentUserContext.UserId, StringComparison.OrdinalIgnoreCase))
        {
            return Result<bool>.Failure(localizer[LocalizationKeys.UserCannotDeleteSelf], AppHttpStatusCodes.BadRequest);
        }

        if (await unitOfWork.UserRepository.HasRegistrationHistoryAsEmployeeAsync(id, cancellationToken))
        {
            return Result<bool>.Failure(
                localizer[LocalizationKeys.UserDeleteBlockedByReferences],
                AppHttpStatusCodes.Conflict);
        }

        var iamRef = user.IamRef;

        await using var tx = await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            unitOfWork.UserRepository.Remove(user);
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

        return Result<bool>.Success(
            true,
            localizer[LocalizationKeys.UserDeleteSuccess],
            AppHttpStatusCodes.Ok);
    }

    private static string? ResolveRoleFromPartnerType(int partnerTypeId)
    {
        return partnerTypeId switch
        {
            PartnerTypeConstants.Platform => IdentityRoleConstants.PlatformAdmin,
            PartnerTypeConstants.Biller => IdentityRoleConstants.BillerAdmin,
            PartnerTypeConstants.Psp => IdentityRoleConstants.PspAdmin,
            PartnerTypeConstants.Customer => IdentityRoleConstants.CustomerAdmin,
            _ => null
        };
    }

    private async Task<IReadOnlyCollection<string>> ResolveRealmRolesAsync(
        int partnerTypeId,
        long partnerId,
        CancellationToken cancellationToken)
    {
        var role = ResolveRoleFromPartnerType(partnerTypeId);
        if (role is null)
        {
            return [];
        }

        if (string.Equals(role, IdentityRoleConstants.BillerAdmin, StringComparison.OrdinalIgnoreCase) &&
            await IsSnelPartnerAsync(partnerId, cancellationToken))
        {
            return [role, IdentityRoleConstants.WalletOwner];
        }

        return [role];
    }

    private async Task<bool> IsSnelPartnerAsync(long partnerId, CancellationToken cancellationToken)
    {
        var billers = await unitOfWork.BillerRepository.GetAllScopedByPartnerAsync(partnerId, cancellationToken);
        return billers.Any(b => string.Equals(b.Code, SnelBillerCode, StringComparison.OrdinalIgnoreCase));
    }

    private static ManagedUserOutputModel MapOutput(UserEntity user, string role, CustomerEntity? customer)
    {
        return new ManagedUserOutputModel(
            Id: user.Id,
            IamRef: user.IamRef ?? string.Empty,
            Username: user.Username,
            FirstName: user.FirstName,
            SecondName: user.SecondName,
            ThirdName: user.ThirdName,
            LastName: user.LastName,
            GenderId: user.GenderId,
            MobileNo: user.MobileNo,
            Email: user.Email,
            NationalNo: user.NationalNo,
            Language: user.Language,
            Status: user.Status,
            Role: role,
            PartnerId: user.PartnerId,
            PartnerTypeId: user.Partner?.PartnerTypeId,
            CustomerId: customer?.Id,
            CreatedAtUtc: user.CreatedAtUtc,
            UpdatedAtUtc: user.UpdatedAtUtc);
    }

}
