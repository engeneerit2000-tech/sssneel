using SNEL.WalletPlatform.Application.Common.Models.Input;

namespace SNEL.WalletPlatform.Application.Payments.Models.Input;

public sealed class PaymentHistoryQueryInputModel : BaseQueryInputModel
{
    public int? StatusId { get; set; } = null;
    public bool? OnlyActive { get; set; }
    public string? BillerCode { get; set; }
    public string? BillingServiceCode { get; set; }
    public string? BillingRefValue { get; set; }
}
