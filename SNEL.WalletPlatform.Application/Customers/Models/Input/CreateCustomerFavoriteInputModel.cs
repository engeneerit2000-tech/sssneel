using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SNEL.WalletPlatform.Application.Customers.Models.Input;

public sealed record CreateCustomerFavoriteInputModel
{
    [Required]
    [JsonPropertyName("favorite_type_id")]
    public int FavoriteTypeId { get; init; }

    [Required]
    [JsonPropertyName("favorite_id")]
    public Guid FavoriteId { get; init; }
}

