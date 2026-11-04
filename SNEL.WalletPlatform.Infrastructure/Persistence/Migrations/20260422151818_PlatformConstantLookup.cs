using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PlatformConstantLookup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlatformConstants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Group = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformConstants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlatformConstantTranslations",
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
                    table.PrimaryKey("PK_PlatformConstantTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlatformConstantTranslations_PlatformConstants_EntityId",
                        column: x => x.EntityId,
                        principalTable: "PlatformConstants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "PlatformConstants",
                columns: new[] { "Id", "Active", "Code", "Group", "Name", "Order", "Value" },
                values: new object[] { 1, true, "MOBILE_APP_VERSION", "SYSTEM", "Mobile App Version", 1, "0.0.17" });

            migrationBuilder.InsertData(
                table: "PlatformConstantTranslations",
                columns: new[] { "Id", "EntityId", "LanguageCode", "Name" },
                values: new object[] { 1, 1, "fr", "Mobile App Version" });

            migrationBuilder.CreateIndex(
                name: "IX_PlatformConstants_Group_Code",
                table: "PlatformConstants",
                columns: new[] { "Group", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlatformConstants_Name",
                table: "PlatformConstants",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformConstantTranslations_EntityId_LanguageCode",
                table: "PlatformConstantTranslations",
                columns: new[] { "EntityId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlatformConstantTranslations_LanguageCode",
                table: "PlatformConstantTranslations",
                column: "LanguageCode");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformConstantTranslations_Name",
                table: "PlatformConstantTranslations",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlatformConstantTranslations");

            migrationBuilder.DropTable(
                name: "PlatformConstants");
        }
    }
}
