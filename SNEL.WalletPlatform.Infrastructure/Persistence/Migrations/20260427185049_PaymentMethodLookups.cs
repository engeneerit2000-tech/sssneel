using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PaymentMethodLookups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConfirmationModeEntity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfirmationModeEntity", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConfirmationTypeEntity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfirmationTypeEntity", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentInitModeEntity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentInitModeEntity", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentMethodPurposeEntity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentMethodPurposeEntity", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConfirmationModeTranslationEntity",
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
                    table.PrimaryKey("PK_ConfirmationModeTranslationEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConfirmationModeTranslationEntity_ConfirmationModeEntity_En~",
                        column: x => x.EntityId,
                        principalTable: "ConfirmationModeEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConfirmationTypeTranslationEntity",
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
                    table.PrimaryKey("PK_ConfirmationTypeTranslationEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConfirmationTypeTranslationEntity_ConfirmationTypeEntity_En~",
                        column: x => x.EntityId,
                        principalTable: "ConfirmationTypeEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentInitModeTranslationEntity",
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
                    table.PrimaryKey("PK_PaymentInitModeTranslationEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentInitModeTranslationEntity_PaymentInitModeEntity_Enti~",
                        column: x => x.EntityId,
                        principalTable: "PaymentInitModeEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentMethodPurposeTranslationEntity",
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
                    table.PrimaryKey("PK_PaymentMethodPurposeTranslationEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentMethodPurposeTranslationEntity_PaymentMethodPurposeE~",
                        column: x => x.EntityId,
                        principalTable: "PaymentMethodPurposeEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ConfirmationModeEntity",
                columns: new[] { "Id", "Active", "Code", "Name", "Order" },
                values: new object[,]
                {
                    { 1, true, "EXTERNAL", "External", 1 },
                    { 2, true, "PLATFORM", "Platform", 2 },
                    { 3, true, "NONE", "None", 3 }
                });

            migrationBuilder.InsertData(
                table: "ConfirmationTypeEntity",
                columns: new[] { "Id", "Active", "Code", "Name", "Order" },
                values: new object[,]
                {
                    { 1, true, "NONE", "None", 1 },
                    { 2, true, "OTP", "OTP", 2 },
                    { 3, true, "PROMPT", "Prompt", 3 }
                });

            migrationBuilder.InsertData(
                table: "PaymentInitModeEntity",
                columns: new[] { "Id", "Active", "Code", "Name", "Order" },
                values: new object[,]
                {
                    { 1, true, "UNKNOWN", "Unknown", 1 },
                    { 2, true, "CLIENT_SIDE", "Client Side", 2 },
                    { 3, true, "SERVER_SIDE", "Server Side", 3 }
                });

            migrationBuilder.InsertData(
                table: "PaymentMethodPurposeEntity",
                columns: new[] { "Id", "Active", "Code", "Name", "Order" },
                values: new object[,]
                {
                    { 1, true, "UNKNOWN", "Unknown", 1 },
                    { 2, true, "TOPUP", "Topup", 2 },
                    { 3, true, "BILL_PAYMENT", "Bill Payment", 3 },
                    { 4, true, "METER_CHARGE", "Meter Charge", 4 }
                });

            migrationBuilder.InsertData(
                table: "ConfirmationModeTranslationEntity",
                columns: new[] { "Id", "EntityId", "LanguageCode", "Name" },
                values: new object[,]
                {
                    { 1, 1, "fr", "Externe" },
                    { 2, 2, "fr", "Plate-forme" },
                    { 3, 3, "fr", "Aucune" }
                });

            migrationBuilder.InsertData(
                table: "ConfirmationTypeTranslationEntity",
                columns: new[] { "Id", "EntityId", "LanguageCode", "Name" },
                values: new object[,]
                {
                    { 1, 1, "fr", "Aucune" },
                    { 2, 2, "fr", "OTP" },
                    { 3, 3, "fr", "Prompt" }
                });

            migrationBuilder.InsertData(
                table: "PaymentInitModeTranslationEntity",
                columns: new[] { "Id", "EntityId", "LanguageCode", "Name" },
                values: new object[,]
                {
                    { 1, 1, "fr", "Inconnue" },
                    { 2, 2, "fr", "Côté client" },
                    { 3, 3, "fr", "Côté serveur" }
                });

            migrationBuilder.InsertData(
                table: "PaymentMethodPurposeTranslationEntity",
                columns: new[] { "Id", "EntityId", "LanguageCode", "Name" },
                values: new object[,]
                {
                    { 1, 1, "fr", "Aucune" },
                    { 2, 2, "fr", "Recharge" },
                    { 3, 3, "fr", "Paiement de facture" },
                    { 4, 4, "fr", "Frais de compteur" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConfirmationModeEntity_Code",
                table: "ConfirmationModeEntity",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConfirmationModeEntity_Name",
                table: "ConfirmationModeEntity",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_ConfirmationModeTranslationEntity_EntityId_LanguageCode",
                table: "ConfirmationModeTranslationEntity",
                columns: new[] { "EntityId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConfirmationModeTranslationEntity_LanguageCode",
                table: "ConfirmationModeTranslationEntity",
                column: "LanguageCode");

            migrationBuilder.CreateIndex(
                name: "IX_ConfirmationModeTranslationEntity_Name",
                table: "ConfirmationModeTranslationEntity",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_ConfirmationTypeEntity_Code",
                table: "ConfirmationTypeEntity",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConfirmationTypeEntity_Name",
                table: "ConfirmationTypeEntity",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_ConfirmationTypeTranslationEntity_EntityId_LanguageCode",
                table: "ConfirmationTypeTranslationEntity",
                columns: new[] { "EntityId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConfirmationTypeTranslationEntity_LanguageCode",
                table: "ConfirmationTypeTranslationEntity",
                column: "LanguageCode");

            migrationBuilder.CreateIndex(
                name: "IX_ConfirmationTypeTranslationEntity_Name",
                table: "ConfirmationTypeTranslationEntity",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentInitModeEntity_Code",
                table: "PaymentInitModeEntity",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentInitModeEntity_Name",
                table: "PaymentInitModeEntity",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentInitModeTranslationEntity_EntityId_LanguageCode",
                table: "PaymentInitModeTranslationEntity",
                columns: new[] { "EntityId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentInitModeTranslationEntity_LanguageCode",
                table: "PaymentInitModeTranslationEntity",
                column: "LanguageCode");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentInitModeTranslationEntity_Name",
                table: "PaymentInitModeTranslationEntity",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethodPurposeEntity_Code",
                table: "PaymentMethodPurposeEntity",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethodPurposeEntity_Name",
                table: "PaymentMethodPurposeEntity",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethodPurposeTranslationEntity_EntityId_LanguageCode",
                table: "PaymentMethodPurposeTranslationEntity",
                columns: new[] { "EntityId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethodPurposeTranslationEntity_LanguageCode",
                table: "PaymentMethodPurposeTranslationEntity",
                column: "LanguageCode");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethodPurposeTranslationEntity_Name",
                table: "PaymentMethodPurposeTranslationEntity",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfirmationModeTranslationEntity");

            migrationBuilder.DropTable(
                name: "ConfirmationTypeTranslationEntity");

            migrationBuilder.DropTable(
                name: "PaymentInitModeTranslationEntity");

            migrationBuilder.DropTable(
                name: "PaymentMethodPurposeTranslationEntity");

            migrationBuilder.DropTable(
                name: "ConfirmationModeEntity");

            migrationBuilder.DropTable(
                name: "ConfirmationTypeEntity");

            migrationBuilder.DropTable(
                name: "PaymentInitModeEntity");

            migrationBuilder.DropTable(
                name: "PaymentMethodPurposeEntity");
        }
    }
}
