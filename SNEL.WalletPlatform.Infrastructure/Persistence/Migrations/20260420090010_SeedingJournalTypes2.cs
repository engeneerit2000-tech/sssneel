using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedingJournalTypes2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "JournalLineTypes",
                columns: new[] { "Id", "Active", "Code", "Name", "Order" },
                values: new object[,]
                {
                    { 16, true, "PAYMENT_VOUCHER", "Payment Voucher", 16 },
                    { 17, true, "RECEIPT_VOUCHER", "Receipt Voucher", 17 },
                    { 18, true, "CREDIT_NOTE", "Credit Note", 18 },
                    { 19, true, "DEBIT_NOTE", "Debit Note", 19 },
                    { 20, true, "MANUAL_JOURNAL", "Manual Journal", 20 }
                });

            migrationBuilder.InsertData(
                table: "JournalTypes",
                columns: new[] { "Id", "Active", "Code", "Name", "Order" },
                values: new object[,]
                {
                    { 18, true, "PAYMENT_VOUCHER", "Payment Voucher", 18 },
                    { 19, true, "RECEIPT_VOUCHER", "Receipt Voucher", 19 },
                    { 20, true, "CREDIT_NOTE", "Credit Note", 20 },
                    { 21, true, "DEBIT_NOTE", "Debit Note", 21 },
                    { 22, true, "MANUAL_JOURNAL", "Manual Journal", 22 }
                });

            migrationBuilder.InsertData(
                table: "JournalLineTypeTranslationEntity",
                columns: new[] { "Id", "EntityId", "LanguageCode", "Name" },
                values: new object[,]
                {
                    { 16, 16, "fr", "Pièce de paiement" },
                    { 17, 17, "fr", "Pièce d’encaissement" },
                    { 18, 18, "fr", "Avoir" },
                    { 19, 19, "fr", "Note de débit" },
                    { 20, 20, "fr", "Journal manuel" }
                });

            migrationBuilder.InsertData(
                table: "JournalTypeTranslationEntity",
                columns: new[] { "Id", "EntityId", "LanguageCode", "Name" },
                values: new object[,]
                {
                    { 18, 18, "fr", "Pièce de paiement" },
                    { 19, 19, "fr", "Pièce d’encaissement" },
                    { 20, 20, "fr", "Avoir" },
                    { 21, 21, "fr", "Note de débit" },
                    { 22, 22, "fr", "Journal manuel" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "JournalLineTypeTranslationEntity",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "JournalLineTypeTranslationEntity",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "JournalLineTypeTranslationEntity",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "JournalLineTypeTranslationEntity",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "JournalLineTypeTranslationEntity",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "JournalTypeTranslationEntity",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "JournalTypeTranslationEntity",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "JournalTypeTranslationEntity",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "JournalTypeTranslationEntity",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "JournalTypeTranslationEntity",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "JournalLineTypes",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "JournalLineTypes",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "JournalLineTypes",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "JournalLineTypes",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "JournalLineTypes",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "JournalTypes",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "JournalTypes",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "JournalTypes",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "JournalTypes",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "JournalTypes",
                keyColumn: "Id",
                keyValue: 22);
        }
    }
}
