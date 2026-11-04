using AutoMapper;
using SNEL.WalletPlatform.Application.Payments.Models.Output;
using SNEL.WalletPlatform.Domain.Payments;

namespace SNEL.WalletPlatform.Application.Payments.Mapper;

public sealed class PaymentTxnMapper : Profile
{
    public PaymentTxnMapper()
    {
        CreateMap<PaymentTxnEntity, PaymentTxnOutputModel>();
    }
}
