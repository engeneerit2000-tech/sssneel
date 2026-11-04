using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Seed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GenderTranslations",
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
                    table.PrimaryKey("PK_GenderTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GenderTranslations_Genders_EntityId",
                        column: x => x.EntityId,
                        principalTable: "Genders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "AccountTypeTranslations",
                keyColumn: "Id",
                keyValue: 1,
                column: "LanguageCode",
                value: "fr");

            migrationBuilder.UpdateData(
                table: "AccountTypeTranslations",
                keyColumn: "Id",
                keyValue: 2,
                column: "LanguageCode",
                value: "fr");

            migrationBuilder.UpdateData(
                table: "AccountTypeTranslations",
                keyColumn: "Id",
                keyValue: 3,
                column: "LanguageCode",
                value: "fr");

            migrationBuilder.UpdateData(
                table: "AccountTypeTranslations",
                keyColumn: "Id",
                keyValue: 4,
                column: "LanguageCode",
                value: "fr");

            migrationBuilder.InsertData(
                table: "GenderTranslations",
                columns: new[] { "Id", "EntityId", "LanguageCode", "Name" },
                values: new object[,]
                {
                    { 1, 1, "FR", "Homme" },
                    { 2, 2, "FR", "Femme" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_GenderTranslations_EntityId_LanguageCode",
                table: "GenderTranslations",
                columns: new[] { "EntityId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GenderTranslations_LanguageCode",
                table: "GenderTranslations",
                column: "LanguageCode");

            migrationBuilder.CreateIndex(
                name: "IX_GenderTranslations_Name",
                table: "GenderTranslations",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GenderTranslations");

            migrationBuilder.UpdateData(
                table: "AccountTypeTranslations",
                keyColumn: "Id",
                keyValue: 1,
                column: "LanguageCode",
                value: "FR");

            migrationBuilder.UpdateData(
                table: "AccountTypeTranslations",
                keyColumn: "Id",
                keyValue: 2,
                column: "LanguageCode",
                value: "FR");

            migrationBuilder.UpdateData(
                table: "AccountTypeTranslations",
                keyColumn: "Id",
                keyValue: 3,
                column: "LanguageCode",
                value: "FR");

            migrationBuilder.UpdateData(
                table: "AccountTypeTranslations",
                keyColumn: "Id",
                keyValue: 4,
                column: "LanguageCode",
                value: "FR");
        }
    }
}
