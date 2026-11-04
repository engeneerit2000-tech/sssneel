using Microsoft.EntityFrameworkCore;
using SNEL.WalletPlatform.Domain.Lookups;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Seed;

public static class CountrySeedExtensions
{
    public static ModelBuilder SeedCountryLookup(this ModelBuilder modelBuilder)
    {
        return modelBuilder.SeedLookupWithTranslations<CountryEntity, CountryTranslationEntity, int>(
            CountrySeeds.Lookups,
            CountrySeeds.Translations);
    }

    private static class CountrySeeds
    {
        public static readonly object[] Lookups =
        [
            new { Id = 1, Code = "COD", Name = "Democratic Republic of the Congo", Order = 1, Active = true }
        ];

        public static readonly object[] Translations =
        [
            new { Id = 1, EntityId = 1, LanguageCode = "fr", Name = "Republique democratique du Congo" }
        ];
    }
}
