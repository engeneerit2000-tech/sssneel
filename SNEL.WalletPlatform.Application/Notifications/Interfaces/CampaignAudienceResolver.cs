using SNEL.WalletPlatform.Application.Notifications.Models.Internal;

namespace SNEL.WalletPlatform.Application.Notifications.Interfaces;

/// <summary>
/// Resolves the next batch of notification recipients for a campaign audience group.
/// Similar to <c>PspConnector</c>: each group has its own resolver implementation.
/// </summary>
public abstract class CampaignAudienceResolver
{
    public abstract string AudienceGroupCode { get; }

    public abstract string NameEn { get; }

    public abstract string NameFr { get; }

    public abstract Task<CampaignAudienceBatch> GetActiveBatchAsync(
        Guid? afterCursorId,
        int batchSize,
        CancellationToken cancellationToken = default);
}
