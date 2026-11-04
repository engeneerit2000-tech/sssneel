
using SNEL.WalletPlatform.Application.Common.Enums;
using SNEL.WalletPlatform.Application.Notification.Enums;
using SNEL.WalletPlatform.Domain.Customers;
using SNEL.WalletPlatform.Domain.Users;
using System.ComponentModel.DataAnnotations;

namespace SNEL.WalletPlatform.Application.Notification.Models.Internal;


public class SendNotificationInternalAPIInputModel
{
    [MinLength(1)]
    public required List<NotificationDataModel> Notifications { get; set; }
}

public class NotificationDataModel
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
    public List<ParameterInputModel>? Parameters { get; set; }

    /// <summary>
    /// nullable, if null will be inserted as empty guid
    /// </summary>
    public Guid? SenderId { get; set; }

    /// <summary>
    /// 1 = request, 2 = complaint, 3 = profile, 4 = subscription
    /// </summary>
    public ReferenceTypeEnum? ReferenceType { get; set; }

    /// <summary>
    /// The identifier for the reference. Example, request Id or complaint Id </br>
    /// </summary>
    public string? ReferenceId { get; set; }

    /// <summary>
    /// if true >> force sending to all requested channels and ignore receiver active channels
    /// </summary>
    public bool OverrideChannelsPreferences { get; set; } = false;


    /// <summary>
    /// The requested channels to send the notification to. </br>
    /// </summary>
    public required List<NotificationChannel> RequestedChannels { get; set; }


    /// <summary>
    /// At least on receiver should be added. </br>
    /// </summary>
    [MinLength(1)]
    public required List<NotificationReceiverInputModel> Receivers { get; set; }
    
    public DateTime? SendingAt { get; set; }

    public bool SaveInAppNotification { get; set; } = true;

    public PushMessageMode PushMessageMode { get; set; } = PushMessageMode.Notification;

    public Dictionary<string, string>? PushData { get; set; }
}

public class ParameterInputModel
{
    public LanguageEnum Language { get; set; }

    /// <summary>
    /// the params should be in the same order as the template paramters </br>
    /// </summary>
    public List<string> Params { get; set; }

}


public class NotificationReceiverInputModel
{
    /// <summary>
    /// Receiver ID. usually guid
    /// </summary>
    public Guid? Id { get; set; }

    public ReceiverPreferencesInputModel? Preferences { get; set; }

    public PushNotificationDetailsInputModel? PushNotificationDetails { get; set; }
    public SMSNotificationDetailsInputModel? SMSNotificationDetails { get; set; }
    public EmailNotificationDetailsInputModel? EmailNotificationDetails { get; set; }
}

public class ReceiverPreferencesInputModel
{
    public LanguageEnum Language { get; set; } = LanguageEnum.FR;
    public List<NotificationChannel>? ActiveChannels { get; set; }
}

public class PushNotificationDetailsInputModel
{
    public required List<PushNotificationToken> Tokens { get; set; }
}

public class PushNotificationToken
{
    public required string TokenId { get; set; }
    public required PlatformType PlatformId { get; set; }
}


public class SMSNotificationDetailsInputModel
{
    public required List<string> PhoneNumbers { get; set; }
}

public class EmailNotificationDetailsInputModel
{
    public required List<string> Emails { get; set; }
}

