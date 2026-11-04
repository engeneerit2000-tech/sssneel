using SNEL.WalletPlatform.Application.Interfaces.UnitOfWork;
using SNEL.WalletPlatform.Application.Notifications;
using SNEL.WalletPlatform.Application.Notifications.Interfaces;
using SNEL.WalletPlatform.Application.Notifications.Models.Internal;
using SNEL.WalletPlatform.Domain.Users;

namespace SNEL.WalletPlatform.Application.Notifications.Services;

/// <summary>
/// Queryable campaign audience. Criteria are hard-coded for now and will be expanded later.
/// Current temporary rule: active customers with more than one subscription.
/// </summary>
public sealed class QueryableCampaignAudienceResolver(IUnitOfWork unitOfWork)
    : CampaignAudienceResolver
{
    public override string AudienceGroupCode => CampaignAudienceGroupCodes.Queryable;

    public override string NameEn => "Queryable";

    public override string NameFr => "Requête";

    public override async Task<CampaignAudienceBatch> GetActiveBatchAsync(
        Guid? afterCursorId,
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        // Temporary hard-coded query for experimentation.
        // Future: drive this from stored/queryable campaign criteria.
        var customers = await unitOfWork.CustomerRepository
            .GetActiveBatchWithMultipleSubscriptionsForNotificationsAsync(
                afterCursorId,
                batchSize,
                cancellationToken);

        if (customers.Count == 0)
        {
            return new CampaignAudienceBatch([], null, 0);
        }

        var partnerIds = customers
            .Select(c => c.PartnerId)
            .Distinct()
            .ToList();

        var users = (await unitOfWork.UserRepository.GetByPartnerIdsWithNotificationsDependenciesAsync(partnerIds))
            .Where(u => u.Status == UserStatus.Active)
            .ToList();

        return new CampaignAudienceBatch(
            users,
            customers[^1].Id,
            customers.Count);
    }
}
