using Microsoft.EntityFrameworkCore;
using NPOI.POIFS.Properties;
using SNEL.WalletPlatform.Domain.Accounting;
using SNEL.WalletPlatform.Domain.Accounting.Enums;
using SNEL.WalletPlatform.Domain.Lookups;
using SNEL.WalletPlatform.Domain.Payments;
using System.IO.Pipelines;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Seed;

public static class PaymentMethodPurposeSeedExtensions
{
    public static ModelBuilder SeedPaymentMethodPurposeLookup(this ModelBuilder modelBuilder)
    {
        return modelBuilder.SeedLookupWithTranslations<PaymentMethodPurposeEntity, PaymentMethodPurposeTranslationEntity, int>(
            PaymentMethodPurposeSeeds.Lookups,
            PaymentMethodPurposeSeeds.Translations);
    }

    private static class PaymentMethodPurposeSeeds
    {
        public static readonly object[] Lookups =
        [
            new { Id = (int)PaymentMethodPurposeEnum.UNKNOWN, Code = PaymentMethodPurposeEnum.UNKNOWN.ToString(), Name = "Unknown", Order = 1, Active = true },
            new { Id = (int)PaymentMethodPurposeEnum.TOPUP, Code = PaymentMethodPurposeEnum.TOPUP.ToString(), Name = "Topup", Order = 2, Active = true },
            new { Id = (int)PaymentMethodPurposeEnum.BILL_PAYMENT, Code = PaymentMethodPurposeEnum.BILL_PAYMENT.ToString(), Name = "Bill Payment", Order = 3, Active = true },
            new { Id = (int)PaymentMethodPurposeEnum.METER_CHARGE, Code = PaymentMethodPurposeEnum.METER_CHARGE.ToString(), Name = "Meter Charge", Order = 4, Active = true },
        ];

        public static readonly object[] Translations =
        [
            new { Id = 1, EntityId = (int)PaymentMethodPurposeEnum.UNKNOWN, LanguageCode = "fr", Name = "Aucune" },
            new { Id = 2, EntityId = (int)PaymentMethodPurposeEnum.TOPUP, LanguageCode = "fr", Name = "Recharge" },
            new { Id = 3, EntityId = (int)PaymentMethodPurposeEnum.BILL_PAYMENT, LanguageCode = "fr", Name = "Paiement de facture" },
            new { Id = 4, EntityId = (int)PaymentMethodPurposeEnum.METER_CHARGE, LanguageCode = "fr", Name = "Frais de compteur" },
        ];
    }
}





