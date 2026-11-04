using Microsoft.EntityFrameworkCore;
using SNEL.WalletPlatform.Domain.Lookups;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Seed;

public static class CurrencySeedExtensions
{
    public static ModelBuilder SeedCurrencyLookup(this ModelBuilder modelBuilder)
    {
        return modelBuilder.SeedLookupWithTranslations<CurrencyEntity, CurrencyTranslationEntity, int>(
            CurrencySeeds.Lookups,
            CurrencySeeds.Translations);
    }

    private static class CurrencySeeds
    {
        public static readonly object[] Lookups =
        [
            new { Id = 1, Code = "976", Name = "Congolese Franc", Order = 1, Active = true, Abbreviation = "CDF", Minor = 2 },
            new { Id = 2, Code = "840", Name = "US Dollar", Order = 2, Active = true, Abbreviation = "USD", Minor = 2 }
        ];

        public static readonly object[] Translations =
        [
            new { Id = 1, EntityId = 1, LanguageCode = "fr", Name = "Franc congolais" },
            new { Id = 2, EntityId = 2, LanguageCode = "fr", Name = "Dollar americain" }
        ];
    }
}
