using Microsoft.EntityFrameworkCore;
using SNEL.WalletPlatform.Domain.Lookups;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Seed;

public static class ChannelSeedExtensions
{
    public static ModelBuilder SeedChannelLookup(this ModelBuilder modelBuilder)
    {
        return modelBuilder.SeedLookupWithTranslations<ChannelEntity, ChannelTranslationEntity, int>(
            ChannelSeeds.Lookups,
            ChannelSeeds.Translations);
    }

    private static class ChannelSeeds
    {
        public static readonly object[] Lookups =
        [
            new { Id = 1, Code = "WEB", Name = "Web", Order = 1, Active = true },
            new { Id = 2, Code = "MOBILE", Name = "Mobile", Order = 2, Active = true }
        ];

        public static readonly object[] Translations =
        [
            new { Id = 1, EntityId = 1, LanguageCode = "fr", Name = "Web" },
            new { Id = 2, EntityId = 2, LanguageCode = "fr", Name = "Mobile" }
        ];
    }
}
