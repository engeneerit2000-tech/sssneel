namespace SNEL.WalletPlatform.Application.Payments.Interfaces;

public interface IMpesaPaymentInitContext
{
    string? FromNumber { get; set; }
}
