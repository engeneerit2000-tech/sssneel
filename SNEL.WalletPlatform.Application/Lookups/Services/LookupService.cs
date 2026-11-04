using Microsoft.Extensions.Localization;
using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Interfaces.UnitOfWork;
using SNEL.WalletPlatform.Application.Lookups.Interfaces;
using SNEL.WalletPlatform.Application.Lookups.Models.Output;
using SNEL.WalletPlatform.Application.Notifications.Interfaces;

namespace SNEL.WalletPlatform.Application.Lookups.Services;

public sealed class LookupService(
    IUnitOfWork unitOfWork,
    IStringLocalizer<SharedResource> localizer,
    ICampaignAudienceResolver campaignAudienceResolver) : ILookupService
{
    public async Task<Result<AllLookupsOutputModel>> GetAllAsync()
    {
        var lookups = await unitOfWork.LookupRepository.GetAllAsync(localizer);
        var campaignAudienceGroups = campaignAudienceResolver.GetAll()
            .Select(x => new CampaignAudienceGroupLookupOutputModel(
                x.AudienceGroupCode,
                x.NameEn,
                x.NameFr))
            .ToList();

        return Result<AllLookupsOutputModel>.Success(
            lookups with { CampaignAudienceGroups = campaignAudienceGroups });
    }
}
