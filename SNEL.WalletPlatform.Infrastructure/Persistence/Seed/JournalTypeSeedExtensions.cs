using Microsoft.EntityFrameworkCore;
using NPOI.POIFS.Properties;
using SNEL.WalletPlatform.Domain.Accounting;
using SNEL.WalletPlatform.Domain.Accounting.Enums;
using SNEL.WalletPlatform.Domain.Payments;
using System.IO.Pipelines;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Seed;

public static class JournalTypeSeedExtensions
{
    public static ModelBuilder SeedJournalTypeLookup(this ModelBuilder modelBuilder)
    {
        return modelBuilder.SeedLookupWithTranslations<JournalTypeEntity, JournalTypeTranslationEntity, int>(
            JournalTypeSeeds.Lookups,
            JournalTypeSeeds.Translations);
    }

    private static class JournalTypeSeeds
    {
        public static readonly object[] Lookups =
        [
            new { Id = (int)JournalTypesEnum.TOPUP, Code = JournalTypesEnum.TOPUP.ToString(), Name = "Topup", Order = 1, Active = true },
            new { Id = (int)JournalTypesEnum.TOPUP_SETTLEMENT, Code = JournalTypesEnum.TOPUP_SETTLEMENT.ToString(), Name = "Topup Settlement", Order = 2, Active = true },
            new { Id = (int)JournalTypesEnum.PAYMENT, Code = JournalTypesEnum.PAYMENT.ToString(), Name = "Payment", Order = 3, Active = true },
            new { Id = (int)JournalTypesEnum.PAYMENT_SETTLEMENT, Code = JournalTypesEnum.PAYMENT_SETTLEMENT.ToString(), Name = "Payment Settlment", Order = 4, Active = true },
            new { Id = (int)JournalTypesEnum.TOPUP_FEES, Code = JournalTypesEnum.TOPUP_FEES.ToString(), Name = "Topup Fees", Order = 5, Active = true },
            new { Id = (int)JournalTypesEnum.TOPUP_FEES_TAX, Code = JournalTypesEnum.TOPUP_FEES_TAX.ToString(), Name = "Topup Fees Tax", Order = 6, Active = true },
            new { Id = (int)JournalTypesEnum.TOPUP_COST, Code = JournalTypesEnum.TOPUP_COST.ToString(), Name = "Topup Cost", Order = 7, Active = true },
            new { Id = (int)JournalTypesEnum.TOPUP_COST_TAX, Code = JournalTypesEnum.TOPUP_COST_TAX.ToString(), Name = "Topup Cost Tax", Order = 8, Active = true },
            new { Id = (int)JournalTypesEnum.TOPUP_COMMISSION, Code = JournalTypesEnum.TOPUP_COMMISSION.ToString(), Name = "Topup Commission", Order = 9, Active = true },
            new { Id = (int)JournalTypesEnum.TOPUP_COMMISSION_TAX, Code = JournalTypesEnum.TOPUP_COMMISSION_TAX.ToString(), Name = "Topup Commission Tax", Order = 10, Active = true },
            new { Id = (int)JournalTypesEnum.PAYMENT_FEES, Code = JournalTypesEnum.PAYMENT_FEES.ToString(), Name = "Payment Fees", Order = 11, Active = true },
            new { Id = (int)JournalTypesEnum.PAYMENT_FEES_TAX, Code = JournalTypesEnum.PAYMENT_FEES_TAX.ToString(), Name = "Payment Fees Tax", Order = 12, Active = true },
            new { Id = (int)JournalTypesEnum.PAYMENT_COST, Code = JournalTypesEnum.PAYMENT_COST.ToString(), Name = "Payment Cost", Order = 13, Active = true },
            new { Id = (int)JournalTypesEnum.PAYMENT_COST_TAX, Code = JournalTypesEnum.PAYMENT_COST_TAX.ToString(), Name = "Payment Cost Tax", Order = 14, Active = true },
            new { Id = (int)JournalTypesEnum.PAYMENT_COMMISSION, Code = JournalTypesEnum.PAYMENT_COMMISSION.ToString(), Name = "Payment Commission", Order = 15, Active = true },
            new { Id = (int)JournalTypesEnum.PAYMENT_COMMISSION_TAX, Code = JournalTypesEnum.PAYMENT_COMMISSION_TAX.ToString(), Name = "Payment Commission Tax", Order = 16, Active = true },
            new { Id = (int)JournalTypesEnum.OPENING_BALANCE, Code = JournalTypesEnum.OPENING_BALANCE.ToString(), Name = "Opening Balance", Order = 17, Active = true },
            new { Id = (int)JournalTypesEnum.PAYMENT_VOUCHER, Code = JournalTypesEnum.PAYMENT_VOUCHER.ToString(), Name = "Payment Voucher", Order = 18, Active = true },
            new { Id = (int)JournalTypesEnum.RECEIPT_VOUCHER, Code = JournalTypesEnum.RECEIPT_VOUCHER.ToString(), Name = "Receipt Voucher", Order = 19, Active = true },
            new { Id = (int)JournalTypesEnum.CREDIT_NOTE, Code = JournalTypesEnum.CREDIT_NOTE.ToString(), Name = "Credit Note", Order = 20, Active = true },
            new { Id = (int)JournalTypesEnum.DEBIT_NOTE, Code = JournalTypesEnum.DEBIT_NOTE.ToString(), Name = "Debit Note", Order = 21, Active = true },
            new { Id = (int)JournalTypesEnum.MANUAL_JOURNAL, Code = JournalTypesEnum.MANUAL_JOURNAL.ToString(), Name = "Manual Journal", Order = 22, Active = true },
        ];

