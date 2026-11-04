using Microsoft.Extensions.Localization;
using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Common.Localization;
using SNEL.WalletPlatform.Application.Common.Security;
using SNEL.WalletPlatform.Application.Interfaces.Auth;
using SNEL.WalletPlatform.Application.Interfaces.UnitOfWork;
using SNEL.WalletPlatform.Application.Payments.Interfaces;

namespace SNEL.WalletPlatform.Application.Payments.Services;

public sealed class PaymentProviderAccessPolicy(
    IUnitOfWork unitOfWork,
    ICurrentUserContext currentUserContext,
    IStringLocalizer<SharedResource> localizer) : IPaymentProviderAccessPolicy
{
    public async Task<Result<long?>> GetPaymentProvidersReadScopeAsync(CancellationToken cancellationToken = default)
    {
        if (currentUserContext.IsInRole(IdentityRoleConstants.PlatformAdmin) || currentUserContext.IsInRole(IdentityRoleConstants.WalletOwner) || IsCustomerRole())
        {
            return Result<long?>.Success(null);
        }

        if (!currentUserContext.IsInRole(IdentityRoleConstants.PspAdmin) || string.IsNullOrWhiteSpace(currentUserContext.UserId))
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
