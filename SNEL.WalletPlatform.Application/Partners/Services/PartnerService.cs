using AutoMapper;
using Microsoft.Extensions.Localization;
using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Common.Localization;
using SNEL.WalletPlatform.Application.Common.Security;
using SNEL.WalletPlatform.Application.Interfaces.Auth;
using SNEL.WalletPlatform.Application.Interfaces.UnitOfWork;
using SNEL.WalletPlatform.Application.Partners.Interfaces;
using SNEL.WalletPlatform.Application.Partners.Models.Input;
using SNEL.WalletPlatform.Application.Partners.Models.Output;

namespace SNEL.WalletPlatform.Application.Partners.Services;

public sealed class PartnerService(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IStringLocalizer<SharedResource> localizer,
    ICurrentUserContext currentUserContext) : IPartnerService
{
    #region ApplicationService Methods

    public async Task<Result<PagedResult<PartnerOutputModel>>> GetPartnersAsync(PartnersQueryInputModel model)
    {
        var normalizedQuery = NormalizeQuery(model);
        var partners = await unitOfWork.PartnerRepository.GetPagedAsync(normalizedQuery);

        var totalCount = await unitOfWork.PartnerRepository.CountAsync(normalizedQuery);
        var items = mapper.Map<PartnerOutputModel[]>(partners);

        return Result<PagedResult<PartnerOutputModel>>.Success(new PagedResult<PartnerOutputModel>(items, totalCount));
    }

    public async Task<Result<PagedResult<PartnerOutputModel>>> GetVisiblePartnersAsync(RelatedPartnersQueryInputModel model)
    {
        if (string.IsNullOrWhiteSpace(currentUserContext.UserId))
        {
            return Result<PagedResult<PartnerOutputModel>>.Failure(localizer[LocalizationKeys.UserContextMissing], AppHttpStatusCodes.Unauthorized);
        }

        var actor = await unitOfWork.UserRepository.GetByIamRefWithPartnerAsync(currentUserContext.UserId);
        if (actor is null)
        {
            return Result<PagedResult<PartnerOutputModel>>.Failure(localizer[LocalizationKeys.UserActorNotFound], AppHttpStatusCodes.NotFound);
        }

        if (!actor.PartnerId.HasValue || actor.Partner is null)
        {
            return Result<PagedResult<PartnerOutputModel>>.Failure(localizer[LocalizationKeys.UserPartnerLinkRequired], AppHttpStatusCodes.BadRequest);
        }

        var canViewAllNonCustomerPartners =
            actor.Partner.PartnerTypeId == PartnerTypeConstants.Platform &&
            currentUserContext.IsInRole(IdentityRoleConstants.PlatformAdmin);

        var query = NormalizeRelatedPartnersQuery(model);

        var totalCount = await unitOfWork.PartnerRepository.CountByVisibilityScopeAsync(
            actor.PartnerId.Value,
            canViewAllNonCustomerPartners,
            query);

        var partners = await unitOfWork.PartnerRepository.GetPagedByVisibilityScopeAsync(
            actor.PartnerId.Value,
            canViewAllNonCustomerPartners,
            query);

        var items = mapper.Map<PartnerOutputModel[]>(partners);
        return Result<PagedResult<PartnerOutputModel>>.Success(new PagedResult<PartnerOutputModel>(items, totalCount));
    }

    private PartnersQueryInputModel NormalizeQuery(PartnersQueryInputModel model)
    {
        var mapped = mapper.Map<PartnersQueryInputModel>(model);

        mapped.PageNumber = mapped.PageNumber <= 0 ? 1 : mapped.PageNumber;
        mapped.PageSize = mapped.PageSize <= 0 ? 20 : Math.Min(mapped.PageSize, 100);
        mapped.SortBy = string.IsNullOrWhiteSpace(mapped.SortBy) ? null : mapped.SortBy.Trim();

        return mapped;
    }

    private static RelatedPartnersQueryInputModel NormalizeRelatedPartnersQuery(RelatedPartnersQueryInputModel model)
    {
        var normalized = new RelatedPartnersQueryInputModel
        {
            PageNumber = model.PageNumber <= 0 ? 1 : model.PageNumber,
            PageSize = model.PageSize <= 0 ? 20 : Math.Min(model.PageSize, 100),
            Search = string.IsNullOrWhiteSpace(model.Search) ? null : model.Search.Trim(),
            SortBy = string.IsNullOrWhiteSpace(model.SortBy) ? null : model.SortBy.Trim(),
            IsAscending = model.IsAscending
        };

        // Match previous default (newest first) when the client omits sort.
        if (normalized.SortBy is null)
        {
            normalized.IsAscending = false;
        }

        return normalized;
    }

    #endregion

}