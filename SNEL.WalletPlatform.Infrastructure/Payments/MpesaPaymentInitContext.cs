using SNEL.WalletPlatform.Application.Payments.Interfaces;

namespace SNEL.WalletPlatform.Infrastructure.Payments;

public sealed class MpesaPaymentInitContext : IMpesaPaymentInitContext
{
    public string? FromNumber { get; set; }
}
