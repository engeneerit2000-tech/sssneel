using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PaymentMethodAbstraction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompletionMode",
                table: "PaymentMethods",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ConfirmationMode",
                table: "PaymentMethods",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ConfirmationTypeEnum",
                table: "PaymentMethods",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PaymentInitMode",
                table: "PaymentMethods",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PaymentMethodPurpose",
                table: "PaymentMethods",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletionMode",
                table: "PaymentMethods");

            migrationBuilder.DropColumn(
                name: "ConfirmationMode",
                table: "PaymentMethods");

            migrationBuilder.DropColumn(
                name: "ConfirmationTypeEnum",
                table: "PaymentMethods");

            migrationBuilder.DropColumn(
                name: "PaymentInitMode",
                table: "PaymentMethods");

            migrationBuilder.DropColumn(
                name: "PaymentMethodPurpose",
                table: "PaymentMethods");
        }
    }
}
