using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixJournalMigration2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JournalLines_Journals_JournalEntityId",
                table: "JournalLines");

            migrationBuilder.DropIndex(
                name: "IX_JournalLines_JournalEntityId",
                table: "JournalLines");

            migrationBuilder.DropColumn(
                name: "JournalEntityId",
                table: "JournalLines");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "JournalEntityId",
                table: "JournalLines",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_JournalLines_JournalEntityId",
                table: "JournalLines",
                column: "JournalEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_JournalLines_Journals_JournalEntityId",
                table: "JournalLines",
                column: "JournalEntityId",
                principalTable: "Journals",
                principalColumn: "Id");
        }
    }
}
