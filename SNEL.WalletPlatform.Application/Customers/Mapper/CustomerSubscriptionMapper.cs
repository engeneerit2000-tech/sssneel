using AutoMapper;
using SNEL.WalletPlatform.Application.Customers.Models.Output;
using SNEL.WalletPlatform.Domain.Customers;

namespace SNEL.WalletPlatform.Application.Customers.Mapper;

public sealed class CustomerSubscriptionMapper : Profile
{
    public CustomerSubscriptionMapper()
    {
        // CustomerOutputModel needs UserEntity (via partner); map in CustomerSubscriptionOutputMapping after Map.
        CreateMap<CustomerSubscriptionEntity, CustomerSubscriptionOutputModel>()
            .ForMember(dest => dest.Customer, opt => opt.Ignore())
            .ForCtorParam("billingRefForDisplay", opt => opt.MapFrom(src => src.BillingRefValue.Contains('-') ? src.BillingRefValue.Substring(src.BillingRefValue.LastIndexOf('-') + 1) : src.BillingRefValue ))
            .ForCtorParam("customer", opt => opt.MapFrom(_ => (CustomerOutputModel?)null));
    }
}
