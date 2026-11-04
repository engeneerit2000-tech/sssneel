using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CompletionMode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompletionModeEntity",
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
                    table.PrimaryKey("PK_CompletionModeEntity", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CompletionModeTranslationEntity",
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
                    table.PrimaryKey("PK_CompletionModeTranslationEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompletionModeTranslationEntity_CompletionModeEntity_Entity~",
                        column: x => x.EntityId,
                        principalTable: "CompletionModeEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "CompletionModeEntity",
                columns: new[] { "Id", "Active", "Code", "Name", "Order" },
                values: new object[,]
                {
                    { 1, true, "IMMEDIATE", "Immediate", 1 },
                    { 2, true, "POLLING", "Polling", 2 },
                    { 3, true, "WEBHOOK", "Webhook", 3 }
                });

            migrationBuilder.InsertData(
                table: "CompletionModeTranslationEntity",
                columns: new[] { "Id", "EntityId", "LanguageCode", "Name" },
                values: new object[,]
                {
                    { 1, 1, "fr", "Immédiate" },
                    { 2, 2, "fr", "Polling" },
                    { 3, 3, "fr", "Webhook" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompletionModeEntity_Code",
                table: "CompletionModeEntity",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompletionModeEntity_Name",
                table: "CompletionModeEntity",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_CompletionModeTranslationEntity_EntityId_LanguageCode",
                table: "CompletionModeTranslationEntity",
                columns: new[] { "EntityId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompletionModeTranslationEntity_LanguageCode",
                table: "CompletionModeTranslationEntity",
                column: "LanguageCode");

            migrationBuilder.CreateIndex(
                name: "IX_CompletionModeTranslationEntity_Name",
                table: "CompletionModeTranslationEntity",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompletionModeTranslationEntity");

            migrationBuilder.DropTable(
                name: "CompletionModeEntity");
        }
    }
}
