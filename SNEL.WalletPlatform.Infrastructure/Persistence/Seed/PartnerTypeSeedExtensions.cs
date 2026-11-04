using Microsoft.EntityFrameworkCore;
using SNEL.WalletPlatform.Domain.Lookups;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Seed;

public static class PartnerTypeSeedExtensions
{
    public static ModelBuilder SeedPartnerTypeLookup(this ModelBuilder modelBuilder)
    {
        return modelBuilder.SeedLookupWithTranslations<PartnerTypeEntity, PartnerTypeTranslationEntity, int>(
            PartnerTypeSeeds.Lookups,
            PartnerTypeSeeds.Translations);
    }

    private static class PartnerTypeSeeds
    {
        public static readonly object[] Lookups =
        [
            new { Id = 1, Code = "BILLER", Name = "Biller", Order = 1, Active = true },
            new { Id = 2, Code = "PSP", Name = "Payment Provider", Order = 2, Active = true },
            new { Id = 3, Code = "CUSTOMER", Name = "Customer", Order = 3, Active = true },
            new { Id = 4, Code = "PLATFORM", Name = "Platform", Order = 4, Active = true }
        ];

        public static readonly object[] Translations =
        [
            new { Id = 1, EntityId = 1, LanguageCode = "fr", Name = "Facturier" },
            new { Id = 2, EntityId = 2, LanguageCode = "fr", Name = "Fournisseur de paiement" },
            new { Id = 3, EntityId = 3, LanguageCode = "fr", Name = "Client" },
            new { Id = 4, EntityId = 4, LanguageCode = "fr", Name = "Plateforme" }
        ];
    }
}
