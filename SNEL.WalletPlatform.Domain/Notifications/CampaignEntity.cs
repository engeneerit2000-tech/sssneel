using SNEL.WalletPlatform.Domain.Common;

namespace SNEL.WalletPlatform.Domain.Notifications;

/// <summary>
/// Defines a mass notification campaign to be sent to a target customer audience group.
/// </summary>
public sealed class CampaignEntity : TrackedEntity<Guid>, ITranslatableEntity<CampaignTranslationEntity, Guid>
{
    private CampaignEntity() { }

    public string Description { get; private set; } = string.Empty;
    public string AudienceGroupCode { get; private set; } = string.Empty;
    public DateTimeOffset StartAtUtc { get; private set; }
    public CampaignStatus Status { get; private set; } = CampaignStatus.Scheduled;
    public Guid? LastProcessedCustomerId { get; private set; }
    public DateTimeOffset? NotificationsStartedAtUtc { get; private set; }
    public DateTimeOffset? NotificationsCompletedAtUtc { get; private set; }

    public ICollection<CampaignTranslationEntity> Translations { get; set; } =
        new List<CampaignTranslationEntity>();

    public static CampaignEntity Create(
        string description,
        string audienceGroupCode,
        DateTimeOffset startAtUtc)
    {
        if (string.IsNullOrWhiteSpace(audienceGroupCode))
            throw new ArgumentException("Audience group code cannot be empty", nameof(audienceGroupCode));

        return new CampaignEntity
        {
            Id = Guid.NewGuid(),
            Description = description?.Trim() ?? string.Empty,
            AudienceGroupCode = audienceGroupCode.Trim().ToUpperInvariant(),
            StartAtUtc = startAtUtc,
            Status = CampaignStatus.Scheduled
        };
    }

    public void Update(
        string description,
        string audienceGroupCode,
        DateTimeOffset startAtUtc)
    {
        if (string.IsNullOrWhiteSpace(audienceGroupCode))
            throw new ArgumentException("Audience group code cannot be empty", nameof(audienceGroupCode));

        Description = description?.Trim() ?? string.Empty;
        AudienceGroupCode = audienceGroupCode.Trim().ToUpperInvariant();
        StartAtUtc = startAtUtc;
        ResetDispatchState();
    }

    public void ResetDispatchState()
    {
        Status = CampaignStatus.Scheduled;
        LastProcessedCustomerId = null;
        NotificationsStartedAtUtc = null;
        NotificationsCompletedAtUtc = null;
    }

    public void BeginDispatch()
    {
        if (Status is CampaignStatus.Completed or CampaignStatus.Cancelled)
            return;

        if (Status == CampaignStatus.Scheduled)
            NotificationsStartedAtUtc = DateTimeOffset.UtcNow;

        Status = CampaignStatus.InProgress;
    }

    public void RecordBatchProgress(Guid lastCustomerId) => LastProcessedCustomerId = lastCustomerId;

    public void MarkCompleted()
    {
        Status = CampaignStatus.Completed;
        NotificationsCompletedAtUtc = DateTimeOffset.UtcNow;
    }

    public void MarkCancelled() => Status = CampaignStatus.Cancelled;

    public bool IsDispatchActive() =>
        Status is CampaignStatus.Scheduled or CampaignStatus.InProgress;

    public bool CanBeModified() => Status != CampaignStatus.Completed;

    public bool CanBeScheduled() => Status is CampaignStatus.Scheduled or CampaignStatus.InProgress;

    public string? GetTitle(string languageCode) =>
        Translations.FirstOrDefault(t => t.LanguageCode == languageCode)?.Name;

    public string? GetMessage(string languageCode) =>
        Translations.FirstOrDefault(t => t.LanguageCode == languageCode)?.Message;
}
