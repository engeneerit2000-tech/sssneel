using Microsoft.EntityFrameworkCore;
using NPOI.POIFS.Properties;
using SNEL.WalletPlatform.Domain.Accounting;
using SNEL.WalletPlatform.Domain.Accounting.Enums;
using SNEL.WalletPlatform.Domain.Lookups;
using SNEL.WalletPlatform.Domain.Payments;
using System.IO.Pipelines;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Seed;

public static class PaymentTypeSeedExtensions
{
    public static ModelBuilder SeedPaymentTypeLookup(this ModelBuilder modelBuilder)
    {
        return modelBuilder.SeedLookupWithTranslations<PaymentTypeEntity, PaymentTypeTranslationEntity, int>(
            PaymentTypeSeeds.Lookups,
            PaymentTypeSeeds.Translations);
    }

    private static class PaymentTypeSeeds
    {
        public static readonly object[] Lookups =
        [
            new { Id = (int)PaymentTypeEnum.MOBILE_MONEY, Code = PaymentTypeEnum.MOBILE_MONEY.ToString(), Name = "Mobile Money", Order = 1, Active = true },
            new { Id = (int)PaymentTypeEnum.BANK_CARD, Code = PaymentTypeEnum.BANK_CARD.ToString(), Name = "Bank Card", Order = 2, Active = true },
        ];

        public static readonly object[] Translations =
        [
            new { Id = 1, EntityId = (int)PaymentTypeEnum.MOBILE_MONEY, LanguageCode = "fr", Name = "Argent Mobile" },
            new { Id = 2, EntityId = (int)PaymentTypeEnum.BANK_CARD, LanguageCode = "fr", Name = "Carte Bancaire" },
        ];
    }
}





