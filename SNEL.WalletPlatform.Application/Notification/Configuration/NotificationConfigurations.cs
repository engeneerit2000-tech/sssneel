

namespace SNEL.WalletPlatform.Application.Notification.Configuration;

public sealed class NotificationConfigurations
{
    public const string SectionName = "NotificationSettings";

    public string BaseURL { get; set; }
    public bool SMSIsEnabled { get; set; }
    public bool EmailIsEnabled { get; set; }
    public bool PushIsEnabled { get; set; }
}
