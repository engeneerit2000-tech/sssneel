using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SNEL.WalletPlatform.Infrastructure.Persistence.Seed;

#nullable disable

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261011000000_SeedEasypayPaymentProvider")]
public sealed class SeedEasypayPaymentProvider : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        var config = EasypayPaymentSeedData.BuildPaymentMethodConfigJson()
            .Replace("'", "''", StringComparison.Ordinal);
        var at = EasypayPaymentSeedData.CreatedAtUtc.ToString("yyyy-MM-dd HH:mm:sszzz");

        migrationBuilder.Sql(
            $$"""
            DO $easypay$
            BEGIN
                IF NOT EXISTS (
                    SELECT 1 FROM "PaymentProviders"
                    WHERE "Code" = '{{EasypayPaymentSeedData.ProviderCode}}'
                ) THEN
                    IF EXISTS (SELECT 1 FROM "Partners" WHERE "Id" = {{EasypayPaymentSeedData.PartnerId}}) THEN
                        RAISE EXCEPTION 'Cannot seed EasyPay: partner id {{EasypayPaymentSeedData.PartnerId}} is already in use.';
                    END IF;
                    IF EXISTS (SELECT 1 FROM "PaymentProviders" WHERE "Id" = {{EasypayPaymentSeedData.PaymentProviderId}}) THEN
                        RAISE EXCEPTION 'Cannot seed EasyPay: payment provider id {{EasypayPaymentSeedData.PaymentProviderId}} is already in use.';
                    END IF;
                    IF EXISTS (SELECT 1 FROM "PaymentMethods" WHERE "Id" = {{EasypayPaymentSeedData.PaymentMethodId}}) THEN
                        RAISE EXCEPTION 'Cannot seed EasyPay: payment method id {{EasypayPaymentSeedData.PaymentMethodId}} is already in use.';
                    END IF;

                    INSERT INTO "Partners"
                        ("Id", "PartnerTypeId", "LegalName", "DisplayName", "Active", "CreatedAtUtc", "CreatedBy", "UpdatedAtUtc", "UpdatedBy")
                    VALUES
                        ({{EasypayPaymentSeedData.PartnerId}}, 2, 'Easypay', 'Easypay', TRUE, '{{at}}', 'SYSTEM', '{{at}}', 'SYSTEM');

                    INSERT INTO "Accounts"
                        ("Id", "AccountTypeId", "Balance", "CreatedAtUtc", "CreatedBy", "CurrencyId", "Name", "Order", "PartnerId", "UpdatedAtUtc", "UpdatedBy", "Version")
                    VALUES
                        ('{{EasypayPaymentSeedData.PspLiabilityCdfAccountId}}', 8, 0, '{{at}}', 'SYSTEM', 1, 'Easypay PSP Liability', 1, {{EasypayPaymentSeedData.PartnerId}}, '{{at}}', 'SYSTEM', 0),
                        ('{{EasypayPaymentSeedData.PspLiabilityUsdAccountId}}', 8, 0, '{{at}}', 'SYSTEM', 2, 'Easypay PSP Liability', 2, {{EasypayPaymentSeedData.PartnerId}}, '{{at}}', 'SYSTEM', 0),
                        ('{{EasypayPaymentSeedData.PspBankCdfAccountId}}', 5, 0, '{{at}}', 'SYSTEM', 1, 'Easypay Bank', 1, {{EasypayPaymentSeedData.PartnerId}}, '{{at}}', 'SYSTEM', 0),
                        ('{{EasypayPaymentSeedData.PspBankUsdAccountId}}', 5, 0, '{{at}}', 'SYSTEM', 2, 'Easypay Bank', 2, {{EasypayPaymentSeedData.PartnerId}}, '{{at}}', 'SYSTEM', 0);

                    INSERT INTO "PaymentProviders"
                        ("Id", "Code", "Name", "PartnerId", "CreatedAtUtc", "CreatedBy", "UpdatedAtUtc", "UpdatedBy")
                    VALUES
                        ({{EasypayPaymentSeedData.PaymentProviderId}}, '{{EasypayPaymentSeedData.ProviderCode}}', 'Easypay', {{EasypayPaymentSeedData.PartnerId}}, '{{at}}', 'SYSTEM', '{{at}}', 'SYSTEM');

                    INSERT INTO "PaymentMethods"
                        ("Id", "PaymentCategoryId", "PaymentProviderId", "Code", "Name", "Active", "PaymentInitModeId", "ConfirmationModeId", "CompletionModeId", "PaymentMethodPurposeId", "ConfirmationTypeId", "Config", "CreatedAtUtc", "CreatedBy", "UpdatedAtUtc", "UpdatedBy")
                    VALUES
                        ({{EasypayPaymentSeedData.PaymentMethodId}}, 1, {{EasypayPaymentSeedData.PaymentProviderId}}, '{{EasypayPaymentSeedData.MethodCode}}', '{{EasypayPaymentSeedData.MethodName}}', TRUE, 3, 1, 2, 2, 1, '{{config}}'::jsonb, '{{at}}', 'SYSTEM', '{{at}}', 'SYSTEM');

                    INSERT INTO "PaymentMethodCurrencies"
                        ("Id", "CurrencyId", "PaymentMethodId", "Min", "Max", "FeesType", "FeesValue", "CostType", "CostValue", "AutoSettleJournal", "BankAccountId", "VAT", "Scale", "Active", "CreatedAtUtc", "CreatedBy", "UpdatedAtUtc", "UpdatedBy")
                    VALUES
                        ({{EasypayPaymentSeedData.PaymentMethodCurrencyCdfId}}, 1, {{EasypayPaymentSeedData.PaymentMethodId}}, {{EasypayPaymentSeedData.CdfMinAmount}}, {{EasypayPaymentSeedData.CdfMaxAmount}}, 'FLAT', {{EasypayPaymentSeedData.FeesValue}}, 'FLAT', {{EasypayPaymentSeedData.CostValue}}, FALSE, '{{EasypayPaymentSeedData.PspBankCdfAccountId}}', {{EasypayPaymentSeedData.VatPercent}}, {{EasypayPaymentSeedData.AmountScale}}, TRUE, '{{at}}', 'SYSTEM', '{{at}}', 'SYSTEM'),
                        ({{EasypayPaymentSeedData.PaymentMethodCurrencyUsdId}}, 2, {{EasypayPaymentSeedData.PaymentMethodId}}, {{EasypayPaymentSeedData.UsdMinAmount}}, {{EasypayPaymentSeedData.UsdMaxAmount}}, 'FLAT', {{EasypayPaymentSeedData.FeesValue}}, 'FLAT', {{EasypayPaymentSeedData.CostValue}}, FALSE, '{{EasypayPaymentSeedData.PspBankUsdAccountId}}', {{EasypayPaymentSeedData.VatPercent}}, {{EasypayPaymentSeedData.AmountScale}}, TRUE, '{{at}}', 'SYSTEM', '{{at}}', 'SYSTEM');
                END IF;
            END $easypay$;

            DO $easypay_router$
            BEGIN
                IF NOT EXISTS (
                    SELECT 1 FROM "PaymentProviderRouters"
                    WHERE "CountryCode" = '{{EasypayPaymentSeedData.CountryCode}}'
                      AND "CompanyPrefix" = '{{EasypayPaymentSeedData.CompanyPrefix}}'
                      AND "PaymentProviderCode" = '{{EasypayPaymentSeedData.ProviderCode}}'
                      AND "PaymentMethodCode" = '{{EasypayPaymentSeedData.MethodCode}}'
                ) THEN
                    IF EXISTS (
                        SELECT 1 FROM "PaymentProviderRouters"
                        WHERE "Id" = {{EasypayPaymentSeedData.PaymentProviderRouterId}}
                    ) THEN
                        RAISE EXCEPTION 'Cannot seed EasyPay: payment provider router id {{EasypayPaymentSeedData.PaymentProviderRouterId}} is already in use.';
                    END IF;

                    INSERT INTO "PaymentProviderRouters"
                        ("Id", "CountryCode", "CompanyPrefix", "PaymentProviderCode", "PaymentMethodCode", "CreatedAtUtc", "CreatedBy", "UpdatedAtUtc", "UpdatedBy")
                    VALUES
                        ({{EasypayPaymentSeedData.PaymentProviderRouterId}}, '{{EasypayPaymentSeedData.CountryCode}}', '{{EasypayPaymentSeedData.CompanyPrefix}}', '{{EasypayPaymentSeedData.ProviderCode}}', '{{EasypayPaymentSeedData.MethodCode}}', '{{at}}', 'SYSTEM', '{{at}}', 'SYSTEM');
                END IF;
            END $easypay_router$;

            SELECT setval(pg_get_serial_sequence('"Partners"', 'Id'), GREATEST((SELECT COALESCE(MAX("Id"), 1) FROM "Partners"), {{EasypayPaymentSeedData.PartnerId}}));
            SELECT setval(pg_get_serial_sequence('"PaymentProviders"', 'Id'), GREATEST((SELECT COALESCE(MAX("Id"), 1) FROM "PaymentProviders"), {{EasypayPaymentSeedData.PaymentProviderId}}));
            SELECT setval(pg_get_serial_sequence('"PaymentMethods"', 'Id'), GREATEST((SELECT COALESCE(MAX("Id"), 1) FROM "PaymentMethods"), {{EasypayPaymentSeedData.PaymentMethodId}}));
            SELECT setval(pg_get_serial_sequence('"PaymentMethodCurrencies"', 'Id'), GREATEST((SELECT COALESCE(MAX("Id"), 1) FROM "PaymentMethodCurrencies"), {{EasypayPaymentSeedData.PaymentMethodCurrencyUsdId}}));
            SELECT setval(pg_get_serial_sequence('"PaymentProviderRouters"', 'Id'), GREATEST((SELECT COALESCE(MAX("Id"), 1) FROM "PaymentProviderRouters"), {{EasypayPaymentSeedData.PaymentProviderRouterId}}));
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            $$"""
            DELETE FROM "PaymentProviderRouters"
            WHERE "PaymentProviderCode" = '{{EasypayPaymentSeedData.ProviderCode}}'
              AND "PaymentMethodCode" = '{{EasypayPaymentSeedData.MethodCode}}';
            DELETE FROM "PaymentMethodCurrencies"
            WHERE "PaymentMethodId" IN (
                SELECT "Id" FROM "PaymentMethods"
                WHERE "PaymentProviderId" IN (
                    SELECT "Id" FROM "PaymentProviders"
                    WHERE "Code" = '{{EasypayPaymentSeedData.ProviderCode}}'
                )
            );
            DELETE FROM "PaymentMethods"
            WHERE "PaymentProviderId" IN (
                SELECT "Id" FROM "PaymentProviders"
                WHERE "Code" = '{{EasypayPaymentSeedData.ProviderCode}}'
            );
            DELETE FROM "PaymentProviders" WHERE "Code" = '{{EasypayPaymentSeedData.ProviderCode}}';
            DELETE FROM "Accounts" WHERE "Id" IN (
                '{{EasypayPaymentSeedData.PspBankUsdAccountId}}',
                '{{EasypayPaymentSeedData.PspBankCdfAccountId}}',
                '{{EasypayPaymentSeedData.PspLiabilityUsdAccountId}}',
                '{{EasypayPaymentSeedData.PspLiabilityCdfAccountId}}'
            );
            DELETE FROM "Partners" WHERE "Id" = {{EasypayPaymentSeedData.PartnerId}};
            """);
    }
}
