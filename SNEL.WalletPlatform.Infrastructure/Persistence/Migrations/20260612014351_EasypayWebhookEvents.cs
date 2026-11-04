using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EasypayWebhookEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EasypayWebhookEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferenceId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TransactionId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Provider = table.Column<string>(type: "text", nullable: false),
                    Date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    WebhookStatus = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EasypayWebhookEvents", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "PaymentInitModeTranslations",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Côté client");

            migrationBuilder.UpdateData(
                table: "PaymentInitModeTranslations",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "Côté serveur");

            migrationBuilder.CreateIndex(
                name: "IX_EasypayWebhookEvents_ReferenceId",
                table: "EasypayWebhookEvents",
                column: "ReferenceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EasypayWebhookEvents");

            migrationBuilder.UpdateData(
                table: "PaymentInitModeTranslations",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "C�t� client");

            migrationBuilder.UpdateData(
                table: "PaymentInitModeTranslations",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "C�t� serveur");
        }
    }
}
