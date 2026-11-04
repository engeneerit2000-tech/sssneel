using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SNEL.WalletPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceSecondaryStagingApplyIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InvoiceSecondaryStagings_JobKey",
                table: "InvoiceSecondaryStagings");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceSecondaryStagings_JobKey_Updated_Id",
                table: "InvoiceSecondaryStagings",
                columns: new[] { "JobKey", "Updated", "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InvoiceSecondaryStagings_JobKey_Updated_Id",
                table: "InvoiceSecondaryStagings");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceSecondaryStagings_JobKey",
                table: "InvoiceSecondaryStagings",
                column: "JobKey");
        }
    }
}
