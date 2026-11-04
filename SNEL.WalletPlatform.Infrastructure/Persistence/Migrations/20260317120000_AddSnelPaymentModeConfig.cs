using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSnelPaymentModeConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                INSERT INTO "BillerConfigs"
                (
                    "BillerCode",
                    "Name",
                    "BillerName",
                    "Value",
                    "CreatedAtUtc",
                    "UpdatedAtUtc",
                    "CreatedBy",
                    "UpdatedBy"
                )
                SELECT
                    'SNEL',
                    'PaymentModeCode',
                    'MODR_COD_MODE_REGL',
                    '8',
                    NOW(),
                    NOW(),
                    'SYSTEM',
                    'SYSTEM'
                WHERE EXISTS (
                    SELECT 1
                    FROM "Billers"
                    WHERE "Code" = 'SNEL'
                )
                AND NOT EXISTS (
                    SELECT 1
                    FROM "BillerConfigs"
                    WHERE "BillerCode" = 'SNEL'
                      AND "Name" = 'PaymentModeCode'
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DELETE FROM "BillerConfigs"
                WHERE "BillerCode" = 'SNEL'
                  AND "Name" = 'PaymentModeCode';
                """);
        }
    }
}
