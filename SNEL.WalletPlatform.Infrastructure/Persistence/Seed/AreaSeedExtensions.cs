using Microsoft.EntityFrameworkCore;
using SNEL.WalletPlatform.Domain.Lookups;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Seed;

public static class AreaSeedExtensions
{
    public static ModelBuilder SeedAreaLookup(this ModelBuilder modelBuilder)
    {
        return modelBuilder.SeedLookupWithTranslations<AreaEntity, AreaTranslationEntity, int>(
            AreaSeeds.Lookups,
            AreaSeeds.Translations);
    }

    private static class AreaSeeds
    {
        public static readonly object[] Lookups =
        [
            new { Id = 1, Code = "06", CityId = 1, Name = "East", Order = 1, Active = true },
            new { Id = 2, Code = "07", CityId = 1, Name = "West", Order = 2, Active = true },
            new { Id = 3, Code = "08", CityId = 1, Name = "North", Order = 3, Active = true },
            new { Id = 4, Code = "09", CityId = 1, Name = "South", Order = 4, Active = true },
            new { Id = 5, Code = "10", CityId = 1, Name = "Center", Order = 5, Active = true }
        ];

        public static readonly object[] Translations =
        [
            new { Id = 1, EntityId = 1, LanguageCode = "fr", Name = "Est" },
            new { Id = 2, EntityId = 2, LanguageCode = "fr", Name = "Ouest" },
            new { Id = 3, EntityId = 3, LanguageCode = "fr", Name = "Nord" },
            new { Id = 4, EntityId = 4, LanguageCode = "fr", Name = "Sud" },
            new { Id = 5, EntityId = 5, LanguageCode = "fr", Name = "Centre" }
        ];
    }
}
