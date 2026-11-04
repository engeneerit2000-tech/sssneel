using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCentersLookup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Centers",
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
                    table.PrimaryKey("PK_Centers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CenterTranslations",
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
                    table.PrimaryKey("PK_CenterTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CenterTranslations_Centers_EntityId",
                        column: x => x.EntityId,
                        principalTable: "Centers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Centers",
                columns: new[] { "Id", "Active", "Code", "Name", "Order" },
                values: new object[] { 1, true, "06", "Kinshasa-East", 1 });

            migrationBuilder.InsertData(
                table: "CenterTranslations",
                columns: new[] { "Id", "EntityId", "LanguageCode", "Name" },
                values: new object[] { 1, 1, "fr", "Kinshasa-Est" });

            migrationBuilder.CreateIndex(
                name: "IX_Centers_Code",
                table: "Centers",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Centers_Name",
                table: "Centers",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_CenterTranslations_EntityId_LanguageCode",
                table: "CenterTranslations",
                columns: new[] { "EntityId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CenterTranslations_LanguageCode",
                table: "CenterTranslations",
                column: "LanguageCode");

            migrationBuilder.CreateIndex(
                name: "IX_CenterTranslations_Name",
                table: "CenterTranslations",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CenterTranslations");

            migrationBuilder.DropTable(
                name: "Centers");
        }
    }
}
