using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class BSC_PMC_ACCOUNTING : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlatformShareType",
                table: "PaymentMethodCurrencies");

            migrationBuilder.DropColumn(
                name: "PlatformShareValue",
                table: "PaymentMethodCurrencies");

            migrationBuilder.DropColumn(
                name: "PlatformShareType",
                table: "BillingServiceCurrencies");

            migrationBuilder.RenameColumn(
                name: "PlatformShareValue",
                table: "BillingServiceCurrencies",
                newName: "VAT");

            migrationBuilder.AlterColumn<decimal>(
                name: "Min",
                table: "PaymentMethodCurrencies",
                type: "numeric(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Max",
                table: "PaymentMethodCurrencies",
                type: "numeric(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "FeesValue",
                table: "PaymentMethodCurrencies",
                type: "numeric(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FeesType",
                table: "PaymentMethodCurrencies",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "CostValue",
                table: "PaymentMethodCurrencies",
                type: "numeric(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CostType",
                table: "PaymentMethodCurrencies",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AutoSettleJournal",
                table: "PaymentMethodCurrencies",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "BankAccountId",
                table: "PaymentMethodCurrencies",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Scale",
                table: "PaymentMethodCurrencies",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "VAT",
                table: "PaymentMethodCurrencies",
                type: "numeric(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "Min",
                table: "BillingServiceCurrencies",
                type: "numeric(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "Max",
                table: "BillingServiceCurrencies",
                type: "numeric(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<string>(
                name: "FeesType",
                table: "BillingServiceCurrencies",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "CommissionType",
                table: "BillingServiceCurrencies",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<bool>(
                name: "AutoSettleJournal",
                table: "BillingServiceCurrencies",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "BankAccountId",
                table: "BillingServiceCurrencies",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "CostType",
                table: "BillingServiceCurrencies",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "CostValue",
                table: "BillingServiceCurrencies",
                type: "numeric(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Scale",
                table: "BillingServiceCurrencies",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AutoSettleJournal",
                table: "PaymentMethodCurrencies");

            migrationBuilder.DropColumn(
                name: "BankAccountId",
                table: "PaymentMethodCurrencies");

            migrationBuilder.DropColumn(
                name: "Scale",
                table: "PaymentMethodCurrencies");

            migrationBuilder.DropColumn(
                name: "VAT",
                table: "PaymentMethodCurrencies");

            migrationBuilder.DropColumn(
                name: "AutoSettleJournal",
                table: "BillingServiceCurrencies");

            migrationBuilder.DropColumn(
                name: "BankAccountId",
                table: "BillingServiceCurrencies");

            migrationBuilder.DropColumn(
                name: "CostType",
                table: "BillingServiceCurrencies");

            migrationBuilder.DropColumn(
                name: "CostValue",
                table: "BillingServiceCurrencies");

            migrationBuilder.DropColumn(
                name: "Scale",
                table: "BillingServiceCurrencies");

            migrationBuilder.RenameColumn(
                name: "VAT",
                table: "BillingServiceCurrencies",
                newName: "PlatformShareValue");

            migrationBuilder.AlterColumn<decimal>(
                name: "Min",
                table: "PaymentMethodCurrencies",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,6)",
                oldPrecision: 18,
                oldScale: 6);

            migrationBuilder.AlterColumn<decimal>(
                name: "Max",
                table: "PaymentMethodCurrencies",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,6)",
                oldPrecision: 18,
                oldScale: 6);

            migrationBuilder.AlterColumn<decimal>(
                name: "FeesValue",
                table: "PaymentMethodCurrencies",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,6)",
                oldPrecision: 18,
                oldScale: 6);

            migrationBuilder.AlterColumn<int>(
                name: "FeesType",
                table: "PaymentMethodCurrencies",
                type: "integer",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<decimal>(
                name: "CostValue",
                table: "PaymentMethodCurrencies",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,6)",
                oldPrecision: 18,
                oldScale: 6);

            migrationBuilder.AlterColumn<int>(
                name: "CostType",
                table: "PaymentMethodCurrencies",
                type: "integer",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<int>(
                name: "PlatformShareType",
                table: "PaymentMethodCurrencies",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PlatformShareValue",
                table: "PaymentMethodCurrencies",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Min",
                table: "BillingServiceCurrencies",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,6)",
                oldPrecision: 18,
                oldScale: 6);

            migrationBuilder.AlterColumn<decimal>(
                name: "Max",
                table: "BillingServiceCurrencies",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,6)",
                oldPrecision: 18,
                oldScale: 6);

            migrationBuilder.AlterColumn<string>(
                name: "FeesType",
                table: "BillingServiceCurrencies",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "CommissionType",
                table: "BillingServiceCurrencies",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "PlatformShareType",
                table: "BillingServiceCurrencies",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }
    }
}
