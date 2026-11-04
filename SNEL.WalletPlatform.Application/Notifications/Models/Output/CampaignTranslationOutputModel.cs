namespace SNEL.WalletPlatform.Application.Notifications.Models.Output;

public sealed record CampaignTranslationOutputModel(
    int Id,
    string LanguageCode,
    string Title,
    string Message);
