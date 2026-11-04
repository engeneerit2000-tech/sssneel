using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AccountFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrencyId",
                table: "PaymentOrderItems",
                type: "integer",
                nullable: true);

            migrationBuilder.InsertData(
                table: "AccountTypes",
                columns: new[] { "Id", "Active", "Code", "Name", "Order" },
                values: new object[,]
                {
                    { 6, true, "TAX_PAYABLE", "Tax Payable", 6 },
                    { 7, true, "CUSTOMER_LIABILITY", "Customer Liability", 7 },
                    { 8, true, "PSP_LIABILITY", "PSP Liability", 8 },
                    { 9, true, "BILLER_LIABILITY", "Biller Liability", 9 },
                    { 10, true, "TOPUP_COST", "Topup Cost", 10 },
                    { 11, true, "PAYMENT_COST", "Payment Cost", 11 },
                    { 12, true, "TAX_EXPENSE", "Tax Expense", 12 },
                    { 13, true, "CASH", "CASH", 13 }
                });

            migrationBuilder.UpdateData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("0f3e0d8d-2748-4afd-966b-1c25a8981867"),
                column: "AccountTypeId",
                value: 6);

            migrationBuilder.UpdateData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("20391f85-3759-426b-8490-71b6d6d05df2"),
                column: "AccountTypeId",
                value: 6);

            migrationBuilder.UpdateData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("45eebeaf-e6f1-4934-9010-21d7c9ab69d3"),
                column: "AccountTypeId",
                value: 12);

            migrationBuilder.UpdateData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("5116ca27-8161-4a59-84de-eb824aacd5a1"),
                column: "AccountTypeId",
                value: 12);

            migrationBuilder.UpdateData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("703751f9-32ef-4460-b315-840353d05234"),
                column: "AccountTypeId",
                value: 11);

            migrationBuilder.UpdateData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("7638d5e8-1a70-459e-a918-0606ab1671ac"),
                column: "AccountTypeId",
                value: 11);

            migrationBuilder.UpdateData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("a60adf4a-fc8b-440f-a69a-0dc16b2c7ff4"),
                column: "AccountTypeId",
                value: 13);

            migrationBuilder.UpdateData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("ae7c6cc2-3566-460b-aca5-f35439dc874a"),
                column: "AccountTypeId",
                value: 10);

            migrationBuilder.UpdateData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("b63f912e-cdfa-482c-88a0-7a6deadb189a"),
                column: "AccountTypeId",
                value: 10);

            migrationBuilder.UpdateData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("da5d1565-6352-487e-b17c-e819c168616f"),
                column: "AccountTypeId",
                value: 13);

            migrationBuilder.InsertData(
                table: "AccountTypeTranslations",
                columns: new[] { "Id", "EntityId", "LanguageCode", "Name" },
                values: new object[,]
                {
                    { 6, 6, "fr", "Taxes à payer" },
                    { 7, 7, "fr", "Dette envers le client" },
                    { 8, 8, "fr", "Dette envers le prestataire de paiement" },
                    { 9, 9, "fr", "Dette envers le facturier" },
                    { 10, 10, "fr", "Coût de recharge" },
                    { 11, 11, "fr", "Coût de paiement" },
                    { 12, 12, "fr", "Charge fiscale" },
                    { 13, 13, "fr", "Trésorerie" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentOrderItems_CurrencyId",
                table: "PaymentOrderItems",
                column: "CurrencyId");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentOrderItems_Currencies_CurrencyId",
                table: "PaymentOrderItems",
                column: "CurrencyId",
                principalTable: "Currencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentOrderItems_Currencies_CurrencyId",
                table: "PaymentOrderItems");

            migrationBuilder.DropIndex(
                name: "IX_PaymentOrderItems_CurrencyId",
                table: "PaymentOrderItems");

            migrationBuilder.DeleteData(
                table: "AccountTypeTranslations",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "AccountTypeTranslations",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "AccountTypeTranslations",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "AccountTypeTranslations",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "AccountTypeTranslations",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "AccountTypeTranslations",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "AccountTypeTranslations",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "AccountTypeTranslations",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "AccountTypes",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "AccountTypes",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "AccountTypes",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "AccountTypes",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "AccountTypes",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "AccountTypes",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "AccountTypes",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "AccountTypes",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DropColumn(
                name: "CurrencyId",
                table: "PaymentOrderItems");

            migrationBuilder.UpdateData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("0f3e0d8d-2748-4afd-966b-1c25a8981867"),
                column: "AccountTypeId",
                value: 3);

            migrationBuilder.UpdateData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("20391f85-3759-426b-8490-71b6d6d05df2"),
                column: "AccountTypeId",
                value: 3);

            migrationBuilder.UpdateData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("45eebeaf-e6f1-4934-9010-21d7c9ab69d3"),
                column: "AccountTypeId",
                value: 2);

            migrationBuilder.UpdateData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("5116ca27-8161-4a59-84de-eb824aacd5a1"),
                column: "AccountTypeId",
                value: 2);

            migrationBuilder.UpdateData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("703751f9-32ef-4460-b315-840353d05234"),
                column: "AccountTypeId",
                value: 2);

            migrationBuilder.UpdateData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("7638d5e8-1a70-459e-a918-0606ab1671ac"),
                column: "AccountTypeId",
                value: 2);

            migrationBuilder.UpdateData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("a60adf4a-fc8b-440f-a69a-0dc16b2c7ff4"),
                column: "AccountTypeId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("ae7c6cc2-3566-460b-aca5-f35439dc874a"),
                column: "AccountTypeId",
                value: 2);

            migrationBuilder.UpdateData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("b63f912e-cdfa-482c-88a0-7a6deadb189a"),
                column: "AccountTypeId",
                value: 2);

            migrationBuilder.UpdateData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("da5d1565-6352-487e-b17c-e819c168616f"),
                column: "AccountTypeId",
                value: 1);
        }
    }
}
