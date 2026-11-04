using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TopupJournal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SourceType",
                table: "Journals");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SourceTimestamp",
                table: "Journals",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<int>(
                name: "SourceTypeId",
                table: "Journals",
                type: "integer",
                maxLength: 50,
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "JournalEntityId",
                table: "JournalLines",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "JournalLineTypeTranslationEntity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EntityId = table.Column<int>(type: "integer", nullable: false),
                    LanguageCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalLineTypeTranslationEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JournalLineTypeTranslationEntity_JournalLineTypes_EntityId",
                        column: x => x.EntityId,
                        principalTable: "JournalLineTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JournalTypeTranslationEntity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EntityId = table.Column<int>(type: "integer", nullable: false),
                    LanguageCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalTypeTranslationEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JournalTypeTranslationEntity_JournalTypes_EntityId",
                        column: x => x.EntityId,
                        principalTable: "JournalTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "JournalLineTypes",
                columns: new[] { "Id", "Active", "Code", "Name", "Order" },
                values: new object[,]
                {
                    { 1, true, "CUSTOMER_LIABILITY", "Customer Liability", 1 },
                    { 2, true, "PSP_LIABILITY", "Payment Provider Liability", 2 }
                });

            migrationBuilder.InsertData(
                table: "JournalTypes",
                columns: new[] { "Id", "Active", "Code", "Name", "Order" },
                values: new object[,]
                {
                    { 1, true, "WALLET_TOPUP", "Wallet Topup", 1 },
                    { 2, true, "WALLET_TOPUP_SETTLEMENT", "Wallet Topup Settlement", 2 },
                    { 3, true, "BILL_PAYMENT", "Biller Payment", 3 },
                    { 4, true, "BILLER_SETTLEMENT", "Biller Payment Settlment", 4 }
                });

            migrationBuilder.InsertData(
                table: "JournalLineTypeTranslationEntity",
                columns: new[] { "Id", "EntityId", "LanguageCode", "Name" },
                values: new object[,]
                {
                    { 1, 1, "fr", "Responsabilité du client" },
                    { 2, 2, "fr", "Responsabilité du prestataire de services de paiement" }
                });

            migrationBuilder.InsertData(
                table: "JournalTypeTranslationEntity",
                columns: new[] { "Id", "EntityId", "LanguageCode", "Name" },
                values: new object[,]
                {
                    { 1, 1, "fr", "Rechargement du portefeuille" },
                    { 2, 2, "fr", "Règlement du rechargement du portefeuille" },
                    { 3, 3, "fr", "Paiement de facturier" },
                    { 4, 4, "fr", "Règlement du paiement de facturier" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_JournalLines_JournalEntityId",
                table: "JournalLines",
                column: "JournalEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalLineTypeTranslationEntity_EntityId_LanguageCode",
                table: "JournalLineTypeTranslationEntity",
                columns: new[] { "EntityId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JournalLineTypeTranslationEntity_LanguageCode",
                table: "JournalLineTypeTranslationEntity",
                column: "LanguageCode");

            migrationBuilder.CreateIndex(
                name: "IX_JournalLineTypeTranslationEntity_Name",
                table: "JournalLineTypeTranslationEntity",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_JournalTypeTranslationEntity_EntityId_LanguageCode",
                table: "JournalTypeTranslationEntity",
                columns: new[] { "EntityId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JournalTypeTranslationEntity_LanguageCode",
                table: "JournalTypeTranslationEntity",
                column: "LanguageCode");

            migrationBuilder.CreateIndex(
                name: "IX_JournalTypeTranslationEntity_Name",
                table: "JournalTypeTranslationEntity",
                column: "Name");

            migrationBuilder.AddForeignKey(
                name: "FK_JournalLines_Journals_JournalEntityId",
                table: "JournalLines",
                column: "JournalEntityId",
                principalTable: "Journals",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JournalLines_Journals_JournalEntityId",
                table: "JournalLines");

            migrationBuilder.DropTable(
                name: "JournalLineTypeTranslationEntity");

            migrationBuilder.DropTable(
                name: "JournalTypeTranslationEntity");

            migrationBuilder.DropIndex(
                name: "IX_JournalLines_JournalEntityId",
                table: "JournalLines");

            migrationBuilder.DeleteData(
                table: "JournalLineTypes",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "JournalLineTypes",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "JournalTypes",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "JournalTypes",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "JournalTypes",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "JournalTypes",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DropColumn(
                name: "SourceTimestamp",
                table: "Journals");

            migrationBuilder.DropColumn(
                name: "SourceTypeId",
                table: "Journals");

            migrationBuilder.DropColumn(
                name: "JournalEntityId",
                table: "JournalLines");

            migrationBuilder.AddColumn<string>(
                name: "SourceType",
                table: "Journals",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }
    }
}
