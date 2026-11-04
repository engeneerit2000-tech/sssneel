using Microsoft.EntityFrameworkCore;
using SNEL.WalletPlatform.Domain.Lookups;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Seed;

public static class CitySeedExtensions
{
    public static ModelBuilder SeedCityLookup(this ModelBuilder modelBuilder)
    {
        return modelBuilder.SeedLookupWithTranslations<CityEntity, CityTranslationEntity, int>(
            CitySeeds.Lookups,
            CitySeeds.Translations);
    }

    private static class CitySeeds
    {
        public static readonly object[] Lookups =
        [
            new { Id = 1, Code = "KINSHASA", RegionId = 1, Name = "Kinshasa", Order = 1, Active = true }
        ];

        public static readonly object[] Translations =
        [
            new { Id = 1, EntityId = 1, LanguageCode = "fr", Name = "Kinshasa" }
        ];
    }
}
