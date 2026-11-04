using AutoMapper;
using SNEL.WalletPlatform.Application.Lookups.Models.Output;
using SNEL.WalletPlatform.Domain.Accounting;
using SNEL.WalletPlatform.Domain.Lookups;

namespace SNEL.WalletPlatform.Application.Lookups.Mapper;

public sealed class LookupMapper : Profile
{
    public LookupMapper()
    {
        CreateMap<CurrencyEntity, CurrencyOutputModel>();
        CreateMap<AccountTypeEntity, AccountTypeLookupOutputModel>()
            .ForCtorParam(nameof(AccountTypeLookupOutputModel.NameEn), opt => opt.MapFrom(src => src.Name))
            .ForCtorParam(nameof(AccountTypeLookupOutputModel.NameFr), opt => opt.MapFrom(src => src.Translations.FirstOrDefault(t => t.LanguageCode == "fr")!.Name));
    }
}
