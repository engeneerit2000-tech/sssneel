using Microsoft.EntityFrameworkCore;
using NPOI.POIFS.Properties;
using SNEL.WalletPlatform.Domain.Accounting;
using SNEL.WalletPlatform.Domain.Accounting.Enums;
using SNEL.WalletPlatform.Domain.Lookups;
using SNEL.WalletPlatform.Domain.Payments;
using System.IO.Pipelines;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Seed;

public static class PaymentInitModeSeedExtensions
{
    public static ModelBuilder SeedPaymentInitModeLookup(this ModelBuilder modelBuilder)
    {
        return modelBuilder.SeedLookupWithTranslations<PaymentInitModeEntity, PaymentInitModeTranslationEntity, int>(
            PaymentInitModeSeeds.Lookups,
            PaymentInitModeSeeds.Translations);
    }

    private static class PaymentInitModeSeeds
    {
        public static readonly object[] Lookups =
        [
            new { Id = (int)PaymentInitModeEnum.UNKNOWN, Code = PaymentInitModeEnum.UNKNOWN.ToString(), Name = "Unknown", Order = 1, Active = true },
            new { Id = (int)PaymentInitModeEnum.CLIENT_SIDE, Code = PaymentInitModeEnum.CLIENT_SIDE.ToString(), Name = "Client Side", Order = 2, Active = true },
            new { Id = (int)PaymentInitModeEnum.SERVER_SIDE, Code = PaymentInitModeEnum.SERVER_SIDE.ToString(), Name = "Server Side", Order = 3, Active = true },
        ];

        public static readonly object[] Translations =
        [
            new { Id = 1, EntityId = (int)PaymentInitModeEnum.UNKNOWN, LanguageCode = "fr", Name = "Inconnue" },
            new { Id = 2, EntityId = (int)PaymentInitModeEnum.CLIENT_SIDE, LanguageCode = "fr", Name = "Côté client" },
            new { Id = 3, EntityId = (int)PaymentInitModeEnum.SERVER_SIDE, LanguageCode = "fr", Name = "Côté serveur" },
        ];
    }
}





