namespace SNEL.WalletPlatform.Application.Payments.Interfaces;

/// <summary>
/// Per-request mobile number for Afrimony merchant pay (from init-topup API body).
/// </summary>
public interface IAfrimonyPaymentInitContext
{
    string? FromNumber { get; set; }
}
