using AutoMapper;
using SNEL.WalletPlatform.Application.Auth.Models.Input;
using SNEL.WalletPlatform.Application.Customers.Models.Input;
using SNEL.WalletPlatform.Application.Customers.Models.Internal;
using SNEL.WalletPlatform.Application.Customers.Models.Output;
using SNEL.WalletPlatform.Domain.Customers;

namespace SNEL.WalletPlatform.Application.Customers.Mapper;

public sealed class CustomerMapper : Profile
{
    public CustomerMapper()
    {
        CreateMap<CompleteRegistrationInputModel, CreateCustomerInputModel>();

        CreateMap<CustomersQueryInputModel, CustomerQueryInternalModel>()
            .ConstructUsing(src => new CustomerQueryInternalModel(
                src.Status,
                src.PageNumber,
                src.PageSize,
                src.Search,
                src.SortBy,
                src.IsAscending)); 
        
        CreateMap<CustomerSubscriptionsQueryInputModel, CustomerSubscriptionsQueryInternalModel>()
            .ConstructUsing(src => new CustomerSubscriptionsQueryInternalModel(
                null,
                src.BillingServiceId,
                src.PageNumber,
                src.PageSize,
                src.Search,
                src.SortBy,
                src.IsAscending));
    }
}
