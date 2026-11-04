using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBillerCustomerTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BillerCustomers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BillerId = table.Column<long>(type: "bigint", nullable: false),
                    CustomerId = table.Column<long>(type: "bigint", nullable: false),
                    Number = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillerCustomers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BillerCustomers_Billers_BillerId",
                        column: x => x.BillerId,
                        principalTable: "Billers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BillerCustomerStagings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JobKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    BillerId = table.Column<long>(type: "bigint", nullable: false),
                    CustomerId = table.Column<long>(type: "bigint", nullable: false),
                    Number = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Updated = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillerCustomerStagings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BillerCustomerStagings_Billers_BillerId",
                        column: x => x.BillerId,
                        principalTable: "Billers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BillerCustomers_BillerId_CustomerId",
                table: "BillerCustomers",
                columns: new[] { "BillerId", "CustomerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BillerCustomerStagings_BillerId",
                table: "BillerCustomerStagings",
                column: "BillerId");

            migrationBuilder.CreateIndex(
                name: "IX_BillerCustomerStagings_JobKey",
                table: "BillerCustomerStagings",
                column: "JobKey");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BillerCustomers");

            migrationBuilder.DropTable(
                name: "BillerCustomerStagings");
        }
    }
}
