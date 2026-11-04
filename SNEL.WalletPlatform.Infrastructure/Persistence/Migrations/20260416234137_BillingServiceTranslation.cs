using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class BillingServiceTranslation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BillingServiceTranslationEntity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EntityId = table.Column<long>(type: "bigint", nullable: false),
                    LanguageCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillingServiceTranslationEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BillingServiceTranslationEntity_BillingServices_EntityId",
                        column: x => x.EntityId,
                        principalTable: "BillingServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BillingServiceTranslationEntity_EntityId_LanguageCode",
                table: "BillingServiceTranslationEntity",
                columns: new[] { "EntityId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BillingServiceTranslationEntity_LanguageCode",
                table: "BillingServiceTranslationEntity",
                column: "LanguageCode");

            migrationBuilder.CreateIndex(
                name: "IX_BillingServiceTranslationEntity_Name",
                table: "BillingServiceTranslationEntity",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BillingServiceTranslationEntity");
        }
    }
}
