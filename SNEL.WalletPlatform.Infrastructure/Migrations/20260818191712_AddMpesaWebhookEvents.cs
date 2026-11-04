using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SNEL.WalletPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMpesaWebhookEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MpesaWebhookEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginalConversationId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ThirdPartyConversationId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TransactionId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ResultCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ResultDescription = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MpesaWebhookEvents", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MpesaWebhookEvents_ThirdPartyConversationId",
                table: "MpesaWebhookEvents",
                column: "ThirdPartyConversationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MpesaWebhookEvents");
        }
    }
}
