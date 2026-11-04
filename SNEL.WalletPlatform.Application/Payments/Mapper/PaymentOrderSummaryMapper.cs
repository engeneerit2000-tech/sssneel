using AutoMapper;
using SNEL.WalletPlatform.Application.Payments.Models.Output;
using SNEL.WalletPlatform.Domain.Payments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNEL.WalletPlatform.Application.Payments.Mapper
{
    public class PaymentOrderSummaryMapper : Profile
    {
        public PaymentOrderSummaryMapper()
        {
            CreateMap<PaymentOrderItemEntity, PaymentOrderItemSummaryOutputModel>()
                .ForCtorParam(nameof(PaymentOrderItemSummaryOutputModel.Id),
                    opt => opt.MapFrom(src => src.Id))
                .ForCtorParam(nameof(PaymentOrderItemSummaryOutputModel.ItemType),
                    opt => opt.MapFrom(src => (int)src.ItemType))
                .ForCtorParam(nameof(PaymentOrderItemSummaryOutputModel.Amount),
                    opt => opt.MapFrom(src => src.Amount))
                .ForCtorParam(nameof(PaymentOrderItemSummaryOutputModel.Fees),
                    opt => opt.MapFrom(src => src.TotalFees))
                .ForCtorParam(nameof(PaymentOrderItemSummaryOutputModel.Tax),
                    opt => opt.MapFrom(src => src.TotalFeesTax))
                .ForCtorParam(nameof(PaymentOrderItemSummaryOutputModel.TotalFees),
                    opt => opt.MapFrom(src => src.TotalFees))
                .ForCtorParam(nameof(PaymentOrderItemSummaryOutputModel.TotalFeesTax),
                    opt => opt.MapFrom(src => src.TotalFeesTax))
                .ForCtorParam(nameof(PaymentOrderItemSummaryOutputModel.TotalCost),
                    opt => opt.MapFrom(src => src.TotalCost))
                .ForCtorParam(nameof(PaymentOrderItemSummaryOutputModel.TotalCostTax),
                    opt => opt.MapFrom(src => src.TotalCostTax))
                .ForCtorParam(nameof(PaymentOrderItemSummaryOutputModel.TotalCommission),
                    opt => opt.MapFrom(src => src.TotalCommission))
                .ForCtorParam(nameof(PaymentOrderItemSummaryOutputModel.TotalCommissionTax),
                    opt => opt.MapFrom(src => src.TotalCommissionTax))
                .ForCtorParam(nameof(PaymentOrderItemSummaryOutputModel.TotalAmount),
                    opt => opt.MapFrom(src => src.TotalAmount))
                .ForCtorParam(nameof(PaymentOrderItemSummaryOutputModel.State),
                    opt => opt.MapFrom(src => (int)src.Status))
                .ForCtorParam(nameof(PaymentOrderItemSummaryOutputModel.BillingRefValue), opt => opt.MapFrom(src => src.BillingRefValue))
                .ForCtorParam(nameof(PaymentOrderItemSummaryOutputModel.BillingRefForDisplay), opt => opt.MapFrom(src => src.BillingRefValue.Contains('-') ? src.BillingRefValue.Substring(src.BillingRefValue.LastIndexOf('-') + 1) : src.BillingRefValue))
                .ForCtorParam(nameof(PaymentOrderItemSummaryOutputModel.BillingServiceName), opt => opt.MapFrom(src => src.BillingService != null ? src.BillingService.Name : null))
                .ForCtorParam(nameof(PaymentOrderItemSummaryOutputModel.BillingServiceCode), opt => opt.MapFrom(src => src.BillingService != null ? src.BillingService.Code : null))
                .ForCtorParam(nameof(PaymentOrderItemSummaryOutputModel.CreatedAtUtc),
                    opt => opt.MapFrom(src => src.CreatedAtUtc))
                .ForCtorParam(nameof(PaymentOrderItemSummaryOutputModel.ExternalRef),
                    opt => opt.MapFrom(src => src.ExternalRef ?? string.Empty))
                .ForCtorParam(nameof(PaymentOrderItemSummaryOutputModel.InvoiceDate),
                    opt => opt.MapFrom(_ => (DateTimeOffset?)null))
                .ForCtorParam(nameof(PaymentOrderItemSummaryOutputModel.InvoiceRef),
                    opt => opt.MapFrom(_ => (string?)null));

            CreateMap<PaymentOrderEntity, PaymentOrderSummaryOutputModel>()
                .ForCtorParam(nameof(PaymentOrderSummaryOutputModel.Status), opt => opt.MapFrom(src => (int)src.Status))
                .ForCtorParam(nameof(PaymentOrderSummaryOutputModel.CurrencyCode), opt => opt.MapFrom(src => src.Currency != null ? (src.Currency.Abbreviation ?? src.Currency.Code) : null));
        }
    }
}
