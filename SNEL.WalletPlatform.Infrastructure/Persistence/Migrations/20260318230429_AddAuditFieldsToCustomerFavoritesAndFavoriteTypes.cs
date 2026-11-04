using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditFieldsToCustomerFavoritesAndFavoriteTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FavoriteTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FavoriteTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CustomerFavorites",
                columns: table => new
                {
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    favorite_type_id = table.Column<int>(type: "integer", nullable: false),
                    favorite_id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerFavorites", x => new { x.customer_id, x.favorite_type_id, x.favorite_id });
                    table.ForeignKey(
                        name: "FK_CustomerFavorites_CustomerSubscriptions_favorite_id",
                        column: x => x.favorite_id,
                        principalTable: "CustomerSubscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomerFavorites_Customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomerFavorites_FavoriteTypes_favorite_type_id",
                        column: x => x.favorite_type_id,
                        principalTable: "FavoriteTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FavoriteTypeTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    EntityId = table.Column<int>(type: "integer", nullable: false),
                    LanguageCode = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FavoriteTypeTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FavoriteTypeTranslations_FavoriteTypes_EntityId",
                        column: x => x.EntityId,
                        principalTable: "FavoriteTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "FavoriteTypes",
                columns: new[] { "Id", "Active", "Code", "CreatedAtUtc", "CreatedBy", "Name", "Order", "UpdatedAtUtc", "UpdatedBy" },
                values: new object[] { 1, true, "SUBSCRIPTION", new DateTimeOffset(new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "SYSTEM", "Subscription", 1, new DateTimeOffset(new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "SYSTEM" });

            migrationBuilder.InsertData(
                table: "FavoriteTypeTranslations",
                columns: new[] { "Id", "CreatedAtUtc", "CreatedBy", "EntityId", "LanguageCode", "Name", "UpdatedAtUtc", "UpdatedBy" },
                values: new object[] { 1, new DateTimeOffset(new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "SYSTEM", 1, "fr", "Abonnement", new DateTimeOffset(new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "SYSTEM" });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerFavorites_favorite_id",
                table: "CustomerFavorites",
                column: "favorite_id");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerFavorites_favorite_type_id",
                table: "CustomerFavorites",
                column: "favorite_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_FavoriteTypeTranslations_EntityId",
                table: "FavoriteTypeTranslations",
                column: "EntityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerFavorites");

            migrationBuilder.DropTable(
                name: "FavoriteTypeTranslations");

            migrationBuilder.DropTable(
                name: "FavoriteTypes");
        }
    }
}
