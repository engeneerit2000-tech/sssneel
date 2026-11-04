using AutoMapper;
using SNEL.WalletPlatform.Application.Partners.Models.Output;
using SNEL.WalletPlatform.Domain.Lookups;
using SNEL.WalletPlatform.Domain.Partners;

namespace SNEL.WalletPlatform.Application.Accounting.Mapper;

public sealed class PartnerMapper : Profile
{
    public PartnerMapper()
    {
        CreateMap<PartnerEntity, PartnerOutputModel>();
        CreateMap<PartnerTypeEntity, PartnerTypeOutputModel>();
    }
}
