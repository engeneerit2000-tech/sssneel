using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class invoice_staging_refactor_1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InvoiceInfo",
                table: "InvoiceStagings");

            migrationBuilder.DropColumn(
                name: "PaymentsJson",
                table: "InvoiceStagings");

            migrationBuilder.RenameColumn(
                name: "TarifType",
                table: "InvoiceStagings",
                newName: "TariffType");

            migrationBuilder.RenameColumn(
                name: "InvoiceCurr",
                table: "InvoiceStagings",
                newName: "InvoiceCurrencyCode");

            migrationBuilder.RenameColumn(
                name: "BilledConsuption",
                table: "InvoiceStagings",
                newName: "BilledConsumption");

            migrationBuilder.AddColumn<DateTime>(
                name: "InvoiceUpdateDate",
                table: "Invoices",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InvoiceUpdateDate",
                table: "Invoices");

            migrationBuilder.RenameColumn(
                name: "TariffType",
                table: "InvoiceStagings",
                newName: "TarifType");

            migrationBuilder.RenameColumn(
                name: "InvoiceCurrencyCode",
                table: "InvoiceStagings",
                newName: "InvoiceCurr");

            migrationBuilder.RenameColumn(
                name: "BilledConsumption",
                table: "InvoiceStagings",
                newName: "BilledConsuption");

            migrationBuilder.AddColumn<string>(
                name: "InvoiceInfo",
                table: "InvoiceStagings",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentsJson",
                table: "InvoiceStagings",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);
        }
    }
}
