using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Common.Localization;
using SNEL.WalletPlatform.Application.Common.Security;
using SNEL.WalletPlatform.Application.Interfaces.Auth;
using SNEL.WalletPlatform.Application.Interfaces.UnitOfWork;
using SNEL.WalletPlatform.Application.Users.Interfaces;
using SNEL.WalletPlatform.Application.Users.Models.Internal;
using SNEL.WalletPlatform.Domain.Users;
using Microsoft.Extensions.Localization;

namespace SNEL.WalletPlatform.Application.Users.Services;

public sealed class UserManagementAccessPolicy(
    IUnitOfWork unitOfWork,
    ICurrentUserContext currentUserContext,
    IStringLocalizer<SharedResource> localizer) : IUserManagementAccessPolicy
{
    public async Task<Result<ActorScope>> GetActorScopeAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(currentUserContext.UserId))
        {
            return Result<ActorScope>.Failure(localizer[LocalizationKeys.UserContextMissing], AppHttpStatusCodes.Unauthorized);
        }

        var actor = await unitOfWork.UserRepository.GetByIamRefWithPartnerAsync(currentUserContext.UserId, cancellationToken);
        if (actor is null)
        {
            return Result<ActorScope>.Failure(localizer[LocalizationKeys.UserActorNotFound], AppHttpStatusCodes.NotFound);
        }

        if (!actor.PartnerId.HasValue || actor.Partner is null)
        {
            return Result<ActorScope>.Failure(localizer[LocalizationKeys.UserPartnerLinkRequired], AppHttpStatusCodes.BadRequest);
        }

        var partnerTypeId = actor.Partner.PartnerTypeId;
        var isPlatform = partnerTypeId == PartnerTypeConstants.Platform;
        var isBiller = partnerTypeId == PartnerTypeConstants.Biller;
        var isPsp = partnerTypeId == PartnerTypeConstants.Psp;

        if (!isPlatform && !isBiller && !isPsp)
        {
            return Result<ActorScope>.Failure(localizer[LocalizationKeys.UserManagementForbidden], AppHttpStatusCodes.Forbidden);
        }

        return Result<ActorScope>.Success(
            isPlatform
                ? new ActorScope(true, false, false, null)
                : new ActorScope(false, isBiller, isPsp, actor.PartnerId.Value));
    }

    public bool CanAccessUser(ActorScope scope, UserEntity user)
    {
        if (scope.IsPlatformAdmin)
        {
            return true;
        }

        return scope.ScopePartnerId.HasValue && user.PartnerId == scope.ScopePartnerId;
    }

    public bool CanManageRole(ActorScope scope, string role)
    {
        if (scope.IsPlatformAdmin)
        {
            return IdentityRoleConstants.ManageableRoles.Contains(role, StringComparer.OrdinalIgnoreCase);
        }

        if (scope.IsBillerAdmin)
        {
            return string.Equals(role, IdentityRoleConstants.BillerAdmin, StringComparison.OrdinalIgnoreCase);
        }

        if (scope.IsPspAdmin)
        {
            return string.Equals(role, IdentityRoleConstants.PspAdmin, StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }
}
