using AutoMapper;
using SNEL.WalletPlatform.Application.Billers.Models.Output;
using SNEL.WalletPlatform.Domain.Billing;
using SNEL.WalletPlatform.Domain.Partners;

namespace SNEL.WalletPlatform.Application.Billers.Mapper;

public sealed class BillerMapper : Profile
{
    public BillerMapper()
    {
        CreateMap<BillerEntity, BillerOutputModel>();
        CreateMap<BillerEntity, BillerInfoOutputModel>();
        CreateMap<PartnerEntity, PartnerInfoOutputModel>();
    }
}
