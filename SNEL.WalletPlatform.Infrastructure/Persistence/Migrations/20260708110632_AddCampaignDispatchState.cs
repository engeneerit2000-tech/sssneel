using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCampaignDispatchState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LastProcessedCustomerId",
                table: "Campaigns",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "NotificationsCompletedAtUtc",
                table: "Campaigns",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "NotificationsStartedAtUtc",
                table: "Campaigns",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Campaigns",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_Status",
                table: "Campaigns",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Campaigns_Status",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "LastProcessedCustomerId",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "NotificationsCompletedAtUtc",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "NotificationsStartedAtUtc",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Campaigns");
        }
    }
}
