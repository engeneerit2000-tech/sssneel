using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class BillerCustomerIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BillerCustomers_BillerId_CustomerId",
                table: "BillerCustomers");

            migrationBuilder.CreateIndex(
                name: "IX_BillerCustomers_BillerId_CustomerId_Number",
                table: "BillerCustomers",
                columns: new[] { "BillerId", "CustomerId", "Number" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BillerCustomers_BillerId_CustomerId_Number",
                table: "BillerCustomers");

            migrationBuilder.CreateIndex(
                name: "IX_BillerCustomers_BillerId_CustomerId",
                table: "BillerCustomers",
                columns: new[] { "BillerId", "CustomerId" },
                unique: true);
        }
    }
}
