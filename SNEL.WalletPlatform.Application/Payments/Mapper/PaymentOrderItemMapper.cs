using AutoMapper;
using SNEL.WalletPlatform.Application.Payments.Models.Output;
using SNEL.WalletPlatform.Domain.Payments;

namespace SNEL.WalletPlatform.Application.Payments.Mapper;

public sealed class PaymentOrderItemMapper : Profile
{
    public PaymentOrderItemMapper()
    {
        CreateMap<PaymentOrderEntity, PaymentOrderOutputModel>()
            .ForCtorParam(nameof(PaymentOrderOutputModel.Fees), o => o.MapFrom(s => (decimal?)s.TotalFees))
            .ForCtorParam(nameof(PaymentOrderOutputModel.Tax), o => o.MapFrom(s => (decimal?)s.TotalFeesTax))
            .ForCtorParam(nameof(PaymentOrderOutputModel.Total), o => o.MapFrom(s => s.TotalAmount))
            .ForCtorParam(nameof(PaymentOrderOutputModel.Currency), o=> o.MapFrom(s => s.Currency!.Abbreviation));

        CreateMap<PaymentOrderItemEntity, PaymentOrderItemOutputModel>()
            .ForCtorParam(nameof(PaymentOrderItemOutputModel.Fees), o => o.MapFrom(s => (decimal?)s.TotalFees))
            .ForCtorParam(nameof(PaymentOrderItemOutputModel.Tax), o => o.MapFrom(s => (decimal?)s.TotalFeesTax))
            .ForCtorParam(nameof(PaymentOrderItemOutputModel.Total), o => o.MapFrom(s => s.TotalAmount))
            .ForCtorParam(nameof(PaymentOrderItemOutputModel.PaymentOrder), o => o.MapFrom(s => s.PaymentOrder))
            .ForCtorParam(nameof(PaymentOrderItemOutputModel.BillingRefForDisplay), o => o.MapFrom(src => src.BillingRefValue.Contains('-') ? src.BillingRefValue.Substring(src.BillingRefValue.LastIndexOf('-') + 1) : src.BillingRefValue));
        CreateMap<PaymentOrderItemEntity, PaymentOrderItemExcelModel>()
            .ForCtorParam(nameof(PaymentOrderItemExcelModel.Fees), o => o.MapFrom(s => (decimal?)s.TotalFees))
            .ForCtorParam(nameof(PaymentOrderItemExcelModel.Tax), o => o.MapFrom(s => (decimal?)s.TotalFeesTax))
            .ForCtorParam(nameof(PaymentOrderItemExcelModel.Total), o => o.MapFrom(s => s.TotalAmount))
            .ForCtorParam(nameof(PaymentOrderItemExcelModel.Description), o => o.MapFrom(s => s.PaymentOrder!.Description))
            .ForCtorParam(nameof(PaymentOrderItemExcelModel.BillingRefForDisplay), o => o.MapFrom(src => src.BillingRefValue.Contains('-') ? src.BillingRefValue.Substring(src.BillingRefValue.LastIndexOf('-') + 1) : src.BillingRefValue));
    }
}
