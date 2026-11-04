using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixTranslationTableName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JournalLineTypeTranslationEntity_JournalLineTypes_EntityId",
                table: "JournalLineTypeTranslationEntity");

            migrationBuilder.DropForeignKey(
                name: "FK_JournalTypeTranslationEntity_JournalTypes_EntityId",
                table: "JournalTypeTranslationEntity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_JournalTypeTranslationEntity",
                table: "JournalTypeTranslationEntity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_JournalLineTypeTranslationEntity",
                table: "JournalLineTypeTranslationEntity");

            migrationBuilder.RenameTable(
                name: "JournalTypeTranslationEntity",
                newName: "JournalTypeTranslations");

            migrationBuilder.RenameTable(
                name: "JournalLineTypeTranslationEntity",
                newName: "JournalLineTypeTranslations");

            migrationBuilder.RenameIndex(
                name: "IX_JournalTypeTranslationEntity_Name",
                table: "JournalTypeTranslations",
                newName: "IX_JournalTypeTranslations_Name");

            migrationBuilder.RenameIndex(
                name: "IX_JournalTypeTranslationEntity_LanguageCode",
                table: "JournalTypeTranslations",
                newName: "IX_JournalTypeTranslations_LanguageCode");

            migrationBuilder.RenameIndex(
                name: "IX_JournalTypeTranslationEntity_EntityId_LanguageCode",
                table: "JournalTypeTranslations",
                newName: "IX_JournalTypeTranslations_EntityId_LanguageCode");

            migrationBuilder.RenameIndex(
                name: "IX_JournalLineTypeTranslationEntity_Name",
                table: "JournalLineTypeTranslations",
                newName: "IX_JournalLineTypeTranslations_Name");

            migrationBuilder.RenameIndex(
                name: "IX_JournalLineTypeTranslationEntity_LanguageCode",
                table: "JournalLineTypeTranslations",
                newName: "IX_JournalLineTypeTranslations_LanguageCode");

            migrationBuilder.RenameIndex(
                name: "IX_JournalLineTypeTranslationEntity_EntityId_LanguageCode",
                table: "JournalLineTypeTranslations",
                newName: "IX_JournalLineTypeTranslations_EntityId_LanguageCode");

            migrationBuilder.AddPrimaryKey(
                name: "PK_JournalTypeTranslations",
                table: "JournalTypeTranslations",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_JournalLineTypeTranslations",
                table: "JournalLineTypeTranslations",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_JournalLineTypeTranslations_JournalLineTypes_EntityId",
                table: "JournalLineTypeTranslations",
                column: "EntityId",
                principalTable: "JournalLineTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_JournalTypeTranslations_JournalTypes_EntityId",
                table: "JournalTypeTranslations",
                column: "EntityId",
                principalTable: "JournalTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JournalLineTypeTranslations_JournalLineTypes_EntityId",
                table: "JournalLineTypeTranslations");

            migrationBuilder.DropForeignKey(
                name: "FK_JournalTypeTranslations_JournalTypes_EntityId",
                table: "JournalTypeTranslations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_JournalTypeTranslations",
                table: "JournalTypeTranslations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_JournalLineTypeTranslations",
                table: "JournalLineTypeTranslations");

            migrationBuilder.RenameTable(
                name: "JournalTypeTranslations",
                newName: "JournalTypeTranslationEntity");

            migrationBuilder.RenameTable(
                name: "JournalLineTypeTranslations",
                newName: "JournalLineTypeTranslationEntity");

            migrationBuilder.RenameIndex(
                name: "IX_JournalTypeTranslations_Name",
                table: "JournalTypeTranslationEntity",
                newName: "IX_JournalTypeTranslationEntity_Name");

            migrationBuilder.RenameIndex(
                name: "IX_JournalTypeTranslations_LanguageCode",
                table: "JournalTypeTranslationEntity",
                newName: "IX_JournalTypeTranslationEntity_LanguageCode");

            migrationBuilder.RenameIndex(
                name: "IX_JournalTypeTranslations_EntityId_LanguageCode",
                table: "JournalTypeTranslationEntity",
                newName: "IX_JournalTypeTranslationEntity_EntityId_LanguageCode");

            migrationBuilder.RenameIndex(
                name: "IX_JournalLineTypeTranslations_Name",
                table: "JournalLineTypeTranslationEntity",
                newName: "IX_JournalLineTypeTranslationEntity_Name");

            migrationBuilder.RenameIndex(
                name: "IX_JournalLineTypeTranslations_LanguageCode",
                table: "JournalLineTypeTranslationEntity",
                newName: "IX_JournalLineTypeTranslationEntity_LanguageCode");

            migrationBuilder.RenameIndex(
                name: "IX_JournalLineTypeTranslations_EntityId_LanguageCode",
                table: "JournalLineTypeTranslationEntity",
                newName: "IX_JournalLineTypeTranslationEntity_EntityId_LanguageCode");

            migrationBuilder.AddPrimaryKey(
                name: "PK_JournalTypeTranslationEntity",
                table: "JournalTypeTranslationEntity",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_JournalLineTypeTranslationEntity",
                table: "JournalLineTypeTranslationEntity",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_JournalLineTypeTranslationEntity_JournalLineTypes_EntityId",
                table: "JournalLineTypeTranslationEntity",
                column: "EntityId",
                principalTable: "JournalLineTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_JournalTypeTranslationEntity_JournalTypes_EntityId",
                table: "JournalTypeTranslationEntity",
                column: "EntityId",
                principalTable: "JournalTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
