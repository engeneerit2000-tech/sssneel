using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedAfrimonyProviderRouterPrefixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.InsertData(
                table: "PaymentProviderRouters",
                columns: new[] { "Id", "CompanyPrefix", "CountryCode", "CreatedAtUtc", "CreatedBy", "PaymentMethodCode", "PaymentProviderCode", "UpdatedAtUtc", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, "90", "243", new DateTimeOffset(new DateTime(2026, 8, 19, 12, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "SYSTEM", "MERCHANT_PAY", "AFRIMONEY", new DateTimeOffset(new DateTime(2026, 8, 19, 12, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "SYSTEM" },
                    { 2, "91", "243", new DateTimeOffset(new DateTime(2026, 8, 19, 12, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "SYSTEM", "MERCHANT_PAY", "AFRIMONEY", new DateTimeOffset(new DateTime(2026, 8, 19, 12, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "SYSTEM" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "PaymentProviderRouters",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "PaymentProviderRouters",
                keyColumn: "Id",
                keyValue: 2);

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
        }
    }
}
