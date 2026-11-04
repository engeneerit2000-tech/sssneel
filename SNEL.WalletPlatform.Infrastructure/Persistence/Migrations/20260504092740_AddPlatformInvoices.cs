using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPlatformInvoices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "PlatformInvoiceId",
                table: "JournalLines",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PlatformInvoices",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PartnerId = table.Column<long>(type: "bigint", nullable: false),
                    CurrencyId = table.Column<int>(type: "integer", nullable: false),
                    DescriptionTemplateKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    DescriptionParametersJson = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    InvoiceDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    Tax = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformInvoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlatformInvoices_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlatformInvoices_Partners_PartnerId",
                        column: x => x.PartnerId,
                        principalTable: "Partners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_JournalLines_PlatformInvoiceId",
                table: "JournalLines",
                column: "PlatformInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformInvoices_CurrencyId",
                table: "PlatformInvoices",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformInvoices_PartnerId",
                table: "PlatformInvoices",
                column: "PartnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_JournalLines_PlatformInvoices_PlatformInvoiceId",
                table: "JournalLines",
                column: "PlatformInvoiceId",
                principalTable: "PlatformInvoices",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JournalLines_PlatformInvoices_PlatformInvoiceId",
                table: "JournalLines");

            migrationBuilder.DropTable(
                name: "PlatformInvoices");

            migrationBuilder.DropIndex(
                name: "IX_JournalLines_PlatformInvoiceId",
                table: "JournalLines");

            migrationBuilder.DropColumn(
                name: "PlatformInvoiceId",
                table: "JournalLines");

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
        }
    }
}
