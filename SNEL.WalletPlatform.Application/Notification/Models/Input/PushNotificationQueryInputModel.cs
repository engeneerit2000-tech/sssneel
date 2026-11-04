using SNEL.WalletPlatform.Application.Common.Models.Input;

namespace SNEL.WalletPlatform.Application.Notification.Models.Input;

public class PushNotificationQueryInputModel : BaseQueryInputModel
{

    // filters
    public int? LanguageId { get; set; }
    public bool? IsRead { get; set; }
}
