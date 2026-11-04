using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class JournalTypesSeeding2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "JournalLineTypeTranslationEntity",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "Dette envers le client");

            migrationBuilder.UpdateData(
                table: "JournalLineTypeTranslationEntity",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Dette envers le prestataire de paiement");

            migrationBuilder.UpdateData(
                table: "JournalLineTypeTranslationEntity",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "Dette envers le facturier");

            migrationBuilder.InsertData(
                table: "JournalLineTypes",
                columns: new[] { "Id", "Active", "Code", "Name", "Order" },
                values: new object[,]
                {
                    { 4, true, "TOPUP_FEES", "Topup Fees", 4 },
                    { 5, true, "TOPUP_FEES_TAX", "Topup Fees", 5 },
                    { 6, true, "TOPUP_COST", "Topup Cost", 6 },
                    { 7, true, "TOPUP_COST_TAX", "Topup Cost Tax", 7 },
                    { 8, true, "PAYMENT_FEES", "Payment Fees", 8 },
                    { 9, true, "PAYMENT_FEES_TAX", "Payment Fees Tax", 9 },
                    { 10, true, "PAYMENT_COST", "Payment Cost", 10 },
                    { 11, true, "PAYMENT_COST_TAX", "Payment Cost Tax", 11 },
                    { 12, true, "PAYMENT_COMMISSION", "Payment Commission", 12 },
                    { 13, true, "PAYMENT_COMMISSION_TAX", "Payment Commission Tax", 13 }
                });

            migrationBuilder.UpdateData(
                table: "JournalTypeTranslationEntity",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "Recharge");

            migrationBuilder.UpdateData(
                table: "JournalTypeTranslationEntity",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Règlement de recharge");

            migrationBuilder.UpdateData(
                table: "JournalTypeTranslationEntity",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "Paiement");

            migrationBuilder.UpdateData(
                table: "JournalTypeTranslationEntity",
                keyColumn: "Id",
                keyValue: 4,
                column: "Name",
                value: "Règlement de paiement");

            migrationBuilder.UpdateData(
                table: "JournalTypes",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Code", "Name" },
                values: new object[] { "TOPUP", "Topup" });

            migrationBuilder.UpdateData(
                table: "JournalTypes",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Code", "Name" },
                values: new object[] { "TOPUP_SETTLEMENT", "Topup Settlement" });

            migrationBuilder.UpdateData(
                table: "JournalTypes",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Code", "Name" },
                values: new object[] { "PAYMENT", "Payment" });

            migrationBuilder.UpdateData(
                table: "JournalTypes",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Code", "Name" },
                values: new object[] { "PAYMENT_SETTLEMENT", "Payment Settlment" });

            migrationBuilder.InsertData(
                table: "JournalTypes",
                columns: new[] { "Id", "Active", "Code", "Name", "Order" },
                values: new object[,]
                {
                    { 5, true, "TOPUP_FEES", "Topup Fees", 5 },
                    { 6, true, "TOPUP_FEES_TAX", "Topup Fees Tax", 6 },
                    { 7, true, "TOPUP_COST", "Topup Cost", 7 },
                    { 8, true, "TOPUP_COST_TAX", "Topup Cost Tax", 8 },
                    { 9, true, "TOPUP_COMMISSION", "Topup Commission", 9 },
                    { 10, true, "TOPUP_COMMISSION_TAX", "Topup Commission Tax", 10 },
                    { 11, true, "PAYMENT_FEES", "Payment Fees", 11 },
                    { 12, true, "PAYMENT_FEES_TAX", "Payment Fees Tax", 12 },
                    { 13, true, "PAYMENT_COST", "Payment Cost", 13 },
                    { 14, true, "PAYMENT_COST_TAX", "Payment Cost Tax", 14 },
                    { 15, true, "PAYMENT_COMMISSION", "Payment Commission", 15 },
                    { 16, true, "PAYMENT_COMMISSION_TAX", "Payment Commission Tax", 16 },
                    { 17, true, "OPENING_BALANCE", "Opening Balance", 17 }
                });

            migrationBuilder.InsertData(
                table: "JournalLineTypeTranslationEntity",
                columns: new[] { "Id", "EntityId", "LanguageCode", "Name" },
                values: new object[,]
                {
                    { 4, 4, "fr", "Frais de recharge" },
                    { 5, 5, "fr", "Frais de recharge" },
                    { 6, 6, "fr", "Coût de recharge" },
                    { 7, 7, "fr", "Taxe sur le coût de recharge" },
                    { 8, 8, "fr", "Frais de paiement" },
                    { 9, 9, "fr", "Taxe sur les frais de paiement" },
                    { 10, 10, "fr", "Coût de paiement" },
                    { 11, 11, "fr", "Taxe sur le coût de paiement" },
                    { 12, 12, "fr", "Commission de paiement" },
                    { 13, 13, "fr", "Taxe sur la commission de paiement" }
                });

            migrationBuilder.InsertData(
                table: "JournalTypeTranslationEntity",
                columns: new[] { "Id", "EntityId", "LanguageCode", "Name" },
                values: new object[,]
                {
                    { 5, 5, "fr", "Frais de recharge" },
                    { 6, 6, "fr", "Taxe sur les frais de recharge" },
                    { 7, 7, "fr", "Coût de recharge" },
                    { 8, 8, "fr", "Taxe sur le coût de recharge" },
                    { 9, 9, "fr", "Commission de recharge" },
                    { 10, 10, "fr", "Taxe sur la commission de recharge" },
                    { 11, 11, "fr", "Frais de paiement" },
                    { 12, 12, "fr", "Taxe sur les frais de paiement" },
                    { 13, 13, "fr", "Coût de paiement" },
                    { 14, 14, "fr", "Taxe sur le coût de paiement" },
                    { 15, 15, "fr", "Commission de paiement" },
                    { 16, 16, "fr", "Taxe sur la commission de paiement" },
                    { 17, 17, "fr", "Solde d’ouverture" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "JournalLineTypeTranslationEntity",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "JournalLineTypeTranslationEntity",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "JournalLineTypeTranslationEntity",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "JournalLineTypeTranslationEntity",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "JournalLineTypeTranslationEntity",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "JournalLineTypeTranslationEntity",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "JournalLineTypeTranslationEntity",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "JournalLineTypeTranslationEntity",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "JournalLineTypeTranslationEntity",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "JournalLineTypeTranslationEntity",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "JournalTypeTranslationEntity",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "JournalTypeTranslationEntity",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "JournalTypeTranslationEntity",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "JournalTypeTranslationEntity",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "JournalTypeTranslationEntity",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "JournalTypeTranslationEntity",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "JournalTypeTranslationEntity",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "JournalTypeTranslationEntity",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "JournalTypeTranslationEntity",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "JournalTypeTranslationEntity",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "JournalTypeTranslationEntity",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "JournalTypeTranslationEntity",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "JournalTypeTranslationEntity",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "JournalLineTypes",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "JournalLineTypes",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "JournalLineTypes",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "JournalLineTypes",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "JournalLineTypes",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "JournalLineTypes",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "JournalLineTypes",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "JournalLineTypes",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "JournalLineTypes",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "JournalLineTypes",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "JournalTypes",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "JournalTypes",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "JournalTypes",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "JournalTypes",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "JournalTypes",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "JournalTypes",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "JournalTypes",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "JournalTypes",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "JournalTypes",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "JournalTypes",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "JournalTypes",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "JournalTypes",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "JournalTypes",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.UpdateData(
                table: "JournalLineTypeTranslationEntity",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "Responsabilité du client");

            migrationBuilder.UpdateData(
                table: "JournalLineTypeTranslationEntity",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Responsabilité du prestataire de services de paiement");

            migrationBuilder.UpdateData(
                table: "JournalLineTypeTranslationEntity",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "Responsabilité du facturier");

            migrationBuilder.UpdateData(
                table: "JournalTypeTranslationEntity",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "Rechargement du portefeuille");

            migrationBuilder.UpdateData(
                table: "JournalTypeTranslationEntity",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Règlement du rechargement du portefeuille");

            migrationBuilder.UpdateData(
                table: "JournalTypeTranslationEntity",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "Paiement de facturier");

            migrationBuilder.UpdateData(
                table: "JournalTypeTranslationEntity",
                keyColumn: "Id",
                keyValue: 4,
                column: "Name",
                value: "Règlement du paiement de facturier");

            migrationBuilder.UpdateData(
                table: "JournalTypes",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Code", "Name" },
                values: new object[] { "WALLET_TOPUP", "Wallet Topup" });

            migrationBuilder.UpdateData(
                table: "JournalTypes",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Code", "Name" },
                values: new object[] { "WALLET_TOPUP_SETTLEMENT", "Wallet Topup Settlement" });

            migrationBuilder.UpdateData(
                table: "JournalTypes",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Code", "Name" },
                values: new object[] { "BILL_PAYMENT", "Biller Payment" });

            migrationBuilder.UpdateData(
                table: "JournalTypes",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Code", "Name" },
                values: new object[] { "BILLER_SETTLEMENT", "Biller Payment Settlment" });
        }
    }
}
