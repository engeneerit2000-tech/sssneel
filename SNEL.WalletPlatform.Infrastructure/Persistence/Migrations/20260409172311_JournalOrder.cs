using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class JournalOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "Journals",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "JournalLines",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Order",
                table: "Journals");

            migrationBuilder.DropColumn(
                name: "Order",
                table: "JournalLines");
        }
    }
}
