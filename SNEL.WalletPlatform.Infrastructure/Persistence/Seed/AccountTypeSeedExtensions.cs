using Microsoft.EntityFrameworkCore;
using SNEL.WalletPlatform.Domain.Accounting;
using SNEL.WalletPlatform.Domain.Accounting.Enums;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Seed;

public static class AccountTypeSeedExtensions
{
    public static ModelBuilder SeedAccountTypeLookup(this ModelBuilder modelBuilder)
    {
        return modelBuilder.SeedLookupWithTranslations<AccountTypeEntity, AccountTypeTranslationEntity, int>(
            AccountTypeSeeds.Lookups,
            AccountTypeSeeds.Translations);
    }

    private static class AccountTypeSeeds
    {
        public static readonly object[] Lookups =
        [
            new { Id = (int)AccountTypesEnum.ASSETS, Code = AccountTypesEnum.ASSETS.ToString(), Name = "Assets", Order = 1, Active = true },
            new { Id = (int)AccountTypesEnum.EXPENSES, Code =AccountTypesEnum.EXPENSES.ToString() , Name = "Expenses", Order = 2, Active = true },
            new { Id = (int)AccountTypesEnum.LIABILITY, Code =AccountTypesEnum.LIABILITY.ToString() , Name = "Liability", Order = 3, Active = true },
            new { Id = (int)AccountTypesEnum.REVENUE, Code =AccountTypesEnum.REVENUE.ToString() , Name = "Revenue", Order = 4, Active = true },
            new { Id = (int)AccountTypesEnum.BANK, Code =AccountTypesEnum.BANK.ToString() , Name = "Bank", Order = 5, Active = true },//ASSETS
            new { Id = (int)AccountTypesEnum.TAX_PAYABLE, Code =AccountTypesEnum.TAX_PAYABLE.ToString() , Name = "Tax Payable", Order = 6, Active = true },//LIABILITY
            new { Id = (int)AccountTypesEnum.CUSTOMER_LIABILITY, Code =AccountTypesEnum.CUSTOMER_LIABILITY.ToString() , Name = "Customer Liability", Order = 7, Active = true },//LIABILITY
            new { Id = (int)AccountTypesEnum.PSP_LIABILITY, Code =AccountTypesEnum.PSP_LIABILITY.ToString() , Name = "PSP Liability", Order = 8, Active = true },//LIABILITY
            new { Id = (int)AccountTypesEnum.BILLER_LIABILITY, Code =AccountTypesEnum.BILLER_LIABILITY.ToString() , Name = "Biller Liability", Order = 9, Active = true },//LIABILITY
            new { Id = (int)AccountTypesEnum.TOPUP_COST, Code =AccountTypesEnum.TOPUP_COST.ToString() , Name = "Topup Cost", Order = 10, Active = true },//EXPENSE
            new { Id = (int)AccountTypesEnum.PAYMENT_COST, Code =AccountTypesEnum.PAYMENT_COST.ToString() , Name = "Payment Cost", Order = 11, Active = true },//EXPENSE
            new { Id = (int)AccountTypesEnum.TAX_EXPENSE, Code =AccountTypesEnum.TAX_EXPENSE.ToString() , Name = "Tax Expense", Order = 12, Active = true },//EXPENSE
            new { Id = (int)AccountTypesEnum.CASH, Code =AccountTypesEnum.CASH.ToString() , Name = "CASH", Order = 13, Active = true }//ASSETS
        ];

        public static readonly object[] Translations =
        [
            new { Id = 1, EntityId = 1, LanguageCode = "fr", Name = "Actifs" },
            new { Id = 2, EntityId = 2, LanguageCode = "fr", Name = "Charges" },
            new { Id = 3, EntityId = 3, LanguageCode = "fr", Name = "Passif" },
            new { Id = 4, EntityId = 4, LanguageCode = "fr", Name = "Produits" },
            new { Id = 5, EntityId = 5, LanguageCode = "fr", Name = "Banque" },
            new { Id = 6, EntityId = 6, LanguageCode = "fr", Name = "Taxes à payer" },
            new { Id = 7, EntityId = 7, LanguageCode = "fr", Name = "Dette envers le client" },
            new { Id = 8, EntityId = 8, LanguageCode = "fr", Name = "Dette envers le prestataire de paiement" },
            new { Id = 9, EntityId = 9, LanguageCode = "fr", Name = "Dette envers le facturier" },
            new { Id = 10, EntityId = 10, LanguageCode = "fr", Name = "Coût de recharge" },
            new { Id = 11, EntityId = 11, LanguageCode = "fr", Name = "Coût de paiement" },
            new { Id = 12, EntityId = 12, LanguageCode = "fr", Name = "Charge fiscale" },
            new { Id = 13, EntityId = 13, LanguageCode = "fr", Name = "Trésorerie" }
        ];
    }
}
