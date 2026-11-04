using SNEL.WalletPlatform.Application.Common.Models.Input;

namespace SNEL.WalletPlatform.Application.Payments.Models.Internal;

public sealed class TopUpPaymentHistoryQueryInternalModel : BaseQueryInputModel
{
    public Guid CustomerId { get; set; }
    public int? StatusId { get; set; } = null;
    public bool OnlyActive { get; set; } = true;
    public string? PaymentProviderCode { get; set; }
    public string? PaymentMethodCode { get; set; }
}
