using Microsoft.EntityFrameworkCore;
using SNEL.WalletPlatform.Domain.Partners;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Seed;

public static class ContactTypeSeedExtensions
{
    public static ModelBuilder SeedContactTypeLookup(this ModelBuilder modelBuilder)
    {
        return modelBuilder.SeedLookupWithTranslations<ContactTypeEntity, ContactTypeTranslationEntity, int>(
            ContactTypeSeeds.Lookups,
            ContactTypeSeeds.Translations);
    }

    private static class ContactTypeSeeds
    {
        public static readonly object[] Lookups =
        [
            new { Id = 1, Code = "PHONE", Name = "Phone", Order = 1, Active = true },
            new { Id = 2, Code = "MOBILE", Name = "Mobile", Order = 2, Active = true },
            new { Id = 3, Code = "EMAIL", Name = "Email", Order = 3, Active = true },
            new { Id = 4, Code = "POST", Name = "Post ", Order = 4, Active = true },
            new { Id = 5, Code = "FAX", Name = "Fax", Order = 5, Active = true }
        ];

        public static readonly object[] Translations =
        [
            new { Id = 1, EntityId = 1, LanguageCode = "fr", Name = "Telephone" },
            new { Id = 2, EntityId = 2, LanguageCode = "fr", Name = "Mobile" },
            new { Id = 3, EntityId = 3, LanguageCode = "fr", Name = "Email" },
            new { Id = 4, EntityId = 4, LanguageCode = "fr", Name = "Courrier" },
            new { Id = 5, EntityId = 5, LanguageCode = "fr", Name = "Fax" }
        ];
    }
}
