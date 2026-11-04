using SNEL.WalletPlatform.Application.Payments.Interfaces;

namespace SNEL.WalletPlatform.Infrastructure.Payments;

public sealed class OrangePaymentInitContext : IOrangePaymentInitContext
{
    public string? FromNumber { get; set; }
}
