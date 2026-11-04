using System.ComponentModel.DataAnnotations.Schema;
using SNEL.WalletPlatform.Domain.Common;

namespace SNEL.WalletPlatform.Domain.Lookups;

[Table("FavoriteTypeTranslations")]
public sealed class FavoriteTypeTranslationEntity : TranslationEntity<int>, ITrackedEntity
{
    [ForeignKey(nameof(EntityId))]
    public FavoriteTypeEntity FavoriteType { get; set; } = null!;

    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}

