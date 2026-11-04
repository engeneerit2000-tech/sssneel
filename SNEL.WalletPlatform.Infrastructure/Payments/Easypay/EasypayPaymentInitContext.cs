using SNEL.WalletPlatform.Application.Payments.Interfaces;

namespace SNEL.WalletPlatform.Infrastructure.Payments;

public sealed class EasypayPaymentInitContext : IEasypayPaymentInitContext
{
    public string? FromNumber { get; set; }
    public string? Email { get; set; }
    public string? Description { get; set; }
    public string? ReturnUrl { get; set; }
    public bool SendPaymentLinkByEmail { get; set; }
}
