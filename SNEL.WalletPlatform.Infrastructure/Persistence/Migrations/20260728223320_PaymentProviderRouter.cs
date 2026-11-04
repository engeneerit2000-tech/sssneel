using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PaymentProviderRouter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PaymentProviderRouters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'1000', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CountryCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    CompanyPrefix = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    PaymentProviderCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PaymentMethodCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentProviderRouters", x => x.Id);
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentProviderRouters");

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
