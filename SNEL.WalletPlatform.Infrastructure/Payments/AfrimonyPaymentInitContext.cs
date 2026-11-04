using SNEL.WalletPlatform.Application.Payments.Interfaces;

namespace SNEL.WalletPlatform.Infrastructure.Payments;

public sealed class AfrimonyPaymentInitContext : IAfrimonyPaymentInitContext
{
    public string? FromNumber { get; set; }
}
