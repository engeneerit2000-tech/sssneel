using System.ComponentModel.DataAnnotations;

namespace SNEL.WalletPlatform.Application.Notifications.Models.Input;

public sealed class CampaignTranslationInputModel
{
    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string LanguageCode { get; set; } = string.Empty;

    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(4000, MinimumLength = 1)]
    public string Message { get; set; } = string.Empty;
}
