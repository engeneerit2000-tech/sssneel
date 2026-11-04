namespace SNEL.WalletPlatform.Application.Payments.Interfaces;

/// <summary>
/// Per-request mobile number for AkibaPay customer deposit (from init-topup API body).
/// </summary>
public interface IAkibaPayPaymentInitContext
{
    string? FromNumber { get; set; }
}
