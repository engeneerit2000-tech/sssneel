using Microsoft.EntityFrameworkCore.Migrations;
using SNEL.WalletPlatform.Infrastructure.Persistence.Seed;

#nullable disable

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class UpdateAkibaPayWebhookUrlForStaging : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        var configJson = AkibaPayPaymentSeedData.BuildPaymentMethodConfigJson(AkibaPayPaymentSeedData.StagingCallbackUrl)
            .Replace("'", "''", StringComparison.Ordinal);

        var updatedAt = new DateTimeOffset(2026, 6, 1, 19, 40, 0, TimeSpan.Zero)
            .ToString("yyyy-MM-dd HH:mm:sszzz");

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
                    "UpdatedAtUtc" = '{updatedAt}',
                    "UpdatedBy" = 'SYSTEM'
                WHERE "Id" = v_method_id;
            END
            $akiba$;
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        var configJson = AkibaPayPaymentSeedData.BuildPaymentMethodConfigJson()
            .Replace("'", "''", StringComparison.Ordinal);

        var updatedAt = AkibaPayPaymentSeedData.UpdatedAtUtc.ToString("yyyy-MM-dd HH:mm:sszzz");

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
                    "UpdatedAtUtc" = '{updatedAt}',
                    "UpdatedBy" = 'SYSTEM'
                WHERE "Id" = v_method_id;
            END
            $akiba$;
            """);
    }
}
