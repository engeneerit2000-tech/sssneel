using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class journalLineSeederFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "JournalLineTypes",
                keyColumn: "Id",
                keyValue: 5,
                column: "Name",
                value: "Topup Fees Tax");

            migrationBuilder.UpdateData(
                table: "JournalLineTypes",
                keyColumn: "Id",
                keyValue: 14,
                column: "Name",
                value: "Topup Settlement");

            migrationBuilder.UpdateData(
                table: "JournalLineTypes",
                keyColumn: "Id",
                keyValue: 15,
                column: "Name",
                value: "Payment Settlement");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "JournalLineTypes",
                keyColumn: "Id",
                keyValue: 5,
                column: "Name",
                value: "Topup Fees");

            migrationBuilder.UpdateData(
                table: "JournalLineTypes",
                keyColumn: "Id",
                keyValue: 14,
                column: "Name",
                value: "Payment Commission");

            migrationBuilder.UpdateData(
                table: "JournalLineTypes",
                keyColumn: "Id",
                keyValue: 15,
                column: "Name",
                value: "Payment Commission Tax");

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
}
