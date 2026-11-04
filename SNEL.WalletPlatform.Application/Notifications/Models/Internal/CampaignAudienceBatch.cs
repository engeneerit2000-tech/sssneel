using SNEL.WalletPlatform.Domain.Users;

namespace SNEL.WalletPlatform.Application.Notifications.Models.Internal;

public sealed record CampaignAudienceBatch(
    IReadOnlyList<UserEntity> Users,
    Guid? ProgressCursorId,
    int SourceCount);
