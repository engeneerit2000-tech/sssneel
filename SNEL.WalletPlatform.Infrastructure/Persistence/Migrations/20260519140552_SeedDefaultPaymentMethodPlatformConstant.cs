using Microsoft.EntityFrameworkCore.Migrations;
using SNEL.WalletPlatform.Infrastructure.Persistence.Seed;

#nullable disable

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class SeedDefaultPaymentMethodPlatformConstant : Migration
{
    private const int DefaultPaymentMethodConstantId = 2;
    private const int DefaultPaymentMethodTranslationId = 2;

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        var value = PlatformConstantSeedExtensions.DefaultPaymentMethodValue
            .Replace("'", "''", StringComparison.Ordinal);

        migrationBuilder.Sql(
            $"""
            INSERT INTO "PlatformConstants" ("Id", "Active", "Code", "Group", "Name", "Order", "Value")
            VALUES (
                {DefaultPaymentMethodConstantId},
                TRUE,
                'DEFAULT_PAYMENT_METHOD',
                'PAYMENT',
                'Default Payment Method',
                1,
                '{value}')
            ON CONFLICT ("Id") DO UPDATE SET
                "Active" = EXCLUDED."Active",
                "Code" = EXCLUDED."Code",
                "Group" = EXCLUDED."Group",
                "Name" = EXCLUDED."Name",
                "Order" = EXCLUDED."Order",
                "Value" = EXCLUDED."Value";
            """);

        migrationBuilder.Sql(
            $"""
            INSERT INTO "PlatformConstantTranslations" ("Id", "EntityId", "LanguageCode", "Name")
            VALUES ({DefaultPaymentMethodTranslationId}, {DefaultPaymentMethodConstantId}, 'fr', 'Moyen de paiement par défaut')
            ON CONFLICT ("Id") DO UPDATE SET
                "EntityId" = EXCLUDED."EntityId",
                "LanguageCode" = EXCLUDED."LanguageCode",
                "Name" = EXCLUDED."Name";
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DeleteData(
            table: "PlatformConstantTranslations",
            keyColumn: "Id",
            keyValue: DefaultPaymentMethodTranslationId);

        migrationBuilder.DeleteData(
            table: "PlatformConstants",
            keyColumn: "Id",
            keyValue: DefaultPaymentMethodConstantId);
    }
}
