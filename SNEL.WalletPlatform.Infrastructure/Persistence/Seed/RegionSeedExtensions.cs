using Microsoft.EntityFrameworkCore;
using SNEL.WalletPlatform.Domain.Lookups;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Seed;

public static class RegionSeedExtensions
{
    public static ModelBuilder SeedRegionLookup(this ModelBuilder modelBuilder)
    {
        return modelBuilder.SeedLookupWithTranslations<RegionEntity, RegionTranslationEntity, int>(
            RegionSeeds.Lookups,
            RegionSeeds.Translations);
    }

    private static class RegionSeeds
    {
        public static readonly object[] Lookups =
        [
            new { Id = 1, Code = "COD", CountryId = 1, Name = "Democratic Republic of the Congo", Order = 1, Active = true }
        ];

        public static readonly object[] Translations =
        [
            new { Id = 1, EntityId = 1, LanguageCode = "fr", Name = "Republique democratique du Congo" }
        ];
    }
}
