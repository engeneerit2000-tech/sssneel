namespace SNEL.WalletPlatform.Application.Payments.Interfaces;

/// <summary>
/// Per-request EasyPay initiation data supplied by the init-topup API.
/// </summary>
public interface IEasypayPaymentInitContext
{
    string? FromNumber { get; set; }
    string? Email { get; set; }
    string? Description { get; set; }
    string? ReturnUrl { get; set; }
    bool SendPaymentLinkByEmail { get; set; }
}
