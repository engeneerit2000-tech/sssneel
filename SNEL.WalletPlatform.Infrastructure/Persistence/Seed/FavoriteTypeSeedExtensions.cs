using Microsoft.EntityFrameworkCore;
using SNEL.WalletPlatform.Domain.Lookups;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Seed;

public static class FavoriteTypeSeedExtensions
{
    public static ModelBuilder SeedFavoriteTypeLookup(this ModelBuilder modelBuilder)
    {
        return modelBuilder.SeedLookupWithTranslations<FavoriteTypeEntity, FavoriteTypeTranslationEntity, int>(
            FavoriteTypeSeeds.Lookups,
            FavoriteTypeSeeds.Translations);
    }

    private static class FavoriteTypeSeeds
    {
        private static readonly DateTimeOffset SeedAuditDate = new(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        private const string SeedAuditBy = "SYSTEM";

        public static readonly object[] Lookups =
        [
            new
            {
                Id = 1,
                Code = "SUBSCRIPTION",
                Name = "Subscription",
                Order = 1,
                Active = true,
                CreatedAtUtc = SeedAuditDate,
                UpdatedAtUtc = SeedAuditDate,
                CreatedBy = SeedAuditBy,
                UpdatedBy = SeedAuditBy
            },
        ];

        public static readonly object[] Translations =
        [
            new
            {
                Id = 1,
                EntityId = 1,
                LanguageCode = "fr",
                Name = "Abonnement",
                CreatedAtUtc = SeedAuditDate,
                UpdatedAtUtc = SeedAuditDate,
                CreatedBy = SeedAuditBy,
                UpdatedBy = SeedAuditBy
            },
        ];
    }
}
