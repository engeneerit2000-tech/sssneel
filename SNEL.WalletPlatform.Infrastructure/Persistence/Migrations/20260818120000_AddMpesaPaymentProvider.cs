using Microsoft.EntityFrameworkCore.Migrations;
using SNEL.WalletPlatform.Infrastructure.Persistence.Seed;

#nullable disable

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Migrations;

public partial class AddMpesaPaymentProvider : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        var config = MpesaPaymentSeedData.BuildPaymentMethodConfigJson().Replace("'", "''", StringComparison.Ordinal);
        var at = MpesaPaymentSeedData.CreatedAtUtc.ToString("yyyy-MM-dd HH:mm:sszzz");
        migrationBuilder.Sql(
            $$"""
            DO $mpesa$
            BEGIN
                IF EXISTS (SELECT 1 FROM "PaymentProviders" WHERE "Code" = '{{MpesaPaymentSeedData.ProviderCode}}') THEN RETURN; END IF;
                INSERT INTO "Partners" ("Id", "PartnerTypeId", "LegalName", "DisplayName", "Active", "CreatedAtUtc", "CreatedBy", "UpdatedAtUtc", "UpdatedBy")
                VALUES ({{MpesaPaymentSeedData.PartnerId}}, 2, 'MPesa', 'MPesa', TRUE, '{{at}}', 'SYSTEM', '{{at}}', 'SYSTEM') ON CONFLICT ("Id") DO NOTHING;
                INSERT INTO "Accounts" ("Id", "AccountTypeId", "Balance", "CreatedAtUtc", "CreatedBy", "CurrencyId", "Name", "Order", "PartnerId", "UpdatedAtUtc", "UpdatedBy", "Version") VALUES
                ('{{MpesaPaymentSeedData.PspLiabilityCdfAccountId}}', 8, 0, '{{at}}', 'SYSTEM', 1, 'MPesa PSP Liability', 1, {{MpesaPaymentSeedData.PartnerId}}, '{{at}}', 'SYSTEM', 0),
                ('{{MpesaPaymentSeedData.PspLiabilityUsdAccountId}}', 8, 0, '{{at}}', 'SYSTEM', 2, 'MPesa PSP Liability', 2, {{MpesaPaymentSeedData.PartnerId}}, '{{at}}', 'SYSTEM', 0),
                ('{{MpesaPaymentSeedData.PspBankCdfAccountId}}', 5, 0, '{{at}}', 'SYSTEM', 1, 'MPesa Bank', 1, {{MpesaPaymentSeedData.PartnerId}}, '{{at}}', 'SYSTEM', 0),
                ('{{MpesaPaymentSeedData.PspBankUsdAccountId}}', 5, 0, '{{at}}', 'SYSTEM', 2, 'MPesa Bank', 2, {{MpesaPaymentSeedData.PartnerId}}, '{{at}}', 'SYSTEM', 0) ON CONFLICT ("Id") DO NOTHING;
                INSERT INTO "PaymentProviders" ("Id", "Code", "Name", "PartnerId", "CreatedAtUtc", "CreatedBy", "UpdatedAtUtc", "UpdatedBy")
                VALUES ({{MpesaPaymentSeedData.PaymentProviderId}}, '{{MpesaPaymentSeedData.ProviderCode}}', 'MPesa', {{MpesaPaymentSeedData.PartnerId}}, '{{at}}', 'SYSTEM', '{{at}}', 'SYSTEM') ON CONFLICT ("Id") DO NOTHING;
                INSERT INTO "PaymentMethods" ("Id", "PaymentCategoryId", "PaymentProviderId", "Code", "Name", "Active", "PaymentInitModeId", "ConfirmationModeId", "CompletionModeId", "PaymentMethodPurposeId", "ConfirmationTypeId", "Config", "CreatedAtUtc", "CreatedBy", "UpdatedAtUtc", "UpdatedBy")
                VALUES ({{MpesaPaymentSeedData.PaymentMethodId}}, 1, {{MpesaPaymentSeedData.PaymentProviderId}}, '{{MpesaPaymentSeedData.MethodCode}}', '{{MpesaPaymentSeedData.MethodName}}', TRUE, 3, 1, 2, 2, 1, '{{config}}'::jsonb, '{{at}}', 'SYSTEM', '{{at}}', 'SYSTEM') ON CONFLICT ("Id") DO NOTHING;
                INSERT INTO "PaymentMethodCurrencies" ("Id", "CurrencyId", "PaymentMethodId", "Min", "Max", "FeesType", "FeesValue", "CostType", "CostValue", "AutoSettleJournal", "BankAccountId", "VAT", "Scale", "Active", "CreatedAtUtc", "CreatedBy", "UpdatedAtUtc", "UpdatedBy") VALUES
                ({{MpesaPaymentSeedData.PaymentMethodCurrencyCdfId}}, 1, {{MpesaPaymentSeedData.PaymentMethodId}}, 1, 1000000, 'FLAT', 0, 'FLAT', 0, FALSE, '{{MpesaPaymentSeedData.PspBankCdfAccountId}}', 0, 2, TRUE, '{{at}}', 'SYSTEM', '{{at}}', 'SYSTEM'),
                ({{MpesaPaymentSeedData.PaymentMethodCurrencyUsdId}}, 2, {{MpesaPaymentSeedData.PaymentMethodId}}, 1, 100000, 'FLAT', 0, 'FLAT', 0, FALSE, '{{MpesaPaymentSeedData.PspBankUsdAccountId}}', 0, 2, TRUE, '{{at}}', 'SYSTEM', '{{at}}', 'SYSTEM') ON CONFLICT ("Id") DO NOTHING;
            END $mpesa$;
            SELECT setval(pg_get_serial_sequence('"PaymentProviders"', 'Id'), GREATEST((SELECT COALESCE(MAX("Id"), 1) FROM "PaymentProviders"), {{MpesaPaymentSeedData.PaymentProviderId}}));
            SELECT setval(pg_get_serial_sequence('"PaymentMethods"', 'Id'), GREATEST((SELECT COALESCE(MAX("Id"), 1) FROM "PaymentMethods"), {{MpesaPaymentSeedData.PaymentMethodId}}));
            SELECT setval(pg_get_serial_sequence('"PaymentMethodCurrencies"', 'Id'), GREATEST((SELECT COALESCE(MAX("Id"), 1) FROM "PaymentMethodCurrencies"), {{MpesaPaymentSeedData.PaymentMethodCurrencyUsdId}}));
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql(
        $$"""
        DELETE FROM "PaymentMethodCurrencies" WHERE "PaymentMethodId" = {{MpesaPaymentSeedData.PaymentMethodId}};
        DELETE FROM "PaymentMethods" WHERE "Id" = {{MpesaPaymentSeedData.PaymentMethodId}};
        DELETE FROM "PaymentProviders" WHERE "Code" = '{{MpesaPaymentSeedData.ProviderCode}}';
        DELETE FROM "Accounts" WHERE "Id" IN ('{{MpesaPaymentSeedData.PspBankUsdAccountId}}','{{MpesaPaymentSeedData.PspBankCdfAccountId}}','{{MpesaPaymentSeedData.PspLiabilityUsdAccountId}}','{{MpesaPaymentSeedData.PspLiabilityCdfAccountId}}');
        DELETE FROM "Partners" WHERE "Id" = {{MpesaPaymentSeedData.PartnerId}};
        """);
}
