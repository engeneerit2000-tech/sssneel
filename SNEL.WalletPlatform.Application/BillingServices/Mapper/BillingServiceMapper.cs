using AutoMapper;
using SNEL.WalletPlatform.Application.BillingServices.Models.Output;
using SNEL.WalletPlatform.Application.Invoicing.Models;
using SNEL.WalletPlatform.Domain.Billing;
using SNEL.WalletPlatform.Domain.Invoicing;
using SNEL.WalletPlatform.Domain.Lookups;

namespace SNEL.WalletPlatform.Application.BillingServices.Mapper;

public sealed class BillingServiceMapper : Profile
{
    public BillingServiceMapper()
    {
        CreateMap<InvoiceEntity, InquireBillOutputModel>()
            .ForCtorParam("invoiceId", opt => opt.MapFrom(src => src.Id))
            .ForCtorParam("invoiceRef", opt => opt.MapFrom(src => src.InvoiceRef))
            .ForCtorParam("amountTotal", opt => opt.MapFrom(src => src.AmountTotal))
            .ForCtorParam("amountPaid", opt => opt.MapFrom(src => src.AmountPaid))
            .ForCtorParam("amountOutstanding", opt => opt.MapFrom(src => src.AmountOutstanding))
            .ForCtorParam("currencyCode", opt => opt.MapFrom(src => src.Currency.Code))
            .ForCtorParam("status", opt => opt.MapFrom(src => src.Status))
            .ForCtorParam("statusId", opt => opt.MapFrom(src => Enum.Parse(typeof(InvoiceStatus), src.Status)))
            .ForCtorParam("issueDate", opt => opt.MapFrom(src => src.IssueDate))
            .ForCtorParam("dueDate", opt => opt.MapFrom(src => src.DueDate))
            .ForCtorParam("customerName", opt => opt.MapFrom(src => ResolveOracleCustomerName(src.InvoiceInfo)))
            .ForCtorParam("invoiceInfo", opt => opt.MapFrom(src => src.InvoiceInfo));

        CreateMap<BillingServiceEntity, BillingServiceOutputModel>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.Active))
            //.ForMember(dest => dest.Name, opt => opt.MapFrom((src, dest, destMember, context) =>
            //{
            //    var languageCode = context.Items["LanguageCode"] as string;
            //    if (string.IsNullOrWhiteSpace(languageCode)) languageCode = "en";
            //    languageCode = languageCode.ToLowerInvariant();
            //    return src.Translations.FirstOrDefault(t => t.LanguageCode== languageCode)?.Name
            //           ?? src.Name;
            //}));
            ;
        
        CreateMap<BillingServiceCurrencyEntity, BillingServiceCurrencyOutputModel>();
        CreateMap<BillingCategoryEntity, BillingCategoryOutputModel>();
        CreateMap<BillingServiceTypeEntity, BillingServiceTypeOutputModel>();
    }

    private static string? ResolveOracleCustomerName(string? invoiceInfo) =>
        InvoiceInfoModel.Parse(invoiceInfo)?.CustomerName;
}
