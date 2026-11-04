using Microsoft.EntityFrameworkCore;
using SNEL.WalletPlatform.Domain.Lookups;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Seed;

public static class CenterSeedExtension
{
    public static ModelBuilder SeedCenterLookup(this ModelBuilder modelBuilder)
    {
        return modelBuilder.SeedLookupWithTranslations<CenterEntity, CenterTranslationEntity, int>(
            CenterSeeds.Lookups,
            CenterSeeds.Translations);
    }


    private static class CenterSeeds
    {
        public static readonly object[] Lookups =
        [
            new { Id = 1, Code = "06-03", Name = "Kinshasa-East", Order = 1, Active = true },
            new { Id = 2, Code = "06-05", Name = "Kinshasa-Center", Order = 2, Active = true },
        ];

        public static readonly object[] Translations =
        [
            new { Id = 1, EntityId = 1, LanguageCode = "fr", Name = "Kinshasa-Est" },
            new { Id = 2, EntityId = 2, LanguageCode = "fr", Name = "Kinshasa-Centre" },
        ];
    }
}
