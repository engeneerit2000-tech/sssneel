using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

using SNEL.WalletPlatform.Infrastructure.Persistence.Seed;

namespace SNEL.WalletPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEasypayCardCdfCurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var at = new DateTimeOffset(2026, 11, 2, 0, 0, 0, TimeSpan.Zero)
                .ToString("yyyy-MM-dd HH:mm:sszzz");

            migrationBuilder.Sql(
                $$"""
                DO $easypay_card_cdf$
                DECLARE
                    easypay_provider_id bigint;
                    easypay_partner_id bigint;
                    card_method_id bigint;
                BEGIN
                    SELECT "Id", "PartnerId"
                    INTO easypay_provider_id, easypay_partner_id
                    FROM "PaymentProviders"
                    WHERE "Code" = '{{EasypayPaymentSeedData.ProviderCode}}';

                    IF easypay_provider_id IS NULL THEN
                        RAISE EXCEPTION 'Cannot seed EasyPay Card Collections CDF currency: EasyPay provider is not configured.';
                    END IF;

                    IF NOT EXISTS (
                        SELECT 1
                        FROM "Accounts"
                        WHERE "Id" = '{{EasypayPaymentSeedData.PspBankCdfAccountId}}'
                          AND "PartnerId" = easypay_partner_id
                          AND "CurrencyId" = 1
                          AND "AccountTypeId" = 5
                    ) THEN
                        RAISE EXCEPTION 'Cannot seed EasyPay Card Collections CDF currency: the shared EasyPay COLLECTIONS CDF bank account is missing or incompatible.';
                    END IF;

                    SELECT "Id" INTO card_method_id
                    FROM "PaymentMethods"
                    WHERE "Code" = '{{EasypayPaymentSeedData.CardMethodCode}}'
                      AND "PaymentProviderId" = easypay_provider_id;

                    IF card_method_id IS NULL THEN
                        RAISE EXCEPTION 'Cannot seed EasyPay Card Collections CDF currency: CARD_COLLECTIONS is not configured.';
                    END IF;

                    IF EXISTS (
                        SELECT 1
                        FROM "PaymentMethodCurrencies"
                        WHERE "PaymentMethodId" = card_method_id
                          AND "CurrencyId" = 1
                    ) THEN
                        UPDATE "PaymentMethodCurrencies"
                        SET "BankAccountId" = '{{EasypayPaymentSeedData.PspBankCdfAccountId}}',
                            "UpdatedAtUtc" = '{{at}}',
                            "UpdatedBy" = 'SYSTEM'
                        WHERE "PaymentMethodId" = card_method_id
                          AND "CurrencyId" = 1;
                    ELSE
                        IF EXISTS (
                            SELECT 1
                            FROM "PaymentMethodCurrencies"
                            WHERE "Id" = {{EasypayPaymentSeedData.CardPaymentMethodCurrencyCdfId}}
                        ) THEN
                            RAISE EXCEPTION 'Cannot seed EasyPay Card Collections CDF currency: payment method currency id {{EasypayPaymentSeedData.CardPaymentMethodCurrencyCdfId}} is already in use.';
                        END IF;

                        INSERT INTO "PaymentMethodCurrencies"
                            ("Id", "CurrencyId", "PaymentMethodId", "Min", "Max", "FeesType", "FeesValue", "CostType", "CostValue", "AutoSettleJournal", "BankAccountId", "VAT", "Scale", "Active", "CreatedAtUtc", "CreatedBy", "UpdatedAtUtc", "UpdatedBy")
                        VALUES
                            ({{EasypayPaymentSeedData.CardPaymentMethodCurrencyCdfId}}, 1, card_method_id, {{EasypayPaymentSeedData.CardCdfMinAmount}}, {{EasypayPaymentSeedData.CdfMaxAmount}}, 'FLAT', {{EasypayPaymentSeedData.FeesValue}}, 'FLAT', {{EasypayPaymentSeedData.CostValue}}, FALSE, '{{EasypayPaymentSeedData.PspBankCdfAccountId}}', {{EasypayPaymentSeedData.VatPercent}}, {{EasypayPaymentSeedData.AmountScale}}, TRUE, '{{at}}', 'SYSTEM', '{{at}}', 'SYSTEM');
                    END IF;
                END $easypay_card_cdf$;

                SELECT setval(
                    pg_get_serial_sequence('"PaymentMethodCurrencies"', 'Id'),
                    GREATEST((SELECT COALESCE(MAX("Id"), 1) FROM "PaymentMethodCurrencies"), {{EasypayPaymentSeedData.CardPaymentMethodCurrencyCdfId}}));
                """);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                $$"""
                DELETE FROM "PaymentMethodCurrencies" pmc
                USING "PaymentMethods" pm, "PaymentProviders" pp
                WHERE pmc."Id" = {{EasypayPaymentSeedData.CardPaymentMethodCurrencyCdfId}}
                  AND pmc."CurrencyId" = 1
                  AND pmc."PaymentMethodId" = pm."Id"
                  AND pm."PaymentProviderId" = pp."Id"
                  AND pm."Code" = '{{EasypayPaymentSeedData.CardMethodCode}}'
                  AND pp."Code" = '{{EasypayPaymentSeedData.ProviderCode}}';
                """);

        }
    }
}
