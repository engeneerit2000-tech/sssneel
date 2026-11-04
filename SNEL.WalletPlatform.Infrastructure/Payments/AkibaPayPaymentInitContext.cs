using SNEL.WalletPlatform.Application.Payments.Interfaces;

namespace SNEL.WalletPlatform.Infrastructure.Payments;

public sealed class AkibaPayPaymentInitContext : IAkibaPayPaymentInitContext
{
    public string? FromNumber { get; set; }
}
