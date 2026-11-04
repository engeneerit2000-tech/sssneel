using AutoMapper;
using Microsoft.Extensions.Localization;
using SNEL.WalletPlatform.Application.Common.Localization;
using SNEL.WalletPlatform.Application.Payments.Models.Output;
using SNEL.WalletPlatform.Domain.Payments;

namespace SNEL.WalletPlatform.Application.Payments.Mapper;

public sealed class PaymentMapper(IMapper mapper) //: Profile
{
    public IReadOnlyList<PaymentOutputModel> Map(IReadOnlyList<PaymentEntity> src, string? languageCode = null)
    {
        return src.Select(p => Map(p)).ToList();
    }
    public IReadOnlyList<PaymentExcelModel> MapExcel(IReadOnlyList<PaymentEntity> src, string? languageCode = null)
    {
        return src.Select(p => MapExcel(p)).ToList();
    }
    public PaymentOutputModel Map(PaymentEntity src)
    {
        return new PaymentOutputModel(
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
            PaymentProviderCode: src.PaymentMethod.PaymentProvider.Code,
            PaymentProviderName: src.PaymentMethod.PaymentProvider.Name            
        );
    }

    public PaymentExcelModel MapExcel(PaymentEntity src)
    {
        return new PaymentExcelModel(
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
            PaymentProviderCode: src.PaymentMethod.PaymentProvider.Code,
            PaymentProviderName: src.PaymentMethod.PaymentProvider.Name
        );
    }
}