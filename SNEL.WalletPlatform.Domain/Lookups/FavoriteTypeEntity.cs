using System.ComponentModel.DataAnnotations.Schema;
using SNEL.WalletPlatform.Domain.Common;
using SNEL.WalletPlatform.Domain.Common.Base;
using SNEL.WalletPlatform.Domain.Customers;

namespace SNEL.WalletPlatform.Domain.Lookups;

[Table("FavoriteTypes")]
public sealed class FavoriteTypeEntity : LookupEntity<int>, ITranslatableEntity<FavoriteTypeTranslationEntity, int>, ITrackedEntity
{
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public ICollection<FavoriteTypeTranslationEntity> Translations { get; set; } = new List<FavoriteTypeTranslationEntity>();
    public ICollection<CustomerFavoritesEntity> CustomerFavorites { get; set; } = new List<CustomerFavoritesEntity>();
}

