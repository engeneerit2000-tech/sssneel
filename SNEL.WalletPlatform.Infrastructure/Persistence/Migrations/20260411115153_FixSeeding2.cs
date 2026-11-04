using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixSeeding2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "JournalLineTypes",
                columns: new[] { "Id", "Active", "Code", "Name", "Order" },
                values: new object[,]
                {
                    { 14, true, "TOPUP_SETTLEMENT", "Payment Commission", 14 },
                    { 15, true, "PAYMENT_SETTLEMENT", "Payment Commission Tax", 15 }
                });

            migrationBuilder.InsertData(
                table: "JournalLineTypeTranslationEntity",
                columns: new[] { "Id", "EntityId", "LanguageCode", "Name" },
                values: new object[,]
                {
                    { 14, 14, "fr", "Règlement de recharge" },
                    { 15, 15, "fr", "Règlement de paiement" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "JournalLineTypeTranslationEntity",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "JournalLineTypeTranslationEntity",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "JournalLineTypes",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "JournalLineTypes",
                keyColumn: "Id",
                keyValue: 15);
        }
    }
}
