using SNEL.WalletPlatform.Application.Interfaces.UnitOfWork;
using SNEL.WalletPlatform.Application.Notifications;
using SNEL.WalletPlatform.Application.Notifications.Interfaces;
using SNEL.WalletPlatform.Application.Notifications.Models.Internal;
using SNEL.WalletPlatform.Domain.Users;

namespace SNEL.WalletPlatform.Application.Notifications.Services;

/// <summary>
/// Audience resolver that targets users linked to all active customers.
/// </summary>
public sealed class AllCustomersCampaignAudienceResolver(IUnitOfWork unitOfWork)
    : CampaignAudienceResolver
{
    public override string AudienceGroupCode => CampaignAudienceGroupCodes.AllCustomers;

    public override string NameEn => "All customers";

    public override string NameFr => "Tous les clients";

    public override async Task<CampaignAudienceBatch> GetActiveBatchAsync(
        Guid? afterCursorId,
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        var customers = await unitOfWork.CustomerRepository.GetActiveBatchForNotificationsAsync(
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
