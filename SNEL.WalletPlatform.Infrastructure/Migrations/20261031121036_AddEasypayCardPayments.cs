using Microsoft.EntityFrameworkCore.Migrations;
using SNEL.WalletPlatform.Infrastructure.Persistence.Seed;

#nullable disable

namespace SNEL.WalletPlatform.Infrastructure.Migrations;

public partial class AddEasypayCardPayments : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        var cardConfig = EasypayPaymentSeedData.BuildCardPaymentMethodConfigJson()
            .Replace("'", "''", StringComparison.Ordinal);
        var at = new DateTimeOffset(2026, 10, 31, 0, 0, 0, TimeSpan.Zero)
            .ToString("yyyy-MM-dd HH:mm:sszzz");

        migrationBuilder.AddColumn<string>(
            name: "Phone",
            table: "EasypayWebhookEvents",
            type: "character varying(50)",
            maxLength: 50,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Reason",
            table: "EasypayWebhookEvents",
            type: "character varying(500)",
            maxLength: 500,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "TelecomId",
            table: "EasypayWebhookEvents",
            type: "character varying(200)",
            maxLength: 200,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Type",
            table: "EasypayWebhookEvents",
            type: "character varying(50)",
            maxLength: 50,
            nullable: true);

        migrationBuilder.UpdateData(
            table: "PaymentInitModeTranslations",
            keyColumn: "Id",
            keyValue: 2,
            column: "Name",
            value: "Côté client");

        migrationBuilder.UpdateData(
            table: "PaymentInitModeTranslations",
            keyColumn: "Id",
            keyValue: 3,
            column: "Name",
            value: "Côté serveur");

        migrationBuilder.Sql(
            $$"""
            DO $easypay_card$
            DECLARE
                easypay_provider_id bigint;
                card_method_id bigint;
            BEGIN
                SELECT "Id" INTO easypay_provider_id
                FROM "PaymentProviders"
                WHERE "Code" = '{{EasypayPaymentSeedData.ProviderCode}}';

                IF easypay_provider_id IS NULL THEN
                    RAISE EXCEPTION 'Cannot seed EasyPay Card Collections: EasyPay provider is not configured.';
                END IF;

                SELECT "Id" INTO card_method_id
                FROM "PaymentMethods"
                WHERE "Code" = '{{EasypayPaymentSeedData.CardMethodCode}}'
                  AND "PaymentProviderId" = easypay_provider_id;

                IF card_method_id IS NULL THEN
                    IF EXISTS (
                        SELECT 1 FROM "PaymentMethods"
                        WHERE "Id" = {{EasypayPaymentSeedData.CardPaymentMethodId}}
                    ) THEN
                        RAISE EXCEPTION 'Cannot seed EasyPay Card Collections: payment method id {{EasypayPaymentSeedData.CardPaymentMethodId}} is already in use.';
                    END IF;

                    INSERT INTO "PaymentMethods"
                        ("Id", "PaymentCategoryId", "PaymentProviderId", "Code", "Name", "Active", "PaymentInitModeId", "ConfirmationModeId", "CompletionModeId", "PaymentMethodPurposeId", "ConfirmationTypeId", "Config", "CreatedAtUtc", "CreatedBy", "UpdatedAtUtc", "UpdatedBy")
                    VALUES
                        ({{EasypayPaymentSeedData.CardPaymentMethodId}}, 1, easypay_provider_id, '{{EasypayPaymentSeedData.CardMethodCode}}', '{{EasypayPaymentSeedData.CardMethodName}}', FALSE, 3, 1, 2, 2, 1, '{{cardConfig}}'::jsonb, '{{at}}', 'SYSTEM', '{{at}}', 'SYSTEM')
                    RETURNING "Id" INTO card_method_id;
                END IF;

                IF NOT EXISTS (
                    SELECT 1 FROM "PaymentMethodCurrencies"
                    WHERE "PaymentMethodId" = card_method_id AND "CurrencyId" = 2
                ) THEN
                    IF EXISTS (
                        SELECT 1 FROM "PaymentMethodCurrencies"
                        WHERE "Id" = {{EasypayPaymentSeedData.CardPaymentMethodCurrencyUsdId}}
                    ) THEN
                        RAISE EXCEPTION 'Cannot seed EasyPay Card Collections: payment method currency id {{EasypayPaymentSeedData.CardPaymentMethodCurrencyUsdId}} is already in use.';
                    END IF;

                    INSERT INTO "PaymentMethodCurrencies"
                        ("Id", "CurrencyId", "PaymentMethodId", "Min", "Max", "FeesType", "FeesValue", "CostType", "CostValue", "AutoSettleJournal", "BankAccountId", "VAT", "Scale", "Active", "CreatedAtUtc", "CreatedBy", "UpdatedAtUtc", "UpdatedBy")
                    VALUES
                        ({{EasypayPaymentSeedData.CardPaymentMethodCurrencyUsdId}}, 2, card_method_id, {{EasypayPaymentSeedData.CardUsdMinAmount}}, {{EasypayPaymentSeedData.UsdMaxAmount}}, 'FLAT', {{EasypayPaymentSeedData.FeesValue}}, 'FLAT', {{EasypayPaymentSeedData.CostValue}}, FALSE, '{{EasypayPaymentSeedData.PspBankUsdAccountId}}', {{EasypayPaymentSeedData.VatPercent}}, {{EasypayPaymentSeedData.AmountScale}}, TRUE, '{{at}}', 'SYSTEM', '{{at}}', 'SYSTEM');
                END IF;
            END $easypay_card$;

            SELECT setval(
                pg_get_serial_sequence('"PaymentMethods"', 'Id'),
                GREATEST((SELECT COALESCE(MAX("Id"), 1) FROM "PaymentMethods"), {{EasypayPaymentSeedData.CardPaymentMethodId}}));
            SELECT setval(
                pg_get_serial_sequence('"PaymentMethodCurrencies"', 'Id'),
                GREATEST((SELECT COALESCE(MAX("Id"), 1) FROM "PaymentMethodCurrencies"), {{EasypayPaymentSeedData.CardPaymentMethodCurrencyUsdId}}));
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            $$"""
            DELETE FROM "PaymentMethodCurrencies"
            WHERE "PaymentMethodId" IN (
                SELECT pm."Id"
                FROM "PaymentMethods" pm
                INNER JOIN "PaymentProviders" pp ON pp."Id" = pm."PaymentProviderId"
                WHERE pm."Code" = '{{EasypayPaymentSeedData.CardMethodCode}}'
                  AND pp."Code" = '{{EasypayPaymentSeedData.ProviderCode}}'
            );

            DELETE FROM "PaymentMethods" pm
            USING "PaymentProviders" pp
            WHERE pm."PaymentProviderId" = pp."Id"
              AND pm."Code" = '{{EasypayPaymentSeedData.CardMethodCode}}'
              AND pp."Code" = '{{EasypayPaymentSeedData.ProviderCode}}';
            """);

        migrationBuilder.DropColumn(
            name: "Phone",
            table: "EasypayWebhookEvents");

        migrationBuilder.DropColumn(
            name: "Reason",
            table: "EasypayWebhookEvents");

        migrationBuilder.DropColumn(
            name: "TelecomId",
            table: "EasypayWebhookEvents");

        migrationBuilder.DropColumn(
            name: "Type",
            table: "EasypayWebhookEvents");

        migrationBuilder.UpdateData(
            table: "PaymentInitModeTranslations",
            keyColumn: "Id",
            keyValue: 2,
            column: "Name",
            value: "C�t� client");

        migrationBuilder.UpdateData(
            table: "PaymentInitModeTranslations",
            keyColumn: "Id",
            keyValue: 3,
            column: "Name",
            value: "C�t� serveur");
    }
}
