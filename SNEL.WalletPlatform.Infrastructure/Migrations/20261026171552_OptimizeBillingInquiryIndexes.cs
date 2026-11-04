using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SNEL.WalletPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class OptimizeBillingInquiryIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PaymentOrderItems_InvoiceId",
                table: "PaymentOrderItems");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentOrderItems_InvoiceId_ItemType_Status",
                table: "PaymentOrderItems",
                columns: new[] { "InvoiceId", "ItemType", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PaymentOrderItems_InvoiceId_ItemType_Status",
                table: "PaymentOrderItems");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentOrderItems_InvoiceId",
                table: "PaymentOrderItems",
                column: "InvoiceId");
        }
    }
}
