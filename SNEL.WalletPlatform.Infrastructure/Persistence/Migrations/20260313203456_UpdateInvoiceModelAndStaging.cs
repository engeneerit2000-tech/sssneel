using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateInvoiceModelAndStaging : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AmountExcludingTaxFc",
                table: "InvoiceStagings",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AmountExcludingTaxUsd",
                table: "InvoiceStagings",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BillingMonth",
                table: "InvoiceStagings",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BillingYear",
                table: "InvoiceStagings",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CalculationCoefficient",
                table: "InvoiceStagings",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CenterDesignation",
                table: "InvoiceStagings",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClientCommune",
                table: "InvoiceStagings",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClientPropertyNumber",
                table: "InvoiceStagings",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClientStreet",
                table: "InvoiceStagings",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CurrentBillingIndex",
                table: "InvoiceStagings",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountCode",
                table: "InvoiceStagings",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MeterNumber",
                table: "InvoiceStagings",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Pa",
                table: "InvoiceStagings",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PointOfSaleName",
                table: "InvoiceStagings",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PreviousBillingIndex",
                table: "InvoiceStagings",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubstationCode",
                table: "InvoiceStagings",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SubtotalFc",
                table: "InvoiceStagings",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SubtotalUsd",
                table: "InvoiceStagings",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OriginCustomerId",
                table: "Invoices",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AmountExcludingTaxFc",
                table: "InvoiceStagings");

            migrationBuilder.DropColumn(
                name: "AmountExcludingTaxUsd",
                table: "InvoiceStagings");

            migrationBuilder.DropColumn(
                name: "BillingMonth",
                table: "InvoiceStagings");

            migrationBuilder.DropColumn(
                name: "BillingYear",
                table: "InvoiceStagings");

            migrationBuilder.DropColumn(
                name: "CalculationCoefficient",
                table: "InvoiceStagings");

            migrationBuilder.DropColumn(
                name: "CenterDesignation",
                table: "InvoiceStagings");

            migrationBuilder.DropColumn(
                name: "ClientCommune",
                table: "InvoiceStagings");

            migrationBuilder.DropColumn(
                name: "ClientPropertyNumber",
                table: "InvoiceStagings");

            migrationBuilder.DropColumn(
                name: "ClientStreet",
                table: "InvoiceStagings");

            migrationBuilder.DropColumn(
                name: "CurrentBillingIndex",
                table: "InvoiceStagings");

            migrationBuilder.DropColumn(
                name: "DiscountCode",
                table: "InvoiceStagings");

            migrationBuilder.DropColumn(
                name: "MeterNumber",
                table: "InvoiceStagings");

            migrationBuilder.DropColumn(
                name: "Pa",
                table: "InvoiceStagings");

            migrationBuilder.DropColumn(
                name: "PointOfSaleName",
                table: "InvoiceStagings");

            migrationBuilder.DropColumn(
                name: "PreviousBillingIndex",
                table: "InvoiceStagings");

            migrationBuilder.DropColumn(
                name: "SubstationCode",
                table: "InvoiceStagings");

            migrationBuilder.DropColumn(
                name: "SubtotalFc",
                table: "InvoiceStagings");

            migrationBuilder.DropColumn(
                name: "SubtotalUsd",
                table: "InvoiceStagings");

            migrationBuilder.DropColumn(
                name: "OriginCustomerId",
                table: "Invoices");
        }
    }
}
