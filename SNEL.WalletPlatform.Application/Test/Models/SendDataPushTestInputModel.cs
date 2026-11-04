using System.ComponentModel.DataAnnotations;
using SNEL.WalletPlatform.Application.Notification.Enums;

namespace SNEL.WalletPlatform.Application.Test.Models;

public sealed class SendDataPushTestInputModel
{
    public NotificationCodeEnum NotificationCode { get; set; } = NotificationCodeEnum.PAYMENT_TOPUP_SUCSESS;

    public NotificationTypeEnum NotificationType { get; set; } = NotificationTypeEnum.TOPUP;

    public bool IsTransactional { get; set; } = false;

    /// <summary>
    /// Silent push payload. All values must be strings.
    /// Example topupSuccess keys: eventType, paymentId, walletAccountId, amount, currency, balance.
    /// </summary>
    public Dictionary<string, string>? PushData { get; set; }

    /// <summary>
    /// Merged into pushData when provided. Shortcut values override keys already in pushData.
    /// </summary>
    public string? EventType { get; set; }

    public string? PaymentId { get; set; }

    public string? WalletAccountId { get; set; }

    public string? Amount { get; set; }

    public string? Currency { get; set; }

    public string? Balance { get; set; }
}
