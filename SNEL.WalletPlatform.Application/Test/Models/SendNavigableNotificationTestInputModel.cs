using System.ComponentModel.DataAnnotations;
using SNEL.WalletPlatform.Application.Notification.Enums;

namespace SNEL.WalletPlatform.Application.Test.Models;

/// <summary>
/// Body for sending a visible (navigable) test push notification to a user.
/// </summary>
public sealed class SendNavigableNotificationTestInputModel
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public NotificationCodeEnum NotificationCode { get; set; }

    [Required]
    public NotificationTypeEnum NotificationType { get; set; }

    [Required]
    public ReferenceTypeEnum ReferenceType { get; set; }

    [Required]
    public string ReferenceId { get; set; } = string.Empty;

    /// <summary>
    /// Optional template placeholder values (ordered). Defaults to empty placeholders when omitted.
    /// </summary>
    public List<string>? Parameters { get; set; }

    public bool IsTransactional { get; set; } = true;

    /// <summary>
    /// When true, also request SMS in addition to Push.
    /// </summary>
    public bool IncludeSms { get; set; } = false;
}
