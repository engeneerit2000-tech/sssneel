using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOriginInvoiceColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InvoiceStagings_OracleInvoiceId",
                table: "InvoiceStagings");

            migrationBuilder.DropColumn(
                name: "OracleInvoiceId",
                table: "InvoiceStagings");

            migrationBuilder.AddColumn<long>(
                name: "OriginInvoiceId",
                table: "InvoiceStagings",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "OriginInvoiceId",
                table: "Invoices",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceStagings_OriginInvoiceId",
                table: "InvoiceStagings",
                column: "OriginInvoiceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InvoiceStagings_OriginInvoiceId",
                table: "InvoiceStagings");

            migrationBuilder.DropColumn(
                name: "OriginInvoiceId",
                table: "InvoiceStagings");

            migrationBuilder.DropColumn(
                name: "OriginInvoiceId",
                table: "Invoices");

            migrationBuilder.AddColumn<decimal>(
                name: "OracleInvoiceId",
                table: "InvoiceStagings",
                type: "numeric(18,0)",
                precision: 18,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceStagings_OracleInvoiceId",
                table: "InvoiceStagings",
                column: "OracleInvoiceId");
        }
    }
}
