using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SNEL.WalletPlatform.Application.Customers.Models.Input;

public sealed record SetSubscriptionFavoriteInputModel
{
    [Required]
    [JsonPropertyName("favorite_id")]
    public Guid FavoriteId { get; init; }

    [Required]
    [JsonPropertyName("is_favorite")]
    public bool IsFavorite { get; init; }
}

