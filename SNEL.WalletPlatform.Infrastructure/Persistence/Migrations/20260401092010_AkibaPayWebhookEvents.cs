using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AkibaPayWebhookEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AkibaPayWebhookEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AkibaReference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    MerchantReference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TransactionStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ToNumberTelecomReference = table.Column<string>(type: "text", nullable: true),
                    FromNumberTelecomReference = table.Column<string>(type: "text", nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    ToNumber = table.Column<string>(type: "text", nullable: true),
                    FromNumber = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CallbackGeneratedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AkibaPayWebhookEvents", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AkibaPayWebhookEvents_AkibaReference",
                table: "AkibaPayWebhookEvents",
                column: "AkibaReference");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AkibaPayWebhookEvents");
        }
    }
}
