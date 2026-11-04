using SNEL.WalletPlatform.Domain.Notifications;

namespace SNEL.WalletPlatform.Application.Notifications.Models.Output;

public sealed record CampaignOutputModel(
    Guid Id,
    string Description,
    string AudienceGroupCode,
    DateTimeOffset StartAtUtc,
    CampaignStatus Status,
    Guid? LastProcessedCustomerId,
    DateTimeOffset? NotificationsStartedAtUtc,
    DateTimeOffset? NotificationsCompletedAtUtc,
    IReadOnlyList<CampaignTranslationOutputModel> Translations,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    string? CreatedBy,
    string? UpdatedBy);
