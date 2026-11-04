using AutoMapper;
using SNEL.WalletPlatform.Application.Notifications.Models.Output;
using SNEL.WalletPlatform.Domain.Notifications;

namespace SNEL.WalletPlatform.Application.Notifications.Mapper;

public sealed class CampaignMapper : Profile
{
    public CampaignMapper()
    {
        CreateMap<CampaignTranslationEntity, CampaignTranslationOutputModel>()
            .ForCtorParam(nameof(CampaignTranslationOutputModel.Title), opt => opt.MapFrom(src => src.Name));
        CreateMap<CampaignEntity, CampaignOutputModel>()
            .ForCtorParam(nameof(CampaignOutputModel.Translations), opt => opt.MapFrom(src =>
                src.Translations
                    .OrderBy(t => t.LanguageCode)
                    .ToList()));
    }
}
