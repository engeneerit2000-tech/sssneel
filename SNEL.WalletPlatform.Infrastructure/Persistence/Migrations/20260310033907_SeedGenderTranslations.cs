using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedGenderTranslations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "GenderTranslations",
                keyColumn: "Id",
                keyValue: 1,
                column: "LanguageCode",
                value: "fr");

            migrationBuilder.UpdateData(
                table: "GenderTranslations",
                keyColumn: "Id",
                keyValue: 2,
                column: "LanguageCode",
                value: "fr");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "GenderTranslations",
                keyColumn: "Id",
                keyValue: 1,
                column: "LanguageCode",
                value: "FR");

            migrationBuilder.UpdateData(
                table: "GenderTranslations",
                keyColumn: "Id",
                keyValue: 2,
                column: "LanguageCode",
                value: "FR");
        }
    }
}
