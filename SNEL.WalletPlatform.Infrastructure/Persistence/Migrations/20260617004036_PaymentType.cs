using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PaymentType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PaymentTypeId",
                table: "Payments",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PaymentTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentTypeTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EntityId = table.Column<int>(type: "integer", nullable: false),
                    LanguageCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentTypeTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentTypeTranslations_PaymentTypes_EntityId",
                        column: x => x.EntityId,
                        principalTable: "PaymentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "PaymentTypes",
                columns: new[] { "Id", "Active", "Code", "Name", "Order" },
                values: new object[,]
                {
                    { 1, true, "MOBILE_MONEY", "Mobile Money", 1 },
                    { 2, true, "BANK_CARD", "Bank Card", 2 }
                });

            migrationBuilder.InsertData(
                table: "PaymentTypeTranslations",
                columns: new[] { "Id", "EntityId", "LanguageCode", "Name" },
                values: new object[,]
                {
                    { 1, 1, "fr", "Argent Mobile" },
                    { 2, 2, "fr", "Carte Bancaire" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PaymentTypeId",
                table: "Payments",
                column: "PaymentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTypes_Code",
                table: "PaymentTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTypes_Name",
                table: "PaymentTypes",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTypeTranslations_EntityId_LanguageCode",
                table: "PaymentTypeTranslations",
                columns: new[] { "EntityId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTypeTranslations_LanguageCode",
                table: "PaymentTypeTranslations",
                column: "LanguageCode");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTypeTranslations_Name",
                table: "PaymentTypeTranslations",
                column: "Name");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_PaymentTypes_PaymentTypeId",
                table: "Payments",
                column: "PaymentTypeId",
                principalTable: "PaymentTypes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_PaymentTypes_PaymentTypeId",
                table: "Payments");

            migrationBuilder.DropTable(
                name: "PaymentTypeTranslations");

            migrationBuilder.DropTable(
                name: "PaymentTypes");

            migrationBuilder.DropIndex(
                name: "IX_Payments_PaymentTypeId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PaymentTypeId",
                table: "Payments");
        }
    }
}
