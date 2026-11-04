using SNEL.WalletPlatform.Application.Common.Security;
using SNEL.WalletPlatform.Application.Interfaces.UnitOfWork;
using SNEL.WalletPlatform.Application.Notifications;
using SNEL.WalletPlatform.Application.Notifications.Interfaces;
using SNEL.WalletPlatform.Application.Notifications.Models.Internal;

namespace SNEL.WalletPlatform.Application.Notifications.Services;

/// <summary>
/// Audience resolver that targets active users belonging to PSP partners.
/// </summary>
public sealed class AllPspAdminsCampaignAudienceResolver(IUnitOfWork unitOfWork)
    : CampaignAudienceResolver
{
    public override string AudienceGroupCode => CampaignAudienceGroupCodes.AllPspAdmins;

    public override string NameEn => "All PSP admins";

    public override string NameFr => "Tous les administrateurs PSP";

    public override async Task<CampaignAudienceBatch> GetActiveBatchAsync(
        Guid? afterCursorId,
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        var users = await unitOfWork.UserRepository.GetActiveBatchByPartnerTypeForNotificationsAsync(
            PartnerTypeConstants.Psp,
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
