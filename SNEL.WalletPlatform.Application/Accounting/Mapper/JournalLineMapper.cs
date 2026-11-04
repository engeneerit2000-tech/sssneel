using AutoMapper;
using SNEL.WalletPlatform.Application.Accounting.Models.Input;
using SNEL.WalletPlatform.Application.Accounting.Models.Output;
using SNEL.WalletPlatform.Domain.Accounting;

namespace SNEL.WalletPlatform.Application.Accounting.Mapper;

public sealed class JournalLineMapper : Profile
{
    public JournalLineMapper()
    {
        CreateMap<JournalLineEntity, JournalLineOutputModel>();
        CreateMap<PlatformInvoiceEntity, PlatformInvoiceOutputModel>();
        CreateMap<JournalLineEntity, JournalLineStatementOutputModel>()
            .ForMember(dest => dest.JournalDescription, opt => opt.MapFrom(src => src.Journal.Description))
            .ForMember(dest => dest.JournalSourceRef, opt => opt.MapFrom(src => src.Journal.SourceRef))
            .ForMember(dest => dest.JournalSourceTypeId, opt => opt.MapFrom(src => src.Journal.SourceTypeId));
        CreateMap<JournalLineEntity, JournalLineStatementExcelModel>()
            .ForMember(dest => dest.JournalDescription, opt => opt.MapFrom(src => src.Journal.Description))
            .ForMember(dest => dest.JournalSourceRef, opt => opt.MapFrom(src => src.Journal.SourceRef))
            .ForMember(dest => dest.JournalLineTypeName, opt => opt.MapFrom(src => src.JournalLineType.Name));
        CreateMap<JournalLinesStatementInputModel, JournalLinesStatementInternalModel>();
        CreateMap<JournalLineTypeEntity, JournalLineTypeOutputModel>();
    }
}
