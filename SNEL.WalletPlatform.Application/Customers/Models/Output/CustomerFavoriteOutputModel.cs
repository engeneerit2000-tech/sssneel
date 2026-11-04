using System.Text.Json.Serialization;

namespace SNEL.WalletPlatform.Application.Customers.Models.Output;

public sealed class CustomerFavoriteOutputModel
{
    [JsonPropertyName("customer_id")]
    public Guid CustomerId { get; set; }

    [JsonPropertyName("favorite_type_id")]
    public int FavoriteTypeId { get; set; }

    [JsonPropertyName("favorite_id")]
    public Guid FavoriteId { get; set; }
}

