using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Notification.Models.Internal;
using SNEL.WalletPlatform.Application.Notification.Models.Output;
using SNEL.WalletPlatform.Domain.Customers;

namespace SNEL.WalletPlatform.Application.Notification.Interfaces;

public interface INotificationManager
{
    Task<Result<bool>> SendNotifictionAsync(SendNotificationInternalAPIInputModel model);

    Task<Result<bool>> ReadNotificationAsync(Guid notificaitonId, Guid userId);

    Task<Result<PagedResult<NotificationMessageInternalAPIOutputModel>>> GetNotificationMessagesAsync(GetNotificationsMessagesInternalAPIInputModel model);

    // receiver meta data end points
    Task<Result<int>> GetNewNotificationsCountAsync(Guid userId, NotificationChannel channel);
    Task<Result<bool>> ResetNewNotificationsCountAsync(Guid userId, NotificationChannel channel);

    Task<Result<bool>> CreateCloudMessagingTokenAsync(Guid receiverId, string tokenId, int? platformId, string? deviceName);

    Task<Result<bool>> DeleteCloudMessagingTokenAsync(Guid receiverId, string tokenId);
}
