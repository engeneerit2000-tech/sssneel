using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class tempLookupEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompletionModeTranslationEntity_CompletionModeEntity_Entity~",
                table: "CompletionModeTranslationEntity");

            migrationBuilder.DropForeignKey(
                name: "FK_ConfirmationModeTranslationEntity_ConfirmationModeEntity_En~",
                table: "ConfirmationModeTranslationEntity");

            migrationBuilder.DropForeignKey(
                name: "FK_ConfirmationTypeTranslationEntity_ConfirmationTypeEntity_En~",
                table: "ConfirmationTypeTranslationEntity");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentInitModeTranslationEntity_PaymentInitModeEntity_Enti~",
                table: "PaymentInitModeTranslationEntity");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentMethodPurposeTranslationEntity_PaymentMethodPurposeE~",
                table: "PaymentMethodPurposeTranslationEntity");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentMethods_CompletionModeEntity_CompletionModeId",
                table: "PaymentMethods");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentMethods_ConfirmationModeEntity_ConfirmationModeId",
                table: "PaymentMethods");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentMethods_ConfirmationTypeEntity_ConfirmationTypeId",
                table: "PaymentMethods");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentMethods_PaymentInitModeEntity_PaymentInitModeId",
                table: "PaymentMethods");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentMethods_PaymentMethodPurposeEntity_PaymentMethodPurp~",
                table: "PaymentMethods");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PaymentMethodPurposeTranslationEntity",
                table: "PaymentMethodPurposeTranslationEntity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PaymentMethodPurposeEntity",
                table: "PaymentMethodPurposeEntity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PaymentInitModeTranslationEntity",
                table: "PaymentInitModeTranslationEntity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PaymentInitModeEntity",
                table: "PaymentInitModeEntity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ConfirmationTypeTranslationEntity",
                table: "ConfirmationTypeTranslationEntity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ConfirmationTypeEntity",
                table: "ConfirmationTypeEntity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ConfirmationModeTranslationEntity",
                table: "ConfirmationModeTranslationEntity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ConfirmationModeEntity",
                table: "ConfirmationModeEntity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CompletionModeTranslationEntity",
                table: "CompletionModeTranslationEntity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CompletionModeEntity",
                table: "CompletionModeEntity");

            migrationBuilder.RenameTable(
                name: "PaymentMethodPurposeTranslationEntity",
                newName: "PaymentMethodPurposeTranslations");

            migrationBuilder.RenameTable(
                name: "PaymentMethodPurposeEntity",
                newName: "PaymentMethodPurposes");

            migrationBuilder.RenameTable(
                name: "PaymentInitModeTranslationEntity",
                newName: "PaymentInitModeTranslations");

            migrationBuilder.RenameTable(
                name: "PaymentInitModeEntity",
                newName: "PaymentInitModes");

            migrationBuilder.RenameTable(
                name: "ConfirmationTypeTranslationEntity",
                newName: "ConfirmationTypeTranslations");

            migrationBuilder.RenameTable(
                name: "ConfirmationTypeEntity",
                newName: "ConfirmationTypes");

            migrationBuilder.RenameTable(
                name: "ConfirmationModeTranslationEntity",
                newName: "ConfirmationModeTranslations");

            migrationBuilder.RenameTable(
                name: "ConfirmationModeEntity",
                newName: "ConfirmationModes");

            migrationBuilder.RenameTable(
                name: "CompletionModeTranslationEntity",
                newName: "CompletionModeTranslations");

            migrationBuilder.RenameTable(
                name: "CompletionModeEntity",
                newName: "CompletionModes");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentMethodPurposeTranslationEntity_Name",
                table: "PaymentMethodPurposeTranslations",
                newName: "IX_PaymentMethodPurposeTranslations_Name");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentMethodPurposeTranslationEntity_LanguageCode",
                table: "PaymentMethodPurposeTranslations",
                newName: "IX_PaymentMethodPurposeTranslations_LanguageCode");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentMethodPurposeTranslationEntity_EntityId_LanguageCode",
                table: "PaymentMethodPurposeTranslations",
                newName: "IX_PaymentMethodPurposeTranslations_EntityId_LanguageCode");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentMethodPurposeEntity_Name",
                table: "PaymentMethodPurposes",
                newName: "IX_PaymentMethodPurposes_Name");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentMethodPurposeEntity_Code",
                table: "PaymentMethodPurposes",
                newName: "IX_PaymentMethodPurposes_Code");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentInitModeTranslationEntity_Name",
                table: "PaymentInitModeTranslations",
                newName: "IX_PaymentInitModeTranslations_Name");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentInitModeTranslationEntity_LanguageCode",
                table: "PaymentInitModeTranslations",
                newName: "IX_PaymentInitModeTranslations_LanguageCode");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentInitModeTranslationEntity_EntityId_LanguageCode",
                table: "PaymentInitModeTranslations",
                newName: "IX_PaymentInitModeTranslations_EntityId_LanguageCode");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentInitModeEntity_Name",
                table: "PaymentInitModes",
                newName: "IX_PaymentInitModes_Name");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentInitModeEntity_Code",
                table: "PaymentInitModes",
                newName: "IX_PaymentInitModes_Code");

            migrationBuilder.RenameIndex(
                name: "IX_ConfirmationTypeTranslationEntity_Name",
                table: "ConfirmationTypeTranslations",
                newName: "IX_ConfirmationTypeTranslations_Name");

            migrationBuilder.RenameIndex(
                name: "IX_ConfirmationTypeTranslationEntity_LanguageCode",
                table: "ConfirmationTypeTranslations",
                newName: "IX_ConfirmationTypeTranslations_LanguageCode");

            migrationBuilder.RenameIndex(
                name: "IX_ConfirmationTypeTranslationEntity_EntityId_LanguageCode",
                table: "ConfirmationTypeTranslations",
                newName: "IX_ConfirmationTypeTranslations_EntityId_LanguageCode");

            migrationBuilder.RenameIndex(
                name: "IX_ConfirmationTypeEntity_Name",
                table: "ConfirmationTypes",
                newName: "IX_ConfirmationTypes_Name");

            migrationBuilder.RenameIndex(
                name: "IX_ConfirmationTypeEntity_Code",
                table: "ConfirmationTypes",
                newName: "IX_ConfirmationTypes_Code");

            migrationBuilder.RenameIndex(
                name: "IX_ConfirmationModeTranslationEntity_Name",
                table: "ConfirmationModeTranslations",
                newName: "IX_ConfirmationModeTranslations_Name");

            migrationBuilder.RenameIndex(
                name: "IX_ConfirmationModeTranslationEntity_LanguageCode",
                table: "ConfirmationModeTranslations",
                newName: "IX_ConfirmationModeTranslations_LanguageCode");

            migrationBuilder.RenameIndex(
                name: "IX_ConfirmationModeTranslationEntity_EntityId_LanguageCode",
                table: "ConfirmationModeTranslations",
                newName: "IX_ConfirmationModeTranslations_EntityId_LanguageCode");

            migrationBuilder.RenameIndex(
                name: "IX_ConfirmationModeEntity_Name",
                table: "ConfirmationModes",
                newName: "IX_ConfirmationModes_Name");

            migrationBuilder.RenameIndex(
                name: "IX_ConfirmationModeEntity_Code",
                table: "ConfirmationModes",
                newName: "IX_ConfirmationModes_Code");

            migrationBuilder.RenameIndex(
                name: "IX_CompletionModeTranslationEntity_Name",
                table: "CompletionModeTranslations",
                newName: "IX_CompletionModeTranslations_Name");

            migrationBuilder.RenameIndex(
                name: "IX_CompletionModeTranslationEntity_LanguageCode",
                table: "CompletionModeTranslations",
                newName: "IX_CompletionModeTranslations_LanguageCode");

            migrationBuilder.RenameIndex(
                name: "IX_CompletionModeTranslationEntity_EntityId_LanguageCode",
                table: "CompletionModeTranslations",
                newName: "IX_CompletionModeTranslations_EntityId_LanguageCode");

            migrationBuilder.RenameIndex(
                name: "IX_CompletionModeEntity_Name",
                table: "CompletionModes",
                newName: "IX_CompletionModes_Name");

            migrationBuilder.RenameIndex(
                name: "IX_CompletionModeEntity_Code",
                table: "CompletionModes",
                newName: "IX_CompletionModes_Code");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PaymentMethodPurposeTranslations",
                table: "PaymentMethodPurposeTranslations",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PaymentMethodPurposes",
                table: "PaymentMethodPurposes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PaymentInitModeTranslations",
                table: "PaymentInitModeTranslations",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PaymentInitModes",
                table: "PaymentInitModes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ConfirmationTypeTranslations",
                table: "ConfirmationTypeTranslations",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ConfirmationTypes",
                table: "ConfirmationTypes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ConfirmationModeTranslations",
                table: "ConfirmationModeTranslations",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ConfirmationModes",
                table: "ConfirmationModes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CompletionModeTranslations",
                table: "CompletionModeTranslations",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CompletionModes",
                table: "CompletionModes",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CompletionModeTranslations_CompletionModes_EntityId",
                table: "CompletionModeTranslations",
                column: "EntityId",
                principalTable: "CompletionModes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ConfirmationModeTranslations_ConfirmationModes_EntityId",
                table: "ConfirmationModeTranslations",
                column: "EntityId",
                principalTable: "ConfirmationModes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ConfirmationTypeTranslations_ConfirmationTypes_EntityId",
                table: "ConfirmationTypeTranslations",
                column: "EntityId",
                principalTable: "ConfirmationTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentInitModeTranslations_PaymentInitModes_EntityId",
                table: "PaymentInitModeTranslations",
                column: "EntityId",
                principalTable: "PaymentInitModes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentMethodPurposeTranslations_PaymentMethodPurposes_Enti~",
                table: "PaymentMethodPurposeTranslations",
                column: "EntityId",
                principalTable: "PaymentMethodPurposes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentMethods_CompletionModes_CompletionModeId",
                table: "PaymentMethods",
                column: "CompletionModeId",
                principalTable: "CompletionModes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentMethods_ConfirmationModes_ConfirmationModeId",
                table: "PaymentMethods",
                column: "ConfirmationModeId",
                principalTable: "ConfirmationModes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentMethods_ConfirmationTypes_ConfirmationTypeId",
                table: "PaymentMethods",
                column: "ConfirmationTypeId",
                principalTable: "ConfirmationTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentMethods_PaymentInitModes_PaymentInitModeId",
                table: "PaymentMethods",
                column: "PaymentInitModeId",
                principalTable: "PaymentInitModes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentMethods_PaymentMethodPurposes_PaymentMethodPurposeId",
                table: "PaymentMethods",
                column: "PaymentMethodPurposeId",
                principalTable: "PaymentMethodPurposes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompletionModeTranslations_CompletionModes_EntityId",
                table: "CompletionModeTranslations");

            migrationBuilder.DropForeignKey(
                name: "FK_ConfirmationModeTranslations_ConfirmationModes_EntityId",
                table: "ConfirmationModeTranslations");

            migrationBuilder.DropForeignKey(
                name: "FK_ConfirmationTypeTranslations_ConfirmationTypes_EntityId",
                table: "ConfirmationTypeTranslations");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentInitModeTranslations_PaymentInitModes_EntityId",
                table: "PaymentInitModeTranslations");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentMethodPurposeTranslations_PaymentMethodPurposes_Enti~",
                table: "PaymentMethodPurposeTranslations");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentMethods_CompletionModes_CompletionModeId",
                table: "PaymentMethods");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentMethods_ConfirmationModes_ConfirmationModeId",
                table: "PaymentMethods");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentMethods_ConfirmationTypes_ConfirmationTypeId",
                table: "PaymentMethods");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentMethods_PaymentInitModes_PaymentInitModeId",
                table: "PaymentMethods");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentMethods_PaymentMethodPurposes_PaymentMethodPurposeId",
                table: "PaymentMethods");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PaymentMethodPurposeTranslations",
                table: "PaymentMethodPurposeTranslations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PaymentMethodPurposes",
                table: "PaymentMethodPurposes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PaymentInitModeTranslations",
                table: "PaymentInitModeTranslations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PaymentInitModes",
                table: "PaymentInitModes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ConfirmationTypeTranslations",
                table: "ConfirmationTypeTranslations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ConfirmationTypes",
                table: "ConfirmationTypes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ConfirmationModeTranslations",
                table: "ConfirmationModeTranslations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ConfirmationModes",
                table: "ConfirmationModes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CompletionModeTranslations",
                table: "CompletionModeTranslations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CompletionModes",
                table: "CompletionModes");

            migrationBuilder.RenameTable(
                name: "PaymentMethodPurposeTranslations",
                newName: "PaymentMethodPurposeTranslationEntity");

            migrationBuilder.RenameTable(
                name: "PaymentMethodPurposes",
                newName: "PaymentMethodPurposeEntity");

            migrationBuilder.RenameTable(
                name: "PaymentInitModeTranslations",
                newName: "PaymentInitModeTranslationEntity");

            migrationBuilder.RenameTable(
                name: "PaymentInitModes",
                newName: "PaymentInitModeEntity");

            migrationBuilder.RenameTable(
                name: "ConfirmationTypeTranslations",
                newName: "ConfirmationTypeTranslationEntity");

            migrationBuilder.RenameTable(
                name: "ConfirmationTypes",
                newName: "ConfirmationTypeEntity");

            migrationBuilder.RenameTable(
                name: "ConfirmationModeTranslations",
                newName: "ConfirmationModeTranslationEntity");

            migrationBuilder.RenameTable(
                name: "ConfirmationModes",
                newName: "ConfirmationModeEntity");

            migrationBuilder.RenameTable(
                name: "CompletionModeTranslations",
                newName: "CompletionModeTranslationEntity");

            migrationBuilder.RenameTable(
                name: "CompletionModes",
                newName: "CompletionModeEntity");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentMethodPurposeTranslations_Name",
                table: "PaymentMethodPurposeTranslationEntity",
                newName: "IX_PaymentMethodPurposeTranslationEntity_Name");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentMethodPurposeTranslations_LanguageCode",
                table: "PaymentMethodPurposeTranslationEntity",
                newName: "IX_PaymentMethodPurposeTranslationEntity_LanguageCode");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentMethodPurposeTranslations_EntityId_LanguageCode",
                table: "PaymentMethodPurposeTranslationEntity",
                newName: "IX_PaymentMethodPurposeTranslationEntity_EntityId_LanguageCode");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentMethodPurposes_Name",
                table: "PaymentMethodPurposeEntity",
                newName: "IX_PaymentMethodPurposeEntity_Name");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentMethodPurposes_Code",
                table: "PaymentMethodPurposeEntity",
                newName: "IX_PaymentMethodPurposeEntity_Code");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentInitModeTranslations_Name",
                table: "PaymentInitModeTranslationEntity",
                newName: "IX_PaymentInitModeTranslationEntity_Name");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentInitModeTranslations_LanguageCode",
                table: "PaymentInitModeTranslationEntity",
                newName: "IX_PaymentInitModeTranslationEntity_LanguageCode");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentInitModeTranslations_EntityId_LanguageCode",
                table: "PaymentInitModeTranslationEntity",
                newName: "IX_PaymentInitModeTranslationEntity_EntityId_LanguageCode");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentInitModes_Name",
                table: "PaymentInitModeEntity",
                newName: "IX_PaymentInitModeEntity_Name");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentInitModes_Code",
                table: "PaymentInitModeEntity",
                newName: "IX_PaymentInitModeEntity_Code");

            migrationBuilder.RenameIndex(
                name: "IX_ConfirmationTypeTranslations_Name",
                table: "ConfirmationTypeTranslationEntity",
                newName: "IX_ConfirmationTypeTranslationEntity_Name");

            migrationBuilder.RenameIndex(
                name: "IX_ConfirmationTypeTranslations_LanguageCode",
                table: "ConfirmationTypeTranslationEntity",
                newName: "IX_ConfirmationTypeTranslationEntity_LanguageCode");

            migrationBuilder.RenameIndex(
                name: "IX_ConfirmationTypeTranslations_EntityId_LanguageCode",
                table: "ConfirmationTypeTranslationEntity",
                newName: "IX_ConfirmationTypeTranslationEntity_EntityId_LanguageCode");

            migrationBuilder.RenameIndex(
                name: "IX_ConfirmationTypes_Name",
                table: "ConfirmationTypeEntity",
                newName: "IX_ConfirmationTypeEntity_Name");

            migrationBuilder.RenameIndex(
                name: "IX_ConfirmationTypes_Code",
                table: "ConfirmationTypeEntity",
                newName: "IX_ConfirmationTypeEntity_Code");

            migrationBuilder.RenameIndex(
                name: "IX_ConfirmationModeTranslations_Name",
                table: "ConfirmationModeTranslationEntity",
                newName: "IX_ConfirmationModeTranslationEntity_Name");

            migrationBuilder.RenameIndex(
                name: "IX_ConfirmationModeTranslations_LanguageCode",
                table: "ConfirmationModeTranslationEntity",
                newName: "IX_ConfirmationModeTranslationEntity_LanguageCode");

            migrationBuilder.RenameIndex(
                name: "IX_ConfirmationModeTranslations_EntityId_LanguageCode",
                table: "ConfirmationModeTranslationEntity",
                newName: "IX_ConfirmationModeTranslationEntity_EntityId_LanguageCode");

            migrationBuilder.RenameIndex(
                name: "IX_ConfirmationModes_Name",
                table: "ConfirmationModeEntity",
                newName: "IX_ConfirmationModeEntity_Name");

            migrationBuilder.RenameIndex(
                name: "IX_ConfirmationModes_Code",
                table: "ConfirmationModeEntity",
                newName: "IX_ConfirmationModeEntity_Code");

            migrationBuilder.RenameIndex(
                name: "IX_CompletionModeTranslations_Name",
                table: "CompletionModeTranslationEntity",
                newName: "IX_CompletionModeTranslationEntity_Name");

            migrationBuilder.RenameIndex(
                name: "IX_CompletionModeTranslations_LanguageCode",
                table: "CompletionModeTranslationEntity",
                newName: "IX_CompletionModeTranslationEntity_LanguageCode");

            migrationBuilder.RenameIndex(
                name: "IX_CompletionModeTranslations_EntityId_LanguageCode",
                table: "CompletionModeTranslationEntity",
                newName: "IX_CompletionModeTranslationEntity_EntityId_LanguageCode");

            migrationBuilder.RenameIndex(
                name: "IX_CompletionModes_Name",
                table: "CompletionModeEntity",
                newName: "IX_CompletionModeEntity_Name");

            migrationBuilder.RenameIndex(
                name: "IX_CompletionModes_Code",
                table: "CompletionModeEntity",
                newName: "IX_CompletionModeEntity_Code");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PaymentMethodPurposeTranslationEntity",
                table: "PaymentMethodPurposeTranslationEntity",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PaymentMethodPurposeEntity",
                table: "PaymentMethodPurposeEntity",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PaymentInitModeTranslationEntity",
                table: "PaymentInitModeTranslationEntity",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PaymentInitModeEntity",
                table: "PaymentInitModeEntity",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ConfirmationTypeTranslationEntity",
                table: "ConfirmationTypeTranslationEntity",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ConfirmationTypeEntity",
                table: "ConfirmationTypeEntity",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ConfirmationModeTranslationEntity",
                table: "ConfirmationModeTranslationEntity",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ConfirmationModeEntity",
                table: "ConfirmationModeEntity",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CompletionModeTranslationEntity",
                table: "CompletionModeTranslationEntity",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CompletionModeEntity",
                table: "CompletionModeEntity",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CompletionModeTranslationEntity_CompletionModeEntity_Entity~",
                table: "CompletionModeTranslationEntity",
                column: "EntityId",
                principalTable: "CompletionModeEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ConfirmationModeTranslationEntity_ConfirmationModeEntity_En~",
                table: "ConfirmationModeTranslationEntity",
                column: "EntityId",
                principalTable: "ConfirmationModeEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ConfirmationTypeTranslationEntity_ConfirmationTypeEntity_En~",
                table: "ConfirmationTypeTranslationEntity",
                column: "EntityId",
                principalTable: "ConfirmationTypeEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentInitModeTranslationEntity_PaymentInitModeEntity_Enti~",
                table: "PaymentInitModeTranslationEntity",
                column: "EntityId",
                principalTable: "PaymentInitModeEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentMethodPurposeTranslationEntity_PaymentMethodPurposeE~",
                table: "PaymentMethodPurposeTranslationEntity",
                column: "EntityId",
                principalTable: "PaymentMethodPurposeEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentMethods_CompletionModeEntity_CompletionModeId",
                table: "PaymentMethods",
                column: "CompletionModeId",
                principalTable: "CompletionModeEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentMethods_ConfirmationModeEntity_ConfirmationModeId",
                table: "PaymentMethods",
                column: "ConfirmationModeId",
                principalTable: "ConfirmationModeEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentMethods_ConfirmationTypeEntity_ConfirmationTypeId",
                table: "PaymentMethods",
                column: "ConfirmationTypeId",
                principalTable: "ConfirmationTypeEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentMethods_PaymentInitModeEntity_PaymentInitModeId",
                table: "PaymentMethods",
                column: "PaymentInitModeId",
                principalTable: "PaymentInitModeEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentMethods_PaymentMethodPurposeEntity_PaymentMethodPurp~",
                table: "PaymentMethods",
                column: "PaymentMethodPurposeId",
                principalTable: "PaymentMethodPurposeEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
