using SNEL.WalletPlatform.Domain.Common;
using SNEL.WalletPlatform.Domain.Customers;

namespace SNEL.WalletPlatform.Domain.Users;

public sealed class UserNotificationPreferenceEntity : TrackedEntity<Guid>
{
    private UserNotificationPreferenceEntity() { }

    private UserNotificationPreferenceEntity(NotificationChannel channel, bool active)
    {
        NotificationChannel = channel;
        Active = active;
    }

    public Guid UserId { get; private set; }
    public UserEntity User { get; private set; } = null!;
    public NotificationChannel NotificationChannel { get; private set; }
    public bool Active { get; private set; }

    public static UserNotificationPreferenceEntity Create(NotificationChannel channel, bool isActive = true)
    {
        return new UserNotificationPreferenceEntity(channel, isActive);
    }

    public void SetActive(bool isActive)
    {
        Active = isActive;
    }
}
