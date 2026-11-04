using System.ComponentModel.DataAnnotations;

namespace SNEL.WalletPlatform.Application.Notifications.Models.Input;

public sealed class UpsertCampaignTranslationInputModel
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(4000, MinimumLength = 1)]
    public string Message { get; set; } = string.Empty;
}
