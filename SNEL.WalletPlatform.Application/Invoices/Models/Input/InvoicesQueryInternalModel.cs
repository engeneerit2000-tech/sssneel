using SNEL.WalletPlatform.Application.Common.Models.Input;

namespace SNEL.WalletPlatform.Application.Invoices.Models.Input;

public sealed class InvoicesQueryInternalModel : BaseQueryInputModel
{
    public long BillingServiceId { get; set; }
    public string BillingRefValue { get; set; }
}