using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using SNEL.WalletPlatform.Infrastructure.Persistence;
using SNEL.WalletPlatform.Infrastructure.Persistence.Seed;

#nullable disable

namespace SNEL.WalletPlatform.Infrastructure.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260822000000_AddOrangePaymentProvider")]
public partial class AddOrangePaymentProvider : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        var config = OrangePaymentSeedData.BuildPaymentMethodConfigJson().Replace("'", "''", StringComparison.Ordinal);
        var at = OrangePaymentSeedData.CreatedAtUtc.ToString("yyyy-MM-dd HH:mm:sszzz");
        migrationBuilder.Sql(
            $$"""
            DO $orange$
            BEGIN
                IF EXISTS (SELECT 1 FROM "PaymentProviders" WHERE "Code" = '{{OrangePaymentSeedData.ProviderCode}}') THEN RETURN; END IF;
                INSERT INTO "Partners" ("Id", "PartnerTypeId", "LegalName", "DisplayName", "Active", "CreatedAtUtc", "CreatedBy", "UpdatedAtUtc", "UpdatedBy")
                VALUES ({{OrangePaymentSeedData.PartnerId}}, 2, 'Orange Money', 'Orange Money', TRUE, '{{at}}', 'SYSTEM', '{{at}}', 'SYSTEM') ON CONFLICT ("Id") DO NOTHING;
                INSERT INTO "Accounts" ("Id", "AccountTypeId", "Balance", "CreatedAtUtc", "CreatedBy", "CurrencyId", "Name", "Order", "PartnerId", "UpdatedAtUtc", "UpdatedBy", "Version") VALUES
                ('{{OrangePaymentSeedData.PspLiabilityCdfAccountId}}', 8, 0, '{{at}}', 'SYSTEM', 1, 'Orange PSP Liability', 1, {{OrangePaymentSeedData.PartnerId}}, '{{at}}', 'SYSTEM', 0),
                ('{{OrangePaymentSeedData.PspLiabilityUsdAccountId}}', 8, 0, '{{at}}', 'SYSTEM', 2, 'Orange PSP Liability', 2, {{OrangePaymentSeedData.PartnerId}}, '{{at}}', 'SYSTEM', 0),
                ('{{OrangePaymentSeedData.PspBankCdfAccountId}}', 5, 0, '{{at}}', 'SYSTEM', 1, 'Orange Bank', 1, {{OrangePaymentSeedData.PartnerId}}, '{{at}}', 'SYSTEM', 0),
                ('{{OrangePaymentSeedData.PspBankUsdAccountId}}', 5, 0, '{{at}}', 'SYSTEM', 2, 'Orange Bank', 2, {{OrangePaymentSeedData.PartnerId}}, '{{at}}', 'SYSTEM', 0) ON CONFLICT ("Id") DO NOTHING;
                INSERT INTO "PaymentProviders" ("Id", "Code", "Name", "PartnerId", "CreatedAtUtc", "CreatedBy", "UpdatedAtUtc", "UpdatedBy")
                VALUES ({{OrangePaymentSeedData.PaymentProviderId}}, '{{OrangePaymentSeedData.ProviderCode}}', 'Orange Money', {{OrangePaymentSeedData.PartnerId}}, '{{at}}', 'SYSTEM', '{{at}}', 'SYSTEM') ON CONFLICT ("Id") DO NOTHING;
                INSERT INTO "PaymentMethods" ("Id", "PaymentCategoryId", "PaymentProviderId", "Code", "Name", "Active", "PaymentInitModeId", "ConfirmationModeId", "CompletionModeId", "PaymentMethodPurposeId", "ConfirmationTypeId", "Config", "CreatedAtUtc", "CreatedBy", "UpdatedAtUtc", "UpdatedBy")
                VALUES ({{OrangePaymentSeedData.PaymentMethodId}}, 1, {{OrangePaymentSeedData.PaymentProviderId}}, '{{OrangePaymentSeedData.MethodCode}}', '{{OrangePaymentSeedData.MethodName}}', TRUE, 3, 1, 2, 2, 1, '{{config}}'::jsonb, '{{at}}', 'SYSTEM', '{{at}}', 'SYSTEM') ON CONFLICT ("Id") DO NOTHING;
                INSERT INTO "PaymentMethodCurrencies" ("Id", "CurrencyId", "PaymentMethodId", "Min", "Max", "FeesType", "FeesValue", "CostType", "CostValue", "AutoSettleJournal", "BankAccountId", "VAT", "Scale", "Active", "CreatedAtUtc", "CreatedBy", "UpdatedAtUtc", "UpdatedBy") VALUES
                ({{OrangePaymentSeedData.PaymentMethodCurrencyCdfId}}, 1, {{OrangePaymentSeedData.PaymentMethodId}}, 1, 1000000, 'FLAT', 0, 'FLAT', 0, FALSE, '{{OrangePaymentSeedData.PspBankCdfAccountId}}', 0, 2, TRUE, '{{at}}', 'SYSTEM', '{{at}}', 'SYSTEM'),
                ({{OrangePaymentSeedData.PaymentMethodCurrencyUsdId}}, 2, {{OrangePaymentSeedData.PaymentMethodId}}, 1, 100000, 'FLAT', 0, 'FLAT', 0, FALSE, '{{OrangePaymentSeedData.PspBankUsdAccountId}}', 0, 2, TRUE, '{{at}}', 'SYSTEM', '{{at}}', 'SYSTEM') ON CONFLICT ("Id") DO NOTHING;
            END $orange$;
            SELECT setval(pg_get_serial_sequence('"PaymentProviders"', 'Id'), GREATEST((SELECT COALESCE(MAX("Id"), 1) FROM "PaymentProviders"), {{OrangePaymentSeedData.PaymentProviderId}}));
            SELECT setval(pg_get_serial_sequence('"PaymentMethods"', 'Id'), GREATEST((SELECT COALESCE(MAX("Id"), 1) FROM "PaymentMethods"), {{OrangePaymentSeedData.PaymentMethodId}}));
            SELECT setval(pg_get_serial_sequence('"PaymentMethodCurrencies"', 'Id'), GREATEST((SELECT COALESCE(MAX("Id"), 1) FROM "PaymentMethodCurrencies"), {{OrangePaymentSeedData.PaymentMethodCurrencyUsdId}}));
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql(
        $$"""
        DELETE FROM "PaymentMethodCurrencies" WHERE "PaymentMethodId" = {{OrangePaymentSeedData.PaymentMethodId}};
        DELETE FROM "PaymentMethods" WHERE "Id" = {{OrangePaymentSeedData.PaymentMethodId}};
        DELETE FROM "PaymentProviders" WHERE "Code" = '{{OrangePaymentSeedData.ProviderCode}}';
        DELETE FROM "Accounts" WHERE "Id" IN ('{{OrangePaymentSeedData.PspBankUsdAccountId}}','{{OrangePaymentSeedData.PspBankCdfAccountId}}','{{OrangePaymentSeedData.PspLiabilityUsdAccountId}}','{{OrangePaymentSeedData.PspLiabilityCdfAccountId}}');
        DELETE FROM "Partners" WHERE "Id" = {{OrangePaymentSeedData.PartnerId}};
        """);
}
