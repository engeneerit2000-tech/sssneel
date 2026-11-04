using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SNEL.WalletPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class webhookrefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InterswitchWebhookEvents_Event",
                table: "InterswitchWebhookEvents");

            migrationBuilder.DropIndex(
                name: "IX_InterswitchWebhookEvents_Uuid",
                table: "InterswitchWebhookEvents");

            migrationBuilder.RenameColumn(
                name: "DataJson",
                table: "InterswitchWebhookEvents",
                newName: "SplitAccounts");

            migrationBuilder.AddColumn<string>(
                name: "AccountNumber",
                table: "InterswitchWebhookEvents",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                table: "InterswitchWebhookEvents",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "BankCode",
                table: "InterswitchWebhookEvents",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CardNumber",
                table: "InterswitchWebhookEvents",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Channel",
                table: "InterswitchWebhookEvents",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CurrencyCode",
                table: "InterswitchWebhookEvents",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "Escrow",
                table: "InterswitchWebhookEvents",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MerchantCustomerId",
                table: "InterswitchWebhookEvents",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MerchantCustomerName",
                table: "InterswitchWebhookEvents",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MerchantReference",
                table: "InterswitchWebhookEvents",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "PaymentId",
                table: "InterswitchWebhookEvents",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "PaymentReference",
                table: "InterswitchWebhookEvents",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "RemittanceAmount",
                table: "InterswitchWebhookEvents",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ResponseCode",
                table: "InterswitchWebhookEvents",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ResponseDescription",
                table: "InterswitchWebhookEvents",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RetrievalReferenceNumber",
                table: "InterswitchWebhookEvents",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "InterswitchWebhookEvents",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Token",
                table: "InterswitchWebhookEvents",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "TransactionDate",
                table: "InterswitchWebhookEvents",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountNumber",
                table: "InterswitchWebhookEvents");

            migrationBuilder.DropColumn(
                name: "Amount",
                table: "InterswitchWebhookEvents");

            migrationBuilder.DropColumn(
                name: "BankCode",
                table: "InterswitchWebhookEvents");

            migrationBuilder.DropColumn(
                name: "CardNumber",
                table: "InterswitchWebhookEvents");

            migrationBuilder.DropColumn(
                name: "Channel",
                table: "InterswitchWebhookEvents");

            migrationBuilder.DropColumn(
                name: "CurrencyCode",
                table: "InterswitchWebhookEvents");

            migrationBuilder.DropColumn(
                name: "Escrow",
                table: "InterswitchWebhookEvents");

            migrationBuilder.DropColumn(
                name: "MerchantCustomerId",
                table: "InterswitchWebhookEvents");

            migrationBuilder.DropColumn(
                name: "MerchantCustomerName",
                table: "InterswitchWebhookEvents");

            migrationBuilder.DropColumn(
                name: "MerchantReference",
                table: "InterswitchWebhookEvents");

            migrationBuilder.DropColumn(
                name: "PaymentId",
                table: "InterswitchWebhookEvents");

            migrationBuilder.DropColumn(
                name: "PaymentReference",
                table: "InterswitchWebhookEvents");

            migrationBuilder.DropColumn(
                name: "RemittanceAmount",
                table: "InterswitchWebhookEvents");

            migrationBuilder.DropColumn(
                name: "ResponseCode",
                table: "InterswitchWebhookEvents");

            migrationBuilder.DropColumn(
                name: "ResponseDescription",
                table: "InterswitchWebhookEvents");

            migrationBuilder.DropColumn(
                name: "RetrievalReferenceNumber",
                table: "InterswitchWebhookEvents");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "InterswitchWebhookEvents");

            migrationBuilder.DropColumn(
                name: "Token",
                table: "InterswitchWebhookEvents");

            migrationBuilder.DropColumn(
                name: "TransactionDate",
                table: "InterswitchWebhookEvents");

            migrationBuilder.RenameColumn(
                name: "SplitAccounts",
                table: "InterswitchWebhookEvents",
                newName: "DataJson");

            migrationBuilder.CreateIndex(
                name: "IX_InterswitchWebhookEvents_Event",
                table: "InterswitchWebhookEvents",
                column: "Event");

            migrationBuilder.CreateIndex(
                name: "IX_InterswitchWebhookEvents_Uuid",
                table: "InterswitchWebhookEvents",
                column: "Uuid",
                unique: true);
        }
    }
}
