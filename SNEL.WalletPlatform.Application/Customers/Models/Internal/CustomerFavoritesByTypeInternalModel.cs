namespace SNEL.WalletPlatform.Application.Customers.Models.Internal;

public sealed record CustomerFavoritesByTypeInternalModel(
    int FavoriteTypeId,
    string FavoriteTypeName,
    IReadOnlyCollection<Guid> FavoriteIds);
