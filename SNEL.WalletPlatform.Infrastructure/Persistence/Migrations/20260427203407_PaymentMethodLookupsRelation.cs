using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PaymentMethodLookupsRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"UPDATE ""PaymentMethods"" SET ""ConfirmationTypeEnum"" = ""ConfirmationTypeEnum"" + 1, ""PaymentInitMode"" = ""PaymentInitMode"" + 1, ""PaymentMethodPurpose"" = ""PaymentMethodPurpose"" + 1;");
            migrationBuilder.RenameColumn(
                name: "PaymentMethodPurpose",
                table: "PaymentMethods",
                newName: "PaymentMethodPurposeId");

            migrationBuilder.RenameColumn(
                name: "PaymentInitMode",
                table: "PaymentMethods",
                newName: "PaymentInitModeId");

            migrationBuilder.RenameColumn(
                name: "ConfirmationTypeEnum",
                table: "PaymentMethods",
                newName: "ConfirmationTypeId");

            migrationBuilder.RenameColumn(
                name: "ConfirmationMode",
                table: "PaymentMethods",
                newName: "ConfirmationModeId");

            migrationBuilder.RenameColumn(
                name: "CompletionMode",
                table: "PaymentMethods",
                newName: "CompletionModeId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethods_CompletionModeId",
                table: "PaymentMethods",
                column: "CompletionModeId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethods_ConfirmationModeId",
                table: "PaymentMethods",
                column: "ConfirmationModeId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethods_ConfirmationTypeId",
                table: "PaymentMethods",
                column: "ConfirmationTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethods_PaymentInitModeId",
                table: "PaymentMethods",
                column: "PaymentInitModeId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethods_PaymentMethodPurposeId",
                table: "PaymentMethods",
                column: "PaymentMethodPurposeId");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.DropIndex(
                name: "IX_PaymentMethods_CompletionModeId",
                table: "PaymentMethods");

            migrationBuilder.DropIndex(
                name: "IX_PaymentMethods_ConfirmationModeId",
                table: "PaymentMethods");

            migrationBuilder.DropIndex(
                name: "IX_PaymentMethods_ConfirmationTypeId",
                table: "PaymentMethods");

            migrationBuilder.DropIndex(
                name: "IX_PaymentMethods_PaymentInitModeId",
                table: "PaymentMethods");

            migrationBuilder.DropIndex(
                name: "IX_PaymentMethods_PaymentMethodPurposeId",
                table: "PaymentMethods");

            migrationBuilder.RenameColumn(
                name: "PaymentMethodPurposeId",
                table: "PaymentMethods",
                newName: "PaymentMethodPurpose");

            migrationBuilder.RenameColumn(
                name: "PaymentInitModeId",
                table: "PaymentMethods",
                newName: "PaymentInitMode");

            migrationBuilder.RenameColumn(
                name: "ConfirmationTypeId",
                table: "PaymentMethods",
                newName: "ConfirmationTypeEnum");

            migrationBuilder.RenameColumn(
                name: "ConfirmationModeId",
                table: "PaymentMethods",
                newName: "ConfirmationMode");

            migrationBuilder.RenameColumn(
                name: "CompletionModeId",
                table: "PaymentMethods",
                newName: "CompletionMode");
            migrationBuilder.Sql($@"UPDATE ""PaymentMethods"" SET ""ConfirmationTypeEnum"" = ""ConfirmationTypeEnum"" - 1, ""PaymentInitMode"" = ""PaymentInitMode"" - 1, ""PaymentMethodPurpose"" = ""PaymentMethodPurpose"" - 1;");
        }
    }
}
