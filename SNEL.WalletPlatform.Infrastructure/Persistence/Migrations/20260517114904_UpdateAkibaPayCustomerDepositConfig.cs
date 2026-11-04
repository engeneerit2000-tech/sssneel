using Microsoft.EntityFrameworkCore.Migrations;
using SNEL.WalletPlatform.Infrastructure.Persistence.Seed;

#nullable disable

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class UpdateAkibaPayCustomerDepositConfig : Migration
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
            DECLARE
                v_method_id bigint;
            BEGIN
                SELECT pm."Id" INTO v_method_id
                FROM "PaymentMethods" pm
                INNER JOIN "PaymentProviders" pp ON pp."Id" = pm."PaymentProviderId"
                WHERE pp."Code" = '{AkibaPayPaymentSeedData.ProviderCode}'
                  AND pm."Code" = '{AkibaPayPaymentSeedData.MethodCode}'
                LIMIT 1;

                IF v_method_id IS NULL THEN
                    RETURN;
                END IF;

                UPDATE "PaymentMethods"
                SET "Config" = '{configJson}'::jsonb,
                    "UpdatedAtUtc" = '{seedAt}',
                    "UpdatedBy" = 'SYSTEM'
                WHERE "Id" = v_method_id;

                INSERT INTO "PaymentMethodCurrencies" (
                    "Id", "CurrencyId", "PaymentMethodId", "Min", "Max",
                    "FeesType", "FeesValue", "CostType", "CostValue",
                    "AutoSettleJournal", "BankAccountId", "VAT", "Scale", "Active",
                    "CreatedAtUtc", "CreatedBy", "UpdatedAtUtc", "UpdatedBy")
                VALUES
                    ({AkibaPayPaymentSeedData.PaymentMethodCurrencyCdfId}, 1, v_method_id, {AkibaPayPaymentSeedData.CurrencyMinAmount}, {AkibaPayPaymentSeedData.CdfMaxAmount}, 'FLAT', {AkibaPayPaymentSeedData.FeesValue}, 'FLAT', {AkibaPayPaymentSeedData.CostValue}, FALSE, '{AkibaPayPaymentSeedData.PspBankCdfAccountId}', {AkibaPayPaymentSeedData.VatPercent}, {AkibaPayPaymentSeedData.AmountScale}, TRUE, '{seedAt}', 'SYSTEM', '{seedAt}', 'SYSTEM'),
                    ({AkibaPayPaymentSeedData.PaymentMethodCurrencyUsdId}, 2, v_method_id, {AkibaPayPaymentSeedData.CurrencyMinAmount}, {AkibaPayPaymentSeedData.UsdMaxAmount}, 'FLAT', {AkibaPayPaymentSeedData.FeesValue}, 'FLAT', {AkibaPayPaymentSeedData.CostValue}, FALSE, '{AkibaPayPaymentSeedData.PspBankUsdAccountId}', {AkibaPayPaymentSeedData.VatPercent}, {AkibaPayPaymentSeedData.AmountScale}, TRUE, '{seedAt}', 'SYSTEM', '{seedAt}', 'SYSTEM')
                ON CONFLICT ("CurrencyId", "PaymentMethodId") DO UPDATE SET
                    "Min" = EXCLUDED."Min",
                    "Max" = EXCLUDED."Max",
                    "FeesType" = EXCLUDED."FeesType",
                    "FeesValue" = EXCLUDED."FeesValue",
                    "CostType" = EXCLUDED."CostType",
                    "CostValue" = EXCLUDED."CostValue",
                    "BankAccountId" = EXCLUDED."BankAccountId",
                    "VAT" = EXCLUDED."VAT",
                    "Scale" = EXCLUDED."Scale",
                    "Active" = EXCLUDED."Active",
                    "UpdatedAtUtc" = EXCLUDED."UpdatedAtUtc",
                    "UpdatedBy" = EXCLUDED."UpdatedBy";
            END
            $akiba$;
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Config and currency limits are environment-specific; no-op on rollback.
    }
}
