using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Seed_BillerLiability_JournalLineTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "JournalLineTypes",
                columns: new[] { "Id", "Active", "Code", "Name", "Order" },
                values: new object[] { 3, true, "BILLER_LIABILITY", "Biller Liability", 3 });

            migrationBuilder.InsertData(
                table: "JournalLineTypeTranslationEntity",
                columns: new[] { "Id", "EntityId", "LanguageCode", "Name" },
                values: new object[] { 3, 3, "fr", "Responsabilité du facturier" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "JournalLineTypeTranslationEntity",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "JournalLineTypes",
                keyColumn: "Id",
                keyValue: 3);
        }
    }
}
