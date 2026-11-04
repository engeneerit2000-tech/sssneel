using Microsoft.EntityFrameworkCore;
using SNEL.WalletPlatform.Domain.Payments;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Seed;

public static class PaymentCategorySeedExtensions
{
    public static ModelBuilder SeedPaymentCategoryLookup(this ModelBuilder modelBuilder)
    {
        return modelBuilder.SeedLookupWithTranslations<PaymentCategoryEntity, PaymentCategoryTranslationEntity, int>(
            PaymentCategorySeeds.Lookups,
            PaymentCategorySeeds.Translations);
    }

    private static class PaymentCategorySeeds
    {
        public static readonly object[] Lookups =
        [
            new { Id = 1, Code = "WALLET", Name = "Wallet", Order = 1, Active = true }
        ];

        public static readonly object[] Translations =
        [
            new { Id = 1, EntityId = 1, LanguageCode = "fr", Name = "Portefeuille" }
        ];
    }
}
