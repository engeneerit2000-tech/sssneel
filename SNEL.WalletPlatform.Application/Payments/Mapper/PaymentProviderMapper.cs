using AutoMapper;
using SNEL.WalletPlatform.Application.Accounting.Models.Output;
using SNEL.WalletPlatform.Application.Payments.Models.Output;
using SNEL.WalletPlatform.Domain.Payments;

namespace SNEL.WalletPlatform.Application.Payments.Mapper;

public sealed class PaymentProviderMapper : Profile
{
    public PaymentProviderMapper()
    {
        CreateMap<PaymentProviderEntity, PaymentProviderOutputModel>()
            .ForCtorParam(nameof(PaymentProviderOutputModel.Accounts), opt => opt.MapFrom(_ => Array.Empty<AccountOutputModel>()))
            .ForCtorParam(nameof(PaymentProviderOutputModel.Active), opt => opt.MapFrom(src => src.Partner != null && src.Partner.Active));
    }
}
