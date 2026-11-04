using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using SNEL.WalletPlatform.Domain.Common;
using SNEL.WalletPlatform.Domain.Lookups;

namespace SNEL.WalletPlatform.Domain.Customers;

[PrimaryKey(nameof(CustomerId), nameof(FavoriteTypeId), nameof(FavoriteId))]
[Table("CustomerFavorites")]
public sealed class CustomerFavoritesEntity : ITrackedEntity
{
    [Column("customer_id")]
    public Guid CustomerId { get; set; }

    [ForeignKey(nameof(CustomerId))]
    public CustomerEntity Customer { get; set; } = null!;

    [Column("favorite_type_id")]
    public int FavoriteTypeId { get; set; }

    [ForeignKey(nameof(FavoriteTypeId))]
    public FavoriteTypeEntity FavoriteType { get; set; } = null!;

    [Column("favorite_id")]
    public Guid FavoriteId { get; set; }

    // The "favorite_id" you asked for is the SubscriptionId, which maps to CustomerSubscriptionEntity.Id (Guid).
    [ForeignKey(nameof(FavoriteId))]
    public CustomerSubscriptionEntity Favorite { get; set; } = null!;

    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}

