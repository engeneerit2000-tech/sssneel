using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Common.Localization;
using SNEL.WalletPlatform.Application.Common.Security;
using SNEL.WalletPlatform.Application.Interfaces.UnitOfWork;
using SNEL.WalletPlatform.Application.Users.Interfaces;
using SNEL.WalletPlatform.Application.Users.Models.Internal;
using SNEL.WalletPlatform.Domain.Partners;
using Microsoft.Extensions.Localization;

namespace SNEL.WalletPlatform.Application.Users.Services;

public sealed class UserPartnerResolver(
    IUnitOfWork unitOfWork,
    IStringLocalizer<SharedResource> localizer) : IUserPartnerResolver
{
    public async Task<Result<PartnerEntity>> ResolveTargetPartnerAsync(
        ActorScope scope,
        long requestedPartnerId,
        CancellationToken cancellationToken = default)
    {
        if (!scope.IsPlatformAdmin)
        {
            if (!scope.ScopePartnerId.HasValue)
            {
                return Result<PartnerEntity>.Failure(localizer[LocalizationKeys.UserPartnerLinkRequired], AppHttpStatusCodes.BadRequest);
            }

            var actorPartner = await unitOfWork.PartnerRepository.GetByIdAsync(scope.ScopePartnerId.Value);
            if (actorPartner is null)
            {
                return Result<PartnerEntity>.Failure(localizer[LocalizationKeys.PartnerNotFound], AppHttpStatusCodes.NotFound);
            }

            return Result<PartnerEntity>.Success(actorPartner);
        }

        var partner = await unitOfWork.PartnerRepository.GetByIdAsync(requestedPartnerId);
        if (partner is null)
        {
            return Result<PartnerEntity>.Failure(localizer[LocalizationKeys.PartnerNotFound], AppHttpStatusCodes.NotFound);
        }

        return Result<PartnerEntity>.Success(partner);
    }
}
