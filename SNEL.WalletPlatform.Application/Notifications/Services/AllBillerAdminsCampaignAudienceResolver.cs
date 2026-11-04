using SNEL.WalletPlatform.Application.Common.Security;
using SNEL.WalletPlatform.Application.Interfaces.UnitOfWork;
using SNEL.WalletPlatform.Application.Notifications;
using SNEL.WalletPlatform.Application.Notifications.Interfaces;
using SNEL.WalletPlatform.Application.Notifications.Models.Internal;

namespace SNEL.WalletPlatform.Application.Notifications.Services;

/// <summary>
/// Audience resolver that targets active users belonging to biller partners.
/// </summary>
public sealed class AllBillerAdminsCampaignAudienceResolver(IUnitOfWork unitOfWork)
    : CampaignAudienceResolver
{
    public override string AudienceGroupCode => CampaignAudienceGroupCodes.AllBillerAdmins;

    public override string NameEn => "All biller admins";

    public override string NameFr => "Tous les administrateurs biller";

    public override async Task<CampaignAudienceBatch> GetActiveBatchAsync(
        Guid? afterCursorId,
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        var users = await unitOfWork.UserRepository.GetActiveBatchByPartnerTypeForNotificationsAsync(
            PartnerTypeConstants.Biller,
            afterCursorId,
            batchSize,
            cancellationToken);

        if (users.Count == 0)
        {
            return new CampaignAudienceBatch([], null, 0);
        }

        return new CampaignAudienceBatch(users, users[^1].Id, users.Count);
    }
}
