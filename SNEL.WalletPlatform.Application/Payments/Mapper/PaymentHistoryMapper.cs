using AutoMapper;
using Microsoft.Extensions.Localization;
using SNEL.WalletPlatform.Application.Common.Localization;
using SNEL.WalletPlatform.Application.Payments.Models.Output;
using SNEL.WalletPlatform.Domain.Payments;

namespace SNEL.WalletPlatform.Application.Payments.Mapper;

public sealed class PaymentHistoryMapper(IStringLocalizer<SharedResource> localizer, IMapper mapper) //: Profile
{
    public IReadOnlyList<PaymentWithOrderAndItemsOutputModel> Map(IReadOnlyList<PaymentEntity> src, string? languageCode = null)
    {
        return src.Select(p => Map(p, languageCode)).ToList();
    }
    public PaymentWithOrderAndItemsOutputModel Map(PaymentEntity src, string? languageCode = null)
    {
        languageCode = string.IsNullOrWhiteSpace(languageCode) ? "en" : languageCode.ToLowerInvariant();

        var orderDto = mapper.Map<PaymentOrderSummaryOutputModel>(src.PaymentOrder);
        var orderItems = src.PaymentOrder!.Items.ToList();
        var itemDtos = PaymentOrderItemSummaryListMapper.MapItems(mapper, orderItems, src.Direction);
        bool canComplete = (src.Status == PaymentStatus.Pending || src.Status == PaymentStatus.Processing);
        bool canApply = src.Status == PaymentStatus.Completed && orderItems.Any(x => x.Status == PaymentOrderItemStatus.Paid);
        bool canCancel = PaymentStatusExtensions.ExpirableStatuses.Contains(src.Status);
        bool canReconcile = src.Status == PaymentStatus.Completed;

        if (src.Direction == PaymentDirectionEnum.OUT)
        {
            for (var i = 0; i < itemDtos.Count && i < orderItems.Count; i++)
            {
                var invoice = orderItems[i].Invoice;
                if (invoice is null)
                {
                    continue;
                }

                itemDtos[i] = itemDtos[i] with
                {
                    InvoiceDate = invoice.IssueDate,
                    InvoiceRef = string.IsNullOrWhiteSpace(invoice.InvoiceRef) ? null : invoice.InvoiceRef
                };
            }
        }

        var description = BuildDescription(src, languageCode);

        return new PaymentWithOrderAndItemsOutputModel(
            Id: src.Id,
            PaymentOrderId: src.PaymentOrderId,
            Amount: src.Amount,
            Tax: src.Tax,
            Total: src.Total,
            ExternalRef: src.ExternalRef,
            PaymentRef: src.PaymentRef,
            CreatedAtUtc: src.CreatedAtUtc,
            Status: (int)src.Status,
            CurrencyCode: src.Currency != null ? (src.Currency.Abbreviation ?? src.Currency.Code) : null,
            PaymentMethodCode: src.PaymentMethod?.Code,
            PaymentMethodName: src.PaymentMethod?.Name,
            PaymentOrder: orderDto with { Description = description},
            Items: itemDtos,
            Description: description,
            CanComplete: canComplete,
            CanApply: canApply,
            CanCancel: canCancel,
            CanReconcile: canReconcile
        );
    }

