using SNEL.WalletPlatform.Application.Notification.Enums;
using SNEL.WalletPlatform.Domain.Customers;
using SNEL.WalletPlatform.Domain.Users;


namespace SNEL.WalletPlatform.Application.Notification.Models.Internal;

public sealed class NotificationDetailsModel
{
    /// <summary>
    /// If true, send the notification immediately, and return error if failed
    /// </summary>
    public bool IsTransactional { get; set; } = false;

    /// <summary>
    /// Represents the main category of the notification. </br>
    /// </summary>
    public NotificationTypeEnum NotificationType { get; set; }

    /// <summary>
    /// each notification code should have a template per channel per language </br>
    /// example, SELF_METER_READING_REGISTER, REQUEST_APPROVE
    /// </summary>
    public required NotificationCodeEnum NotificationCode { get; set; }

    /// <summary>
    /// contains parameters list per language </br>
    /// </summary>
    public List<ParameterDataModel>? Params { get; set; }

    /// <summary>
    /// nullable, if null will be inserted as empty guid
    /// </summary>
    public Guid? SenderId { get; set; }

    /// <summary>
    /// Entity type for deep-link navigation (e.g. PROFILE, SUBSCRIPTION, PAYMENT).
    /// </summary>
    public ReferenceTypeEnum? ReferenceType { get; set; }

    /// <summary>
    /// Entity identifier for deep-link navigation (e.g. profile id, subscription id, payment id).
    /// </summary>
    public string? ReferenceId { get; set; }

    /// <summary>
    /// if true >> force sending to all requested channels and ignore receiver active channels
    /// </summary>
    public bool OverrideReceiverChannelsPreferences { get; set; } = false;

    /// <summary>
    /// The requested channels to send the notification to. </br>
    /// </summary>
    public required List<NotificationChannel> RequestedChannels { get; set; }

    public List<UserEntity>? Receivers { get; set; }

    /// <summary>
    /// For any additional phone numbers to send (SMS) the notification to.</br>
    /// Used mainly for sending OTP to non existing users.</br>
    /// </summary>
    public List<string>? AdditionalPhoneNumbers { get; set; }
    
    public DateTime? SendingAt { get; set; }

    public bool SaveInAppNotification { get; set; } = true;

    public PushMessageMode PushMessageMode { get; set; } = PushMessageMode.Notification;

    public Dictionary<string, string>? PushData { get; set; }
}

public class ParameterDataModel
{
    public Common.Enums.LanguageEnum Language { get; set; }

    /// <summary>
    /// the params should be in the same order as the template paramters </br>
    /// </summary>
    public List<string> Params { get; set; }
}
