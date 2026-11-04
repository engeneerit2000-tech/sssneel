using Microsoft.EntityFrameworkCore;
using SNEL.WalletPlatform.Domain.Accounting;
using SNEL.WalletPlatform.Domain.Accounting.Enums;
using SNEL.WalletPlatform.Domain.Payments;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Seed;

public static class JournalLineTypeSeedExtensions
{
    public static ModelBuilder SeedJournalLineTypeLookup(this ModelBuilder modelBuilder)
    {
        return modelBuilder.SeedLookupWithTranslations<JournalLineTypeEntity, JournalLineTypeTranslationEntity, int>(
            JournalLineTypeSeeds.Lookups,
            JournalLineTypeSeeds.Translations);
    }

    private static class JournalLineTypeSeeds
    {
        public static readonly object[] Lookups =
        [
            new { Id = (int)JournalLineTypesEnum.CUSTOMER_LIABILITY, Code = JournalLineTypesEnum.CUSTOMER_LIABILITY.ToString(), Name = "Customer Liability", Order = 1, Active = true },
            new { Id = (int)JournalLineTypesEnum.PSP_LIABILITY, Code = JournalLineTypesEnum.PSP_LIABILITY.ToString(), Name = "Payment Provider Liability", Order = 2, Active = true },
            new { Id = (int)JournalLineTypesEnum.BILLER_LIABILITY, Code = JournalLineTypesEnum.BILLER_LIABILITY.ToString(), Name = "Biller Liability", Order = 3, Active = true },
            new { Id = (int)JournalLineTypesEnum.TOPUP_FEES, Code = JournalLineTypesEnum.TOPUP_FEES.ToString(), Name = "Topup Fees", Order = 4, Active = true },
            new { Id = (int)JournalLineTypesEnum.TOPUP_FEES_TAX, Code = JournalLineTypesEnum.TOPUP_FEES_TAX.ToString(), Name = "Topup Fees Tax", Order = 5, Active = true },
            new { Id = (int)JournalLineTypesEnum.TOPUP_COST, Code = JournalLineTypesEnum.TOPUP_COST.ToString(), Name = "Topup Cost", Order = 6, Active = true },
            new { Id = (int)JournalLineTypesEnum.TOPUP_COST_TAX, Code = JournalLineTypesEnum.TOPUP_COST_TAX.ToString(), Name = "Topup Cost Tax", Order = 7, Active = true },
            new { Id = (int)JournalLineTypesEnum.PAYMENT_FEES, Code = JournalLineTypesEnum.PAYMENT_FEES.ToString(), Name = "Payment Fees", Order = 8, Active = true },
            new { Id = (int)JournalLineTypesEnum.PAYMENT_FEES_TAX, Code = JournalLineTypesEnum.PAYMENT_FEES_TAX.ToString(), Name = "Payment Fees Tax", Order = 9, Active = true },
            new { Id = (int)JournalLineTypesEnum.PAYMENT_COST, Code = JournalLineTypesEnum.PAYMENT_COST.ToString(), Name = "Payment Cost", Order = 10, Active = true },
            new { Id = (int)JournalLineTypesEnum.PAYMENT_COST_TAX, Code = JournalLineTypesEnum.PAYMENT_COST_TAX.ToString(), Name = "Payment Cost Tax", Order = 11, Active = true },
            new { Id = (int)JournalLineTypesEnum.PAYMENT_COMMISSION, Code = JournalLineTypesEnum.PAYMENT_COMMISSION.ToString(), Name = "Payment Commission", Order = 12, Active = true },
            new { Id = (int)JournalLineTypesEnum.PAYMENT_COMMISSION_TAX, Code = JournalLineTypesEnum.PAYMENT_COMMISSION_TAX.ToString(), Name = "Payment Commission Tax", Order = 13, Active = true },
            new { Id = (int)JournalLineTypesEnum.TOPUP_SETTLEMENT, Code = JournalLineTypesEnum.TOPUP_SETTLEMENT.ToString(), Name = "Topup Settlement", Order = 14, Active = true },
            new { Id = (int)JournalLineTypesEnum.PAYMENT_SETTLEMENT, Code = JournalLineTypesEnum.PAYMENT_SETTLEMENT.ToString(), Name = "Payment Settlement", Order = 15, Active = true },
            new { Id = (int)JournalLineTypesEnum.PAYMENT_VOUCHER, Code = JournalLineTypesEnum.PAYMENT_VOUCHER.ToString(), Name = "Payment Voucher", Order = 16, Active = true },
            new { Id = (int)JournalLineTypesEnum.RECEIPT_VOUCHER, Code = JournalLineTypesEnum.RECEIPT_VOUCHER.ToString(), Name = "Receipt Voucher", Order = 17, Active = true },
            new { Id = (int)JournalLineTypesEnum.CREDIT_NOTE, Code = JournalLineTypesEnum.CREDIT_NOTE.ToString(), Name = "Credit Note", Order = 18, Active = true },
            new { Id = (int)JournalLineTypesEnum.DEBIT_NOTE, Code = JournalLineTypesEnum.DEBIT_NOTE.ToString(), Name = "Debit Note", Order = 19, Active = true },
            new { Id = (int)JournalLineTypesEnum.MANUAL_JOURNAL, Code = JournalLineTypesEnum.MANUAL_JOURNAL.ToString(), Name = "Manual Journal", Order = 20, Active = true },

        ];

