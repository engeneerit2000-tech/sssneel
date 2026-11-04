using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddJobkeyAndSourceSystemToInvoiceStaging : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "JobKey",
                table: "InvoiceStagings",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceSystem",
                table: "InvoiceStagings",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceStagings_JobKey_SourceSystem",
                table: "InvoiceStagings",
                columns: new[] { "JobKey", "SourceSystem" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InvoiceStagings_JobKey_SourceSystem",
                table: "InvoiceStagings");

            migrationBuilder.DropColumn(
                name: "JobKey",
                table: "InvoiceStagings");

            migrationBuilder.DropColumn(
                name: "SourceSystem",
                table: "InvoiceStagings");
        }
    }
}
