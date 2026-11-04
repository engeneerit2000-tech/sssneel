using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCampaignAudienceGroupCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AudienceGroupCode",
                table: "Campaigns",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "ALL_CUSTOMERS");

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_AudienceGroupCode",
                table: "Campaigns",
                column: "AudienceGroupCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Campaigns_AudienceGroupCode",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "AudienceGroupCode",
                table: "Campaigns");
        }
    }
}
