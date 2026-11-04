using Microsoft.Extensions.Localization;
using SNEL.WalletPlatform.Application.BillingServices.Interfaces;
using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Common.Localization;
using SNEL.WalletPlatform.Application.Common.Security;
using SNEL.WalletPlatform.Application.Interfaces.Auth;
using SNEL.WalletPlatform.Application.Interfaces.UnitOfWork;

namespace SNEL.WalletPlatform.Application.BillingServices.Services;

public sealed class BillingServiceAccessPolicy(
    IUnitOfWork unitOfWork,
    ICurrentUserContext currentUserContext,
    IStringLocalizer<SharedResource> localizer) : IBillingServiceAccessPolicy
{
    public async Task<Result<long?>> GetBillingServicesReadScopeAsync(CancellationToken cancellationToken = default)
    {
        if (currentUserContext.IsInRole(IdentityRoleConstants.PlatformAdmin) || IsCustomerRole())
        {
            return Result<long?>.Success(null);
        }

        if (!currentUserContext.IsInRole(IdentityRoleConstants.BillerAdmin))
        {
            return Result<long?>.Failure(localizer[LocalizationKeys.UserManagementForbidden], AppHttpStatusCodes.Forbidden);
        }

        return await ResolveBillerPartnerScopeAsync(cancellationToken);
    }

    public async Task<Result<long?>> GetBillingServiceManagementScopeAsync(CancellationToken cancellationToken = default)
    {
        if (currentUserContext.IsInRole(IdentityRoleConstants.PlatformAdmin))
        {
            return Result<long?>.Success(null);
        }

        if (!currentUserContext.IsInRole(IdentityRoleConstants.BillerAdmin))
        {
            return Result<long?>.Failure(localizer[LocalizationKeys.UserManagementForbidden], AppHttpStatusCodes.Forbidden);
        }

        return await ResolveBillerPartnerScopeAsync(cancellationToken);
    }

    private async Task<Result<long?>> ResolveBillerPartnerScopeAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(currentUserContext.UserId))
        {
            return Result<long?>.Failure(localizer[LocalizationKeys.UserManagementForbidden], AppHttpStatusCodes.Forbidden);
        }

        var user = await unitOfWork.UserRepository.GetByIamRefWithPartnerAsync(currentUserContext.UserId, cancellationToken);
        if (user?.PartnerId is null)
        {
            return Result<long?>.Failure(localizer[LocalizationKeys.UserManagementForbidden], AppHttpStatusCodes.Forbidden);
        }

        return Result<long?>.Success(user.PartnerId.Value);
    }

    private bool IsCustomerRole()
    {
        return currentUserContext.IsInRole(IdentityRoleConstants.CustomerAdmin)
               || currentUserContext.IsInRole(IdentityRoleConstants.CustomerUser)
               || currentUserContext.IsInRole(IdentityRoleConstants.CustomerUserTitleCase);
    }
}
