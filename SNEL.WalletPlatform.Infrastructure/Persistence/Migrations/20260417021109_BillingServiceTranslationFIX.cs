using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class BillingServiceTranslationFIX : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BillingServiceTranslationEntity_BillingServices_EntityId",
                table: "BillingServiceTranslationEntity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BillingServiceTranslationEntity",
                table: "BillingServiceTranslationEntity");

            migrationBuilder.RenameTable(
                name: "BillingServiceTranslationEntity",
                newName: "BillingServiceTranslations");

            migrationBuilder.RenameIndex(
                name: "IX_BillingServiceTranslationEntity_Name",
                table: "BillingServiceTranslations",
                newName: "IX_BillingServiceTranslations_Name");

            migrationBuilder.RenameIndex(
                name: "IX_BillingServiceTranslationEntity_LanguageCode",
                table: "BillingServiceTranslations",
                newName: "IX_BillingServiceTranslations_LanguageCode");

            migrationBuilder.RenameIndex(
                name: "IX_BillingServiceTranslationEntity_EntityId_LanguageCode",
                table: "BillingServiceTranslations",
                newName: "IX_BillingServiceTranslations_EntityId_LanguageCode");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BillingServiceTranslations",
                table: "BillingServiceTranslations",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BillingServiceTranslations_BillingServices_EntityId",
                table: "BillingServiceTranslations",
                column: "EntityId",
                principalTable: "BillingServices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BillingServiceTranslations_BillingServices_EntityId",
                table: "BillingServiceTranslations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BillingServiceTranslations",
                table: "BillingServiceTranslations");

            migrationBuilder.RenameTable(
                name: "BillingServiceTranslations",
                newName: "BillingServiceTranslationEntity");

            migrationBuilder.RenameIndex(
                name: "IX_BillingServiceTranslations_Name",
                table: "BillingServiceTranslationEntity",
                newName: "IX_BillingServiceTranslationEntity_Name");

            migrationBuilder.RenameIndex(
                name: "IX_BillingServiceTranslations_LanguageCode",
                table: "BillingServiceTranslationEntity",
                newName: "IX_BillingServiceTranslationEntity_LanguageCode");

            migrationBuilder.RenameIndex(
                name: "IX_BillingServiceTranslations_EntityId_LanguageCode",
                table: "BillingServiceTranslationEntity",
                newName: "IX_BillingServiceTranslationEntity_EntityId_LanguageCode");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BillingServiceTranslationEntity",
                table: "BillingServiceTranslationEntity",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BillingServiceTranslationEntity_BillingServices_EntityId",
                table: "BillingServiceTranslationEntity",
                column: "EntityId",
                principalTable: "BillingServices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
