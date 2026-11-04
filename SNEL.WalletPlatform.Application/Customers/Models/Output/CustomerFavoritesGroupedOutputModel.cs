using System.Text.Json.Serialization;

namespace SNEL.WalletPlatform.Application.Customers.Models.Output;

public sealed record CustomerFavoritesGroupedOutputModel(
    [property: JsonPropertyName("favorites_by_type")] IReadOnlyCollection<CustomerFavoritesByTypeOutputModel> FavoritesByType);

