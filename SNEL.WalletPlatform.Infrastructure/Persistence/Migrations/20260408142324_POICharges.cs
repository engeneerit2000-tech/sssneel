using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class POICharges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_PaymentOrders_PaymentOrderId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "Fees",
                table: "PaymentOrders");

            migrationBuilder.DropColumn(
                name: "Tax",
                table: "PaymentOrders");

            migrationBuilder.DropColumn(
                name: "Total",
                table: "PaymentOrders");

            migrationBuilder.DropColumn(
                name: "Fees",
                table: "PaymentOrderItems");

            migrationBuilder.DropColumn(
                name: "Tax",
                table: "PaymentOrderItems");

            migrationBuilder.DropColumn(
                name: "Total",
                table: "PaymentOrderItems");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "PaymentOrders",
                type: "numeric(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmount",
                table: "PaymentOrders",
                type: "numeric(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalCommission",
                table: "PaymentOrders",
                type: "numeric(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalCommissionTax",
                table: "PaymentOrders",
                type: "numeric(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalCost",
                table: "PaymentOrders",
                type: "numeric(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalCostTax",
                table: "PaymentOrders",
                type: "numeric(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalFees",
                table: "PaymentOrders",
                type: "numeric(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalFeesTax",
                table: "PaymentOrders",
                type: "numeric(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "PaymentOrderItems",
                type: "numeric(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AddColumn<decimal>(
                name: "BillerCommission",
                table: "PaymentOrderItems",
                type: "numeric(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "BillerCommissionTax",
                table: "PaymentOrderItems",
                type: "numeric(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "BillerCost",
                table: "PaymentOrderItems",
                type: "numeric(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "BillerCostTax",
                table: "PaymentOrderItems",
                type: "numeric(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "BillerFees",
                table: "PaymentOrderItems",
                type: "numeric(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "BillerFeesTax",
                table: "PaymentOrderItems",
                type: "numeric(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PaymentProviderCommission",
                table: "PaymentOrderItems",
                type: "numeric(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PaymentProviderCommissionTax",
                table: "PaymentOrderItems",
                type: "numeric(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PaymentProviderCost",
                table: "PaymentOrderItems",
                type: "numeric(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PaymentProviderCostTax",
                table: "PaymentOrderItems",
                type: "numeric(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PaymentProviderFees",
                table: "PaymentOrderItems",
                type: "numeric(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PaymentProviderFeesTax",
                table: "PaymentOrderItems",
                type: "numeric(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmount",
                table: "PaymentOrderItems",
                type: "numeric(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalCommission",
                table: "PaymentOrderItems",
                type: "numeric(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalCommissionTax",
                table: "PaymentOrderItems",
                type: "numeric(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalCost",
                table: "PaymentOrderItems",
                type: "numeric(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalCostTax",
                table: "PaymentOrderItems",
                type: "numeric(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalFees",
                table: "PaymentOrderItems",
                type: "numeric(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalFeesTax",
                table: "PaymentOrderItems",
                type: "numeric(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_PaymentOrders_PaymentOrderId",
                table: "Payments",
                column: "PaymentOrderId",
                principalTable: "PaymentOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_PaymentOrders_PaymentOrderId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "TotalAmount",
                table: "PaymentOrders");

            migrationBuilder.DropColumn(
                name: "TotalCommission",
                table: "PaymentOrders");

            migrationBuilder.DropColumn(
                name: "TotalCommissionTax",
                table: "PaymentOrders");

            migrationBuilder.DropColumn(
                name: "TotalCost",
                table: "PaymentOrders");

            migrationBuilder.DropColumn(
                name: "TotalCostTax",
                table: "PaymentOrders");

            migrationBuilder.DropColumn(
                name: "TotalFees",
                table: "PaymentOrders");

            migrationBuilder.DropColumn(
                name: "TotalFeesTax",
                table: "PaymentOrders");

            migrationBuilder.DropColumn(
                name: "BillerCommission",
                table: "PaymentOrderItems");

            migrationBuilder.DropColumn(
                name: "BillerCommissionTax",
                table: "PaymentOrderItems");

            migrationBuilder.DropColumn(
                name: "BillerCost",
                table: "PaymentOrderItems");

            migrationBuilder.DropColumn(
                name: "BillerCostTax",
                table: "PaymentOrderItems");

            migrationBuilder.DropColumn(
                name: "BillerFees",
                table: "PaymentOrderItems");

            migrationBuilder.DropColumn(
                name: "BillerFeesTax",
                table: "PaymentOrderItems");

            migrationBuilder.DropColumn(
                name: "PaymentProviderCommission",
                table: "PaymentOrderItems");

            migrationBuilder.DropColumn(
                name: "PaymentProviderCommissionTax",
                table: "PaymentOrderItems");

            migrationBuilder.DropColumn(
                name: "PaymentProviderCost",
                table: "PaymentOrderItems");

            migrationBuilder.DropColumn(
                name: "PaymentProviderCostTax",
                table: "PaymentOrderItems");

            migrationBuilder.DropColumn(
                name: "PaymentProviderFees",
                table: "PaymentOrderItems");

            migrationBuilder.DropColumn(
                name: "PaymentProviderFeesTax",
                table: "PaymentOrderItems");

            migrationBuilder.DropColumn(
                name: "TotalAmount",
                table: "PaymentOrderItems");

            migrationBuilder.DropColumn(
                name: "TotalCommission",
                table: "PaymentOrderItems");

            migrationBuilder.DropColumn(
                name: "TotalCommissionTax",
                table: "PaymentOrderItems");

            migrationBuilder.DropColumn(
                name: "TotalCost",
                table: "PaymentOrderItems");

            migrationBuilder.DropColumn(
                name: "TotalCostTax",
                table: "PaymentOrderItems");

            migrationBuilder.DropColumn(
                name: "TotalFees",
                table: "PaymentOrderItems");

            migrationBuilder.DropColumn(
                name: "TotalFeesTax",
                table: "PaymentOrderItems");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "PaymentOrders",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,6)",
                oldPrecision: 18,
                oldScale: 6);

            migrationBuilder.AddColumn<decimal>(
                name: "Fees",
                table: "PaymentOrders",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Tax",
                table: "PaymentOrders",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Total",
                table: "PaymentOrders",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "PaymentOrderItems",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,6)",
                oldPrecision: 18,
                oldScale: 6);

            migrationBuilder.AddColumn<decimal>(
                name: "Fees",
                table: "PaymentOrderItems",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Tax",
                table: "PaymentOrderItems",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Total",
                table: "PaymentOrderItems",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_PaymentOrders_PaymentOrderId",
                table: "Payments",
                column: "PaymentOrderId",
                principalTable: "PaymentOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
