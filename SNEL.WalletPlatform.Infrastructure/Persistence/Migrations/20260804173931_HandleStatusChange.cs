using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class HandleStatusChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "NeedsReview",
                table: "Payments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "StatusChanged",
                table: "Payments",
                type: "timestamp with time zone",
                nullable: true);

            //migrationBuilder.CreateTable(
            //    name: "AfrimonyWebhookEvents",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uuid", nullable: false),
            //        MmTransactionId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
            //        ExternalId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
            //        TxnStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
            //        Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
            //        SenderMsisdn = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
            //        Status = table.Column<string>(type: "text", nullable: false),
            //        CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
            //        UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
            //        CreatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
            //        UpdatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_AfrimonyWebhookEvents", x => x.Id);
            //    });

            //migrationBuilder.CreateIndex(
            //    name: "IX_AfrimonyWebhookEvents_MmTransactionId",
            //    table: "AfrimonyWebhookEvents",
            //    column: "MmTransactionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropTable(
            //    name: "AfrimonyWebhookEvents");

            migrationBuilder.DropColumn(
                name: "NeedsReview",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "StatusChanged",
                table: "Payments");
        }
    }
}
