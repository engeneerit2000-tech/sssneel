using SNEL.WalletPlatform.Domain.Common;

namespace SNEL.WalletPlatform.Domain.Notifications;

public sealed class CampaignTranslationEntity : TranslationEntity<Guid>
{
    public string Message { get; set; } = string.Empty;
    public CampaignEntity Campaign { get; set; } = null!;
}
