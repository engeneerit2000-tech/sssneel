using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRedundantNewInvoiceNotificationColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AmountOutstanding",
                table: "NewInvoiceNotifications");

            migrationBuilder.DropColumn(
                name: "AmountPaid",
                table: "NewInvoiceNotifications");

            migrationBuilder.DropColumn(
                name: "AmountTotal",
                table: "NewInvoiceNotifications");

            migrationBuilder.DropColumn(
                name: "BillingRef",
                table: "NewInvoiceNotifications");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "NewInvoiceNotifications");

            migrationBuilder.DropColumn(
                name: "InvoiceRef",
                table: "NewInvoiceNotifications");

            migrationBuilder.DropColumn(
                name: "IssueDate",
                table: "NewInvoiceNotifications");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AmountOutstanding",
                table: "NewInvoiceNotifications",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AmountPaid",
                table: "NewInvoiceNotifications",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AmountTotal",
                table: "NewInvoiceNotifications",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "BillingRef",
                table: "NewInvoiceNotifications",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DueDate",
                table: "NewInvoiceNotifications",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvoiceRef",
                table: "NewInvoiceNotifications",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "IssueDate",
                table: "NewInvoiceNotifications",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));
        }
    }
}
