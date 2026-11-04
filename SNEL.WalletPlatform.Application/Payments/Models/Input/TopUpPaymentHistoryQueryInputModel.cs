using SNEL.WalletPlatform.Application.Common.Models.Input;

namespace SNEL.WalletPlatform.Application.Payments.Models.Input;

public sealed class TopUpPaymentHistoryQueryInputModel : BaseQueryInputModel
{
    public int? StatusId { get; set; } = null;
    public bool? OnlyActive { get; set; }
    public string? PaymentProviderCode { get; set; }
    public string? PaymentMethodCode { get; set; }    
}
