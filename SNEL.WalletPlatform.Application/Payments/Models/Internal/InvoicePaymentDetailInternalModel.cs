using SNEL.WalletPlatform.Domain.Billing;
using SNEL.WalletPlatform.Domain.Lookups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNEL.WalletPlatform.Application.Payments.Models.Internal
{
    public sealed record InvoicePaymentDetailInternalModel(
        InvoiceEntity Invoice,
        CurrencyEntity Currency,
        BillingServiceCurrencyEntity BillingServiceCurrency,
        BillingChargesRecord ChargesRecord,
        decimal Amount);
}