    private string BuildDescription(PaymentEntity src, string languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode)) languageCode = "en";
        languageCode = languageCode.ToLowerInvariant();
        if (src.PaymentOrder!.Items.Count == 0)
        {
            throw new InvalidOperationException(localizer[LocalizationKeys.PaymentHistoryOrderHasNoItems, src.Id]);
        }

        OrderItemType itemType = src.PaymentOrder.Items.First().ItemType;
        switch (itemType)
        {
            case OrderItemType.Topup:
                return localizer[LocalizationKeys.PaymentOrderKeys.DescriptionTopupBref];//, src.PaymentMethod!.PaymentProvider!.Name, src.PaymentMethod!.Name];
            case OrderItemType.PayBill:
                {
                    //var poi = src.PaymentOrder.Items.First();
                    //string billingServiceName = poi.BillingService!.Translations.FirstOrDefault(t => t.LanguageCode == languageCode)?.Name ?? poi.BillingService.Name;
                    return localizer[LocalizationKeys.PaymentOrderKeys.DescriptionPayBillBref];//, billingServiceName, poi.BillingRefValue!];
                }
            case OrderItemType.MeterCharge:
                {
                    //var poi = src.PaymentOrder.Items.First();
                    //string billingServiceName = poi.BillingService!.Translations.FirstOrDefault(t => t.LanguageCode == languageCode)?.Name ?? poi.BillingService.Name;
                    return localizer[LocalizationKeys.PaymentOrderKeys.DescriptionMeterChargeBref];//, billingServiceName, poi.BillingRefValue!];
                }
            default:
                throw new InvalidOperationException(localizer[LocalizationKeys.PaymentHistoryUnsupportedItemType, itemType, src.Id]);
        }
    }

    public IReadOnlyList<TopupExcelOutputModel> MapTopupExcel(IReadOnlyList<PaymentEntity> src, string? languageCode = null)
    {
        return src.Select(p => MapTopupExcel(p, languageCode)).ToList();
    }
    public TopupExcelOutputModel MapTopupExcel(PaymentEntity src, string? languageCode = null)
    {
        languageCode = string.IsNullOrWhiteSpace(languageCode) ? "en" : languageCode.ToLowerInvariant();
        var orderDto = mapper.Map<PaymentOrderSummaryOutputModel>(src.PaymentOrder);
        var description = BuildDescription(src, languageCode);

        return new TopupExcelOutputModel(
            Description: description,
            Status: src.Status.ToString(),
            PaymentProviderCode: src.PaymentMethod?.PaymentProvider?.Code,
            PaymentProviderName: src.PaymentMethod?.PaymentProvider?.Name,
            PaymentMethodCode: src.PaymentMethod?.Code,
            PaymentMethodName: src.PaymentMethod?.Name, 
            ExternalRef: src.ExternalRef,
            PaymentRef: src.PaymentRef,
            Amount: src.Amount,
            TotalFees: orderDto.TotalFees,
            TotalFeesTax: orderDto.TotalFeesTax,
            TotalCost: orderDto.TotalCost,
            TotalCostTax: orderDto.TotalCostTax,
            TotalCommission: orderDto.TotalCommission,
            TotalCommissionTax: orderDto.TotalCommissionTax,
            TotalAmount: orderDto.TotalAmount,
            CurrencyCode: src.Currency != null ? (src.Currency.Abbreviation ?? src.Currency.Code) : null,
            CreatedAtUtc: src.CreatedAtUtc
        );
    }

    public IReadOnlyList<PaymentExcelOutputModel> MapPaymentExcel(IReadOnlyList<PaymentEntity> src, string? languageCode = null)
    {
        return src.Select(p => MapPaymentExcel(p, languageCode)).ToList();
    }

    public PaymentExcelOutputModel MapPaymentExcel(PaymentEntity src, string? languageCode = null)
    {
        languageCode = string.IsNullOrWhiteSpace(languageCode) ? "en" : languageCode.ToLowerInvariant();
        var orderDto = mapper.Map<PaymentOrderSummaryOutputModel>(src.PaymentOrder);
        var description = BuildDescription(src, languageCode);

        return new PaymentExcelOutputModel(
            Description: description,
            Status: src.Status.ToString(),
            ExternalRef: src.ExternalRef,
            PaymentRef: src.PaymentRef,
            Amount: src.Amount,
            TotalFees: orderDto.TotalFees,
            TotalFeesTax: orderDto.TotalFeesTax,
            TotalCost: orderDto.TotalCost,
            TotalCostTax: orderDto.TotalCostTax,
            TotalCommission: orderDto.TotalCommission,
            TotalCommissionTax: orderDto.TotalCommissionTax,
            TotalAmount: orderDto.TotalAmount,
            CurrencyCode: src.Currency != null ? (src.Currency.Abbreviation ?? src.Currency.Code) : null,
            CreatedAtUtc: src.CreatedAtUtc
        );
    }
}

////CreateMap<PaymentEntity, PaymentWithOrderAndItemsOutputModel>()
////            .ForCtorParam(nameof(PaymentWithOrderAndItemsOutputModel.Status), opt => opt.MapFrom(src => (int)src.Status))
////            .ForCtorParam(nameof(PaymentWithOrderAndItemsOutputModel.CurrencyCode), opt => opt.MapFrom(src => src.Currency != null ? (src.Currency.Abbreviation ?? src.Currency.Code) : null))
////            .ForCtorParam(nameof(PaymentWithOrderAndItemsOutputModel.PaymentMethodCode), opt => opt.MapFrom(src => src.PaymentMethod != null ? src.PaymentMethod.Code : null))
////            .ForCtorParam(nameof(PaymentWithOrderAndItemsOutputModel.PaymentMethodName), opt => opt.MapFrom(src => src.PaymentMethod != null ? src.PaymentMethod.Name : null))
////            .ForCtorParam(nameof(PaymentWithOrderAndItemsOutputModel.PaymentOrder), opt => opt.MapFrom(src => src.PaymentOrder))
////            .ForCtorParam(nameof(PaymentWithOrderAndItemsOutputModel.Items), opt => opt.MapFrom(src => src.PaymentOrder != null ? src.PaymentOrder.Items : Enumerable.Empty<PaymentOrderItemEntity>()))
////            .ForCtorParam(nameof(PaymentWithOrderAndItemsOutputModel.Description), opt => opt.MapFrom((src, context) =>
////            {
                
////            }))
////            ;
//    }
//}
