using SNEL.WalletPlatform.Application.Common.Models.Input;

namespace SNEL.WalletPlatform.Application.Payments.Models.Internal;

public sealed class PaymentHistoryQueryInternalModel : BaseQueryInputModel
{
    public Guid CustomerId { get; set; }
    public int? StatusId { get; set; } = null;
    public bool OnlyActive { get; set; } = true;
    public string? BillerCode { get; set; }
    public string? BillingServiceCode { get; set; }
    public string? BillingRefValue { get; set; }
}
