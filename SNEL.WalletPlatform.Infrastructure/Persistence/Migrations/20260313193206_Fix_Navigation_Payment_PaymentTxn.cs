using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Fix_Navigation_Payment_PaymentTxn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentTxns_Payments_PaymentEntityId",
                table: "PaymentTxns");

            migrationBuilder.DropIndex(
                name: "IX_PaymentTxns_PaymentEntityId",
                table: "PaymentTxns");

            migrationBuilder.DropColumn(
                name: "PaymentEntityId",
                table: "PaymentTxns");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PaymentEntityId",
                table: "PaymentTxns",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTxns_PaymentEntityId",
                table: "PaymentTxns",
                column: "PaymentEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentTxns_Payments_PaymentEntityId",
                table: "PaymentTxns",
                column: "PaymentEntityId",
                principalTable: "Payments",
                principalColumn: "Id");
        }
    }
}
