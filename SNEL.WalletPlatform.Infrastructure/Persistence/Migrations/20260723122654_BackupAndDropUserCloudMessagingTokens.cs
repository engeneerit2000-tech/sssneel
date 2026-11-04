using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class BackupAndDropUserCloudMessagingTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Safety backup before drop. Tokens now live in Notification Service.
            // Developers re-register via Wallet POST /api/v1/auth/cm-token after deploy.
            migrationBuilder.Sql("""
                CREATE TABLE temp_old_user_cloud_messaging_tokens
                AS SELECT * FROM "UserCloudMessagingTokens";
                """);

            migrationBuilder.DropTable(
                name: "UserCloudMessagingTokens");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserCloudMessagingTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    DeviceName = table.Column<string>(type: "text", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PlatformType = table.Column<int>(type: "integer", nullable: false),
                    TokenId = table.Column<string>(type: "text", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCloudMessagingTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserCloudMessagingTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserCloudMessagingTokens_ExpiresAt",
                table: "UserCloudMessagingTokens",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_UserCloudMessagingTokens_TokenId",
                table: "UserCloudMessagingTokens",
                column: "TokenId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCloudMessagingTokens_UserId",
                table: "UserCloudMessagingTokens",
                column: "UserId");
        }
    }
}
