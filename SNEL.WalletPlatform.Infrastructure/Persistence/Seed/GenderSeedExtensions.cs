using Microsoft.EntityFrameworkCore;
using SNEL.WalletPlatform.Domain.Lookups;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Seed;

public static class GenderSeedExtensions
{
    public static ModelBuilder SeedGenderLookup(this ModelBuilder modelBuilder)
    {
        return modelBuilder.SeedLookupWithTranslations<GenderEntity, GenderTranslationEntity, int>(
            GenderSeeds.Lookups,
            GenderSeeds.Translations);
    }

    private static class GenderSeeds
    {
        public static readonly object[] Lookups =
        [
            new { Id = 1, Code = "MALE", Name = "Male", Order = 1, Active = true },
            new { Id = 2, Code = "FEMALE", Name = "Female", Order = 2, Active = true }
        ];

        public static readonly object[] Translations =
        [
            new { Id = 1, EntityId = 1, LanguageCode = "fr", Name = "Homme" },
            new { Id = 2, EntityId = 2, LanguageCode = "fr", Name = "Femme" }
        ];
    }
}
