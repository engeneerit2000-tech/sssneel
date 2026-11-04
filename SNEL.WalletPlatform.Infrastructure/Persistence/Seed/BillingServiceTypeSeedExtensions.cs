using Microsoft.EntityFrameworkCore;
using SNEL.WalletPlatform.Domain.Lookups;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Seed;

public static class BillingServiceTypeSeedExtensions
{
    public static ModelBuilder SeedBillingServiceTypeLookup(this ModelBuilder modelBuilder)
    {
        return modelBuilder.SeedLookupWithTranslations<BillingServiceTypeEntity, BillingServiceTypeTranslationEntity, int>(
            BillingServiceTypeSeeds.Lookups,
            BillingServiceTypeSeeds.Translations);
    }

    private static class BillingServiceTypeSeeds
    {
        public static readonly object[] Lookups =
        [
            new { Id = 1, Code = "PREPAID", Name = "Prepaid", Order = 1, Active = true },
            new { Id = 2, Code = "POSTPAID", Name = "Postpaid", Order = 2, Active = true },
            new { Id = 3, Code = "BOTH", Name = "Both", Order = 3, Active = true }
        ];

        public static readonly object[] Translations =
        [
            new { Id = 1, EntityId = 1, LanguageCode = "fr", Name = "Prepaye" },
            new { Id = 2, EntityId = 2, LanguageCode = "fr", Name = "Postpaye" },
            new { Id = 3, EntityId = 3, LanguageCode = "fr", Name = "Les deux" }
        ];
    }
}
