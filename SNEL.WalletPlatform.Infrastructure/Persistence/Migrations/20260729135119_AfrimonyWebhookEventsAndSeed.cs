using Microsoft.EntityFrameworkCore.Migrations;
using SNEL.WalletPlatform.Infrastructure.Persistence.Seed;

#nullable disable

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AfrimonyWebhookEventsAndSeed : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "AfrimonyWebhookEvents",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                MmTransactionId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                ExternalId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                TxnStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                SenderMsisdn = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                Status = table.Column<string>(type: "text", nullable: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                CreatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                UpdatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AfrimonyWebhookEvents", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AfrimonyWebhookEvents_MmTransactionId",
            table: "AfrimonyWebhookEvents",
            column: "MmTransactionId");

        var configJson = AfrimonyPaymentSeedData.BuildPaymentMethodConfigJson()
            .Replace("'", "''", StringComparison.Ordinal);

        var seedAt = AfrimonyPaymentSeedData.CreatedAtUtc.ToString("yyyy-MM-dd HH:mm:sszzz");

        migrationBuilder.Sql(
            $"""
            DO $afrimony$
            BEGIN
                IF EXISTS (SELECT 1 FROM "PaymentProviders" WHERE "Code" = '{AfrimonyPaymentSeedData.ProviderCode}') THEN
                    RETURN;
                END IF;

                INSERT INTO "Partners" ("Id", "PartnerTypeId", "LegalName", "DisplayName", "Active", "CreatedAtUtc", "CreatedBy", "UpdatedAtUtc", "UpdatedBy")
                VALUES ({AfrimonyPaymentSeedData.PartnerId}, 2, 'Afrimony', 'Afrimony', TRUE, '{seedAt}', 'SYSTEM', '{seedAt}', 'SYSTEM')
                ON CONFLICT ("Id") DO NOTHING;

                INSERT INTO "Accounts" ("Id", "AccountTypeId", "Balance", "CreatedAtUtc", "CreatedBy", "CurrencyId", "Name", "Order", "PartnerId", "UpdatedAtUtc", "UpdatedBy", "Version")
                VALUES
                    ('{AfrimonyPaymentSeedData.PspLiabilityCdfAccountId}', 8, 0, '{seedAt}', 'SYSTEM', 1, 'Afrimony PSP Liability', 1, {AfrimonyPaymentSeedData.PartnerId}, '{seedAt}', 'SYSTEM', 0),
                    ('{AfrimonyPaymentSeedData.PspLiabilityUsdAccountId}', 8, 0, '{seedAt}', 'SYSTEM', 2, 'Afrimony PSP Liability', 2, {AfrimonyPaymentSeedData.PartnerId}, '{seedAt}', 'SYSTEM', 0),
                    ('{AfrimonyPaymentSeedData.PspBankCdfAccountId}', 5, 0, '{seedAt}', 'SYSTEM', 1, 'Afrimony Bank', 1, {AfrimonyPaymentSeedData.PartnerId}, '{seedAt}', 'SYSTEM', 0),
                    ('{AfrimonyPaymentSeedData.PspBankUsdAccountId}', 5, 0, '{seedAt}', 'SYSTEM', 2, 'Afrimony Bank', 2, {AfrimonyPaymentSeedData.PartnerId}, '{seedAt}', 'SYSTEM', 0)
                ON CONFLICT ("Id") DO NOTHING;

                INSERT INTO "PaymentProviders" ("Id", "Code", "Name", "PartnerId", "CreatedAtUtc", "CreatedBy", "UpdatedAtUtc", "UpdatedBy")
                VALUES ({AfrimonyPaymentSeedData.PaymentProviderId}, '{AfrimonyPaymentSeedData.ProviderCode}', 'Afrimony', {AfrimonyPaymentSeedData.PartnerId}, '{seedAt}', 'SYSTEM', '{seedAt}', 'SYSTEM')
                ON CONFLICT ("Id") DO NOTHING;

                INSERT INTO "PaymentMethods" ("Id", "PaymentCategoryId", "PaymentProviderId", "Code", "Name", "Active", "PaymentInitModeId", "ConfirmationModeId", "CompletionModeId", "PaymentMethodPurposeId", "ConfirmationTypeId", "Config", "CreatedAtUtc", "CreatedBy", "UpdatedAtUtc", "UpdatedBy")
                VALUES ({AfrimonyPaymentSeedData.PaymentMethodId}, 1, {AfrimonyPaymentSeedData.PaymentProviderId}, '{AfrimonyPaymentSeedData.MethodCode}', '{AfrimonyPaymentSeedData.MethodName}', TRUE, 3, 1, 3, 2, 1, '{configJson}'::jsonb, '{seedAt}', 'SYSTEM', '{seedAt}', 'SYSTEM')
                ON CONFLICT ("Id") DO NOTHING;

                INSERT INTO "PaymentMethodCurrencies" ("Id", "CurrencyId", "PaymentMethodId", "Min", "Max", "FeesType", "FeesValue", "CostType", "CostValue", "AutoSettleJournal", "BankAccountId", "VAT", "Scale", "Active", "CreatedAtUtc", "CreatedBy", "UpdatedAtUtc", "UpdatedBy")
                VALUES
                    ({AfrimonyPaymentSeedData.PaymentMethodCurrencyCdfId}, 1, {AfrimonyPaymentSeedData.PaymentMethodId}, {AfrimonyPaymentSeedData.CurrencyMinAmount}, {AfrimonyPaymentSeedData.CdfMaxAmount}, 'FLAT', {AfrimonyPaymentSeedData.FeesValue}, 'FLAT', {AfrimonyPaymentSeedData.CostValue}, FALSE, '{AfrimonyPaymentSeedData.PspBankCdfAccountId}', {AfrimonyPaymentSeedData.VatPercent}, {AfrimonyPaymentSeedData.AmountScale}, TRUE, '{seedAt}', 'SYSTEM', '{seedAt}', 'SYSTEM'),
                    ({AfrimonyPaymentSeedData.PaymentMethodCurrencyUsdId}, 2, {AfrimonyPaymentSeedData.PaymentMethodId}, {AfrimonyPaymentSeedData.CurrencyMinAmount}, {AfrimonyPaymentSeedData.UsdMaxAmount}, 'FLAT', {AfrimonyPaymentSeedData.FeesValue}, 'FLAT', {AfrimonyPaymentSeedData.CostValue}, FALSE, '{AfrimonyPaymentSeedData.PspBankUsdAccountId}', {AfrimonyPaymentSeedData.VatPercent}, {AfrimonyPaymentSeedData.AmountScale}, TRUE, '{seedAt}', 'SYSTEM', '{seedAt}', 'SYSTEM')
                ON CONFLICT ("Id") DO NOTHING;
            END
            $afrimony$;

            SELECT setval(pg_get_serial_sequence('"PaymentProviders"', 'Id'), GREATEST((SELECT COALESCE(MAX("Id"), 1) FROM "PaymentProviders"), {AfrimonyPaymentSeedData.PaymentProviderId}));
            SELECT setval(pg_get_serial_sequence('"PaymentMethods"', 'Id'), GREATEST((SELECT COALESCE(MAX("Id"), 1) FROM "PaymentMethods"), {AfrimonyPaymentSeedData.PaymentMethodId}));
            SELECT setval(pg_get_serial_sequence('"PaymentMethodCurrencies"', 'Id'), GREATEST((SELECT COALESCE(MAX("Id"), 1) FROM "PaymentMethodCurrencies"), {AfrimonyPaymentSeedData.PaymentMethodCurrencyUsdId}));
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            $"""
            DELETE FROM "PaymentMethodCurrencies" WHERE "PaymentMethodId" = {AfrimonyPaymentSeedData.PaymentMethodId};
            DELETE FROM "PaymentMethods" WHERE "Id" = {AfrimonyPaymentSeedData.PaymentMethodId};
            DELETE FROM "PaymentProviders" WHERE "Code" = '{AfrimonyPaymentSeedData.ProviderCode}';
            DELETE FROM "Accounts" WHERE "Id" IN (
                '{AfrimonyPaymentSeedData.PspBankUsdAccountId}',
                '{AfrimonyPaymentSeedData.PspBankCdfAccountId}',
                '{AfrimonyPaymentSeedData.PspLiabilityUsdAccountId}',
                '{AfrimonyPaymentSeedData.PspLiabilityCdfAccountId}');
            DELETE FROM "Partners" WHERE "Id" = {AfrimonyPaymentSeedData.PartnerId};
            """);

        migrationBuilder.DropTable(
            name: "AfrimonyWebhookEvents");
    }
}
