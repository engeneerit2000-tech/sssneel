using Microsoft.EntityFrameworkCore;
using SNEL.WalletPlatform.Domain.Billing;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Seed;

public static class BillingCategorySeedExtensions
{
    public static ModelBuilder SeedBillingCategoryLookup(this ModelBuilder modelBuilder)
    {
        return modelBuilder.SeedLookupWithTranslations<BillingCategoryEntity, BillingCategoryTranslationEntity, int>(
            BillingCategorySeeds.Lookups,
            BillingCategorySeeds.Translations);
    }

    private static class BillingCategorySeeds
    {
        public static readonly object[] Lookups =
        [
            new { Id = 1, Code = "UTILITY", Name = "Utility", Order = 1, Active = true }
        ];

        public static readonly object[] Translations =
        [
            new { Id = 1, EntityId = 1, LanguageCode = "fr", Name = "Services publics" }
        ];
    }
}
