using AutoMapper;
using SNEL.WalletPlatform.Application.Invoices.Models.Input;
using SNEL.WalletPlatform.Application.Invoices.Models.Output;
using SNEL.WalletPlatform.Domain.Billing;
using SNEL.WalletPlatform.Domain.Payments;

namespace SNEL.WalletPlatform.Application.Invoices.Mapper;

public sealed class InvoiceMapper : Profile
{
    public InvoiceMapper()
    {
        CreateMap<InvoicesQueryInputModel, InvoicesQueryInternalModel>();
        CreateMap<InvoiceEntity, InvoiceOutputModel>()
            .ForCtorParam(nameof(InvoiceOutputModel.AmountOutstanding), opt => opt.MapFrom(src => 
                src.PaymentOrderItems == null 
                    ? src.AmountOutstanding 
                    : src.AmountOutstanding - src.PaymentOrderItems.Where(x => x.Status == PaymentOrderItemStatus.Paid).Sum(x => x.Amount)));
    }
}
