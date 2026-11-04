using Microsoft.EntityFrameworkCore.Migrations;
using SNEL.WalletPlatform.Infrastructure.Persistence.Seed;

#nullable disable

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class SeedAkibaPayPaymentProvider : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        var configJson = AkibaPayPaymentSeedData.BuildPaymentMethodConfigJson()
            .Replace("'", "''", StringComparison.Ordinal);

        var seedAt = AkibaPayPaymentSeedData.CreatedAtUtc.ToString("yyyy-MM-dd HH:mm:sszzz");

        migrationBuilder.Sql(
            $"""
            DO $akiba$
            BEGIN
                IF EXISTS (SELECT 1 FROM "PaymentProviders" WHERE "Code" = '{AkibaPayPaymentSeedData.ProviderCode}') THEN
                    RETURN;
                END IF;

                INSERT INTO "Partners" ("Id", "PartnerTypeId", "LegalName", "DisplayName", "Active", "CreatedAtUtc", "CreatedBy", "UpdatedAtUtc", "UpdatedBy")
                VALUES ({AkibaPayPaymentSeedData.PartnerId}, 2, 'AkibaPay', 'AkibaPay', TRUE, '{seedAt}', 'SYSTEM', '{seedAt}', 'SYSTEM')
                ON CONFLICT ("Id") DO NOTHING;

                INSERT INTO "Accounts" ("Id", "AccountTypeId", "Balance", "CreatedAtUtc", "CreatedBy", "CurrencyId", "Name", "Order", "PartnerId", "UpdatedAtUtc", "UpdatedBy", "Version")
                VALUES
                    ('{AkibaPayPaymentSeedData.PspLiabilityCdfAccountId}', 8, 0, '{seedAt}', 'SYSTEM', 1, 'AkibaPay PSP Liability', 1, {AkibaPayPaymentSeedData.PartnerId}, '{seedAt}', 'SYSTEM', 0),
                    ('{AkibaPayPaymentSeedData.PspLiabilityUsdAccountId}', 8, 0, '{seedAt}', 'SYSTEM', 2, 'AkibaPay PSP Liability', 2, {AkibaPayPaymentSeedData.PartnerId}, '{seedAt}', 'SYSTEM', 0),
                    ('{AkibaPayPaymentSeedData.PspBankCdfAccountId}', 5, 0, '{seedAt}', 'SYSTEM', 1, 'AkibaPay Bank', 1, {AkibaPayPaymentSeedData.PartnerId}, '{seedAt}', 'SYSTEM', 0),
                    ('{AkibaPayPaymentSeedData.PspBankUsdAccountId}', 5, 0, '{seedAt}', 'SYSTEM', 2, 'AkibaPay Bank', 2, {AkibaPayPaymentSeedData.PartnerId}, '{seedAt}', 'SYSTEM', 0)
                ON CONFLICT ("Id") DO NOTHING;

                INSERT INTO "PaymentProviders" ("Id", "Code", "Name", "PartnerId", "CreatedAtUtc", "CreatedBy", "UpdatedAtUtc", "UpdatedBy")
                VALUES ({AkibaPayPaymentSeedData.PaymentProviderId}, '{AkibaPayPaymentSeedData.ProviderCode}', 'AkibaPay', {AkibaPayPaymentSeedData.PartnerId}, '{seedAt}', 'SYSTEM', '{seedAt}', 'SYSTEM');

                INSERT INTO "PaymentMethods" ("Id", "PaymentCategoryId", "PaymentProviderId", "Code", "Name", "Active", "PaymentInitModeId", "ConfirmationModeId", "CompletionModeId", "PaymentMethodPurposeId", "ConfirmationTypeId", "Config", "CreatedAtUtc", "CreatedBy", "UpdatedAtUtc", "UpdatedBy")
                VALUES ({AkibaPayPaymentSeedData.PaymentMethodId}, 1, {AkibaPayPaymentSeedData.PaymentProviderId}, '{AkibaPayPaymentSeedData.MethodCode}', '{AkibaPayPaymentSeedData.MethodName}', TRUE, 3, 1, 3, 2, 1, '{configJson}'::jsonb, '{seedAt}', 'SYSTEM', '{seedAt}', 'SYSTEM');

                INSERT INTO "PaymentMethodCurrencies" ("Id", "CurrencyId", "PaymentMethodId", "Min", "Max", "FeesType", "FeesValue", "CostType", "CostValue", "AutoSettleJournal", "BankAccountId", "VAT", "Scale", "Active", "CreatedAtUtc", "CreatedBy", "UpdatedAtUtc", "UpdatedBy")
                VALUES
                    ({AkibaPayPaymentSeedData.PaymentMethodCurrencyCdfId}, 1, {AkibaPayPaymentSeedData.PaymentMethodId}, {AkibaPayPaymentSeedData.CurrencyMinAmount}, {AkibaPayPaymentSeedData.CdfMaxAmount}, 'FLAT', {AkibaPayPaymentSeedData.FeesValue}, 'FLAT', {AkibaPayPaymentSeedData.CostValue}, FALSE, '{AkibaPayPaymentSeedData.PspBankCdfAccountId}', {AkibaPayPaymentSeedData.VatPercent}, {AkibaPayPaymentSeedData.AmountScale}, TRUE, '{seedAt}', 'SYSTEM', '{seedAt}', 'SYSTEM'),
                    ({AkibaPayPaymentSeedData.PaymentMethodCurrencyUsdId}, 2, {AkibaPayPaymentSeedData.PaymentMethodId}, {AkibaPayPaymentSeedData.CurrencyMinAmount}, {AkibaPayPaymentSeedData.UsdMaxAmount}, 'FLAT', {AkibaPayPaymentSeedData.FeesValue}, 'FLAT', {AkibaPayPaymentSeedData.CostValue}, FALSE, '{AkibaPayPaymentSeedData.PspBankUsdAccountId}', {AkibaPayPaymentSeedData.VatPercent}, {AkibaPayPaymentSeedData.AmountScale}, TRUE, '{seedAt}', 'SYSTEM', '{seedAt}', 'SYSTEM');
            END
            $akiba$;

            SELECT setval(pg_get_serial_sequence('"PaymentProviders"', 'Id'), GREATEST((SELECT COALESCE(MAX("Id"), 1) FROM "PaymentProviders"), {AkibaPayPaymentSeedData.PaymentProviderId}));
            SELECT setval(pg_get_serial_sequence('"PaymentMethods"', 'Id'), GREATEST((SELECT COALESCE(MAX("Id"), 1) FROM "PaymentMethods"), {AkibaPayPaymentSeedData.PaymentMethodId}));
            SELECT setval(pg_get_serial_sequence('"PaymentMethodCurrencies"', 'Id'), GREATEST((SELECT COALESCE(MAX("Id"), 1) FROM "PaymentMethodCurrencies"), {AkibaPayPaymentSeedData.PaymentMethodCurrencyUsdId}));
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            $"""
            DELETE FROM "PaymentMethodCurrencies" WHERE "PaymentMethodId" = {AkibaPayPaymentSeedData.PaymentMethodId};
            DELETE FROM "PaymentMethods" WHERE "Id" = {AkibaPayPaymentSeedData.PaymentMethodId};
            DELETE FROM "PaymentProviders" WHERE "Code" = '{AkibaPayPaymentSeedData.ProviderCode}';
            DELETE FROM "Accounts" WHERE "Id" IN (
                '{AkibaPayPaymentSeedData.PspBankUsdAccountId}',
                '{AkibaPayPaymentSeedData.PspBankCdfAccountId}',
                '{AkibaPayPaymentSeedData.PspLiabilityUsdAccountId}',
                '{AkibaPayPaymentSeedData.PspLiabilityCdfAccountId}');
            DELETE FROM "Partners" WHERE "Id" = {AkibaPayPaymentSeedData.PartnerId};
            """);
    }
}
