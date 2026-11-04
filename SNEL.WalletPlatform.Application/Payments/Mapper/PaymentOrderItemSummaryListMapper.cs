using AutoMapper;
using SNEL.WalletPlatform.Application.Payments.Models.Output;
using SNEL.WalletPlatform.Domain.Payments;

namespace SNEL.WalletPlatform.Application.Payments.Mapper;

public static class PaymentOrderItemSummaryListMapper
{
    public static List<PaymentOrderItemSummaryOutputModel> MapItems(
        IMapper mapper,
        IEnumerable<PaymentOrderItemEntity> items,
        PaymentDirectionEnum? direction)
    {
        var list = items.ToList();
        var dtos = mapper.Map<List<PaymentOrderItemSummaryOutputModel>>(list);
        for (var i = 0; i < dtos.Count; i++)
        {
            dtos[i] = ApplyDirectionalFeesAndTax(dtos[i], list[i], direction);
        }

        return dtos;
    }

    /// <summary>
    /// IN (top-up): <see cref="PaymentOrderItemSummaryOutputModel.Fees"/>/<c>Tax</c> mirror total fees and fees tax.
    /// OUT (bills / meter): they mirror total cost and cost tax.
    /// </summary>
    public static PaymentOrderItemSummaryOutputModel ApplyDirectionalFeesAndTax(
        PaymentOrderItemSummaryOutputModel item,
        PaymentOrderItemEntity entity,
        PaymentDirectionEnum? direction)
    {
        if (direction != PaymentDirectionEnum.OUT)
        {
            return item;
        }

        return item with
        {
            Fees = entity.TotalCost,
            Tax = entity.TotalCostTax
        };
    }
}
