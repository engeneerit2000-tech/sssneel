using AutoMapper;
using SNEL.WalletPlatform.Application.Payments.Models.Output;
using SNEL.WalletPlatform.Domain.Billing;
using SNEL.WalletPlatform.Domain.Payments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNEL.WalletPlatform.Application.Payments.Mapper
{
    public class InvoiceSummaryMapperMapper : Profile
    {
        public InvoiceSummaryMapperMapper()
        {
            CreateMap<InvoiceEntity, InvoiceSummaryOutputModel>()
                 .ConstructUsing(src => new InvoiceSummaryOutputModel(
                    src.Id,
                    src.InvoiceRef,
                    src.AmountOutstanding,
                    src.AmountTotal,
                    src.AmountPaid,
                    src.CurrencyId,
                    src.IssueDate,
                    src.DueDate,
                    src.Status
                ));             
        }
    }
}
