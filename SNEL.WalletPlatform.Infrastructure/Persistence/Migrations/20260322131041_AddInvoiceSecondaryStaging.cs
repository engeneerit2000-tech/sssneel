using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceSecondaryStaging : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InvoiceSecondaryStagings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BillingServiceId = table.Column<long>(type: "bigint", nullable: false),
                    BillingRef = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    OriginInvoiceId = table.Column<long>(type: "bigint", nullable: true),
                    InvoiceRef = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    AmountOutstanding = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AmountPaid = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AmountTotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CurrencyId = table.Column<int>(type: "integer", nullable: false),
                    IssueDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DueDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    InvoiceInfo = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    OriginCustomerId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PaymentMismatchFlag = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    InvoiceUpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    JobKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Updated = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceSecondaryStagings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoiceSecondaryStagings_BillingServices_BillingServiceId",
                        column: x => x.BillingServiceId,
                        principalTable: "BillingServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InvoiceSecondaryStagings_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceSecondaryStagings_BillingServiceId",
                table: "InvoiceSecondaryStagings",
                column: "BillingServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceSecondaryStagings_CurrencyId",
                table: "InvoiceSecondaryStagings",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceSecondaryStagings_JobKey",
                table: "InvoiceSecondaryStagings",
                column: "JobKey");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceSecondaryStagings_JobKey_InvoiceRef",
                table: "InvoiceSecondaryStagings",
                columns: new[] { "JobKey", "InvoiceRef" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvoiceSecondaryStagings");
        }
    }
}