        public static readonly object[] Translations =
        [
            new { Id = 1, EntityId = (int)JournalTypesEnum.TOPUP, LanguageCode = "fr", Name = "Recharge" },
            new { Id = 2, EntityId = (int)JournalTypesEnum.TOPUP_SETTLEMENT, LanguageCode = "fr", Name = "Règlement de recharge" },
            new { Id = 3, EntityId = (int)JournalTypesEnum.PAYMENT, LanguageCode = "fr", Name = "Paiement" },
            new { Id = 4, EntityId = (int)JournalTypesEnum.PAYMENT_SETTLEMENT, LanguageCode = "fr", Name = "Règlement de paiement" },
            new { Id = 5, EntityId = (int)JournalTypesEnum.TOPUP_FEES, LanguageCode = "fr", Name = "Frais de recharge" },
            new { Id = 6, EntityId = (int)JournalTypesEnum.TOPUP_FEES_TAX, LanguageCode = "fr", Name = "Taxe sur les frais de recharge" },
            new { Id = 7, EntityId = (int)JournalTypesEnum.TOPUP_COST, LanguageCode = "fr", Name = "Coût de recharge" },
            new { Id = 8, EntityId = (int)JournalTypesEnum.TOPUP_COST_TAX, LanguageCode = "fr", Name = "Taxe sur le coût de recharge" },
            new { Id = 9, EntityId = (int)JournalTypesEnum.TOPUP_COMMISSION, LanguageCode = "fr", Name = "Commission de recharge" },
            new { Id = 10, EntityId = (int)JournalTypesEnum.TOPUP_COMMISSION_TAX, LanguageCode = "fr", Name = "Taxe sur la commission de recharge" },
            new { Id = 11, EntityId = (int)JournalTypesEnum.PAYMENT_FEES, LanguageCode = "fr", Name = "Frais de paiement" },
            new { Id = 12, EntityId = (int)JournalTypesEnum.PAYMENT_FEES_TAX, LanguageCode = "fr", Name = "Taxe sur les frais de paiement" },
            new { Id = 13, EntityId = (int)JournalTypesEnum.PAYMENT_COST, LanguageCode = "fr", Name = "Coût de paiement" },
            new { Id = 14, EntityId = (int)JournalTypesEnum.PAYMENT_COST_TAX, LanguageCode = "fr", Name = "Taxe sur le coût de paiement" },
            new { Id = 15, EntityId = (int)JournalTypesEnum.PAYMENT_COMMISSION, LanguageCode = "fr", Name = "Commission de paiement" },
            new { Id = 16, EntityId = (int)JournalTypesEnum.PAYMENT_COMMISSION_TAX, LanguageCode = "fr", Name = "Taxe sur la commission de paiement" },
            new { Id = 17, EntityId = (int)JournalTypesEnum.OPENING_BALANCE, LanguageCode = "fr", Name = "Solde d’ouverture" },
            new { Id = 18, EntityId = (int)JournalTypesEnum.PAYMENT_VOUCHER, LanguageCode = "fr", Name = "Pièce de paiement" },
            new { Id = 19, EntityId = (int)JournalTypesEnum.RECEIPT_VOUCHER, LanguageCode = "fr", Name = "Pièce d’encaissement" },
            new { Id = 20, EntityId = (int)JournalTypesEnum.CREDIT_NOTE, LanguageCode = "fr", Name = "Avoir" },
            new { Id = 21, EntityId = (int)JournalTypesEnum.DEBIT_NOTE, LanguageCode = "fr", Name = "Note de débit" },
            new { Id = 22, EntityId = (int)JournalTypesEnum.MANUAL_JOURNAL, LanguageCode = "fr", Name = "Journal manuel" },
        ];
    }
}





