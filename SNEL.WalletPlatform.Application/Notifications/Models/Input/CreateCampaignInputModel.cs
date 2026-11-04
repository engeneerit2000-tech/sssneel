using System.ComponentModel.DataAnnotations;
using SNEL.WalletPlatform.Application.Notifications;

namespace SNEL.WalletPlatform.Application.Notifications.Models.Input;

public class CreateCampaignInputModel
{
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Audience group code used to resolve which customers receive the campaign.
    /// Defaults to <c>ALL_CUSTOMERS</c>.
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string AudienceGroupCode { get; set; } = CampaignAudienceGroupCodes.AllCustomers;

    [Required]
    public DateTimeOffset StartAtUtc { get; set; }

    [Required]
    public List<CampaignTranslationInputModel> Translations { get; set; } = [];
}
