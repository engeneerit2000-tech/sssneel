using System.Text.Json.Serialization;

namespace SNEL.WalletPlatform.Application.Customers.Models.Output;

public sealed record CustomerFavoritesByTypeOutputModel(
    [property: JsonPropertyName("favorite_type_id")] int FavoriteTypeId,
    [property: JsonPropertyName("favorite_type_name")] string FavoriteTypeName,
    [property: JsonPropertyName("subscriptions")] IReadOnlyCollection<CustomerSubscriptionOutputModel> Subscriptions);