        public static readonly object[] Translations =
        [
            new { Id = 1, EntityId = (int)JournalLineTypesEnum.CUSTOMER_LIABILITY, LanguageCode = "fr", Name = "Dette envers le client"},
            new { Id = 2, EntityId = (int)JournalLineTypesEnum.PSP_LIABILITY, LanguageCode = "fr", Name = "Dette envers le prestataire de paiement"},
            new { Id = 3, EntityId = (int)JournalLineTypesEnum.BILLER_LIABILITY, LanguageCode = "fr", Name = "Dette envers le facturier"},
            new { Id = 4, EntityId = (int)JournalLineTypesEnum.TOPUP_FEES, LanguageCode = "fr", Name = "Frais de recharge"},
            new { Id = 5, EntityId = (int)JournalLineTypesEnum.TOPUP_FEES_TAX, LanguageCode = "fr", Name = "Frais de recharge"},
            new { Id = 6, EntityId = (int)JournalLineTypesEnum.TOPUP_COST, LanguageCode = "fr", Name = "Coût de recharge"},
            new { Id = 7, EntityId = (int)JournalLineTypesEnum.TOPUP_COST_TAX, LanguageCode = "fr", Name = "Taxe sur le coût de recharge"},
            new { Id = 8, EntityId = (int)JournalLineTypesEnum.PAYMENT_FEES, LanguageCode = "fr", Name = "Frais de paiement"},
            new { Id = 9, EntityId = (int)JournalLineTypesEnum.PAYMENT_FEES_TAX, LanguageCode = "fr", Name = "Taxe sur les frais de paiement"},
            new { Id = 10, EntityId = (int)JournalLineTypesEnum.PAYMENT_COST, LanguageCode = "fr", Name = "Coût de paiement"},
            new { Id = 11, EntityId = (int)JournalLineTypesEnum.PAYMENT_COST_TAX, LanguageCode = "fr", Name = "Taxe sur le coût de paiement"},
            new { Id = 12, EntityId = (int)JournalLineTypesEnum.PAYMENT_COMMISSION, LanguageCode = "fr", Name = "Commission de paiement"},
            new { Id = 13, EntityId = (int)JournalLineTypesEnum.PAYMENT_COMMISSION_TAX, LanguageCode = "fr", Name = "Taxe sur la commission de paiement"},
            new { Id = 14, EntityId = (int)JournalLineTypesEnum.TOPUP_SETTLEMENT, LanguageCode = "fr", Name = "Règlement de recharge"},
            new { Id = 15, EntityId = (int)JournalLineTypesEnum.PAYMENT_SETTLEMENT, LanguageCode = "fr", Name = "Règlement de paiement"},
            new { Id = 16, EntityId = (int)JournalLineTypesEnum.PAYMENT_VOUCHER, LanguageCode = "fr", Name = "Pièce de paiement"},
            new { Id = 17, EntityId = (int)JournalLineTypesEnum.RECEIPT_VOUCHER, LanguageCode = "fr", Name = "Pièce d’encaissement"},
            new { Id = 18, EntityId = (int)JournalLineTypesEnum.CREDIT_NOTE, LanguageCode = "fr", Name = "Avoir"},
            new { Id = 19, EntityId = (int)JournalLineTypesEnum.DEBIT_NOTE, LanguageCode = "fr", Name = "Note de débit"},
            new { Id = 20, EntityId = (int)JournalLineTypesEnum.MANUAL_JOURNAL, LanguageCode = "fr", Name = "Journal manuel"},


        ];
    }
}
