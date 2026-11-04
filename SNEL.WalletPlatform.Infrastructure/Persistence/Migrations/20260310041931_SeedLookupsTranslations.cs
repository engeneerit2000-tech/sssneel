using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedLookupsTranslations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "City_Name");

            migrationBuilder.CreateTable(
                name: "AreaTranslations",
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
                    table.PrimaryKey("PK_AreaTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AreaTranslations_Areas_EntityId",
                        column: x => x.EntityId,
                        principalTable: "Areas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BillingCategoryTranslations",
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
                    table.PrimaryKey("PK_BillingCategoryTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BillingCategoryTranslations_BillingCategories_EntityId",
                        column: x => x.EntityId,
                        principalTable: "BillingCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BillingServiceTypeTranslations",
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
                    table.PrimaryKey("PK_BillingServiceTypeTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BillingServiceTypeTranslations_BillingServiceTypes_EntityId",
                        column: x => x.EntityId,
                        principalTable: "BillingServiceTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChannelTranslations",
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
                    table.PrimaryKey("PK_ChannelTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChannelTranslations_Channels_EntityId",
                        column: x => x.EntityId,
                        principalTable: "Channels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CityTranslations",
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
                    table.PrimaryKey("PK_CityTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CityTranslations_Cities_EntityId",
                        column: x => x.EntityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContactTypeTranslations",
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
                    table.PrimaryKey("PK_ContactTypeTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContactTypeTranslations_ContactTypes_EntityId",
                        column: x => x.EntityId,
                        principalTable: "ContactTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CountryTranslations",
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
                    table.PrimaryKey("PK_CountryTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CountryTranslations_Countries_EntityId",
                        column: x => x.EntityId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CurrencyTranslations",
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
                    table.PrimaryKey("PK_CurrencyTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CurrencyTranslations_Currencies_EntityId",
                        column: x => x.EntityId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PartnerTypeTranslations",
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
                    table.PrimaryKey("PK_PartnerTypeTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartnerTypeTranslations_PartnerTypes_EntityId",
                        column: x => x.EntityId,
                        principalTable: "PartnerTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentCategoryTranslations",
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
                    table.PrimaryKey("PK_PaymentCategoryTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentCategoryTranslations_PaymentCategories_EntityId",
                        column: x => x.EntityId,
                        principalTable: "PaymentCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RegionTranslations",
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
                    table.PrimaryKey("PK_RegionTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegionTranslations_Regions_EntityId",
                        column: x => x.EntityId,
                        principalTable: "Regions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AreaTranslations",
                columns: new[] { "Id", "EntityId", "LanguageCode", "Name" },
                values: new object[,]
                {
                    { 1, 1, "fr", "Est" },
                    { 2, 2, "fr", "Ouest" },
                    { 3, 3, "fr", "Nord" },
                    { 4, 4, "fr", "Sud" },
                    { 5, 5, "fr", "Centre" }
                });

            migrationBuilder.InsertData(
                table: "BillingCategoryTranslations",
                columns: new[] { "Id", "EntityId", "LanguageCode", "Name" },
                values: new object[] { 1, 1, "fr", "Services publics" });

            migrationBuilder.InsertData(
                table: "BillingServiceTypeTranslations",
                columns: new[] { "Id", "EntityId", "LanguageCode", "Name" },
                values: new object[,]
                {
                    { 1, 1, "fr", "Prepaye" },
                    { 2, 2, "fr", "Postpaye" },
                    { 3, 3, "fr", "Les deux" }
                });

            migrationBuilder.InsertData(
                table: "ChannelTranslations",
                columns: new[] { "Id", "EntityId", "LanguageCode", "Name" },
                values: new object[,]
                {
                    { 1, 1, "fr", "Web" },
                    { 2, 2, "fr", "Mobile" }
                });

            migrationBuilder.InsertData(
                table: "CityTranslations",
                columns: new[] { "Id", "EntityId", "LanguageCode", "Name" },
                values: new object[] { 1, 1, "fr", "Kinshasa" });

            migrationBuilder.InsertData(
                table: "ContactTypeTranslations",
                columns: new[] { "Id", "EntityId", "LanguageCode", "Name" },
                values: new object[,]
                {
                    { 1, 1, "fr", "Telephone" },
                    { 2, 2, "fr", "Mobile" },
                    { 3, 3, "fr", "Email" },
                    { 4, 4, "fr", "Courrier" },
                    { 5, 5, "fr", "Fax" }
                });

            migrationBuilder.InsertData(
                table: "CountryTranslations",
                columns: new[] { "Id", "EntityId", "LanguageCode", "Name" },
                values: new object[] { 1, 1, "fr", "Republique democratique du Congo" });

            migrationBuilder.InsertData(
                table: "CurrencyTranslations",
                columns: new[] { "Id", "EntityId", "LanguageCode", "Name" },
                values: new object[,]
                {
                    { 1, 1, "fr", "Franc congolais" },
                    { 2, 2, "fr", "Dollar americain" }
                });

            migrationBuilder.InsertData(
                table: "PartnerTypeTranslations",
                columns: new[] { "Id", "EntityId", "LanguageCode", "Name" },
                values: new object[,]
                {
                    { 1, 1, "fr", "Facturier" },
                    { 2, 2, "fr", "Fournisseur de paiement" },
                    { 3, 3, "fr", "Client" }
                });

            migrationBuilder.InsertData(
                table: "PaymentCategoryTranslations",
                columns: new[] { "Id", "EntityId", "LanguageCode", "Name" },
                values: new object[] { 1, 1, "fr", "Portefeuille" });

            migrationBuilder.InsertData(
                table: "RegionTranslations",
                columns: new[] { "Id", "EntityId", "LanguageCode", "Name" },
                values: new object[] { 1, 1, "fr", "Republique democratique du Congo" });

            migrationBuilder.CreateIndex(
                name: "IX_AreaTranslations_EntityId_LanguageCode",
                table: "AreaTranslations",
                columns: new[] { "EntityId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AreaTranslations_LanguageCode",
                table: "AreaTranslations",
                column: "LanguageCode");

            migrationBuilder.CreateIndex(
                name: "IX_AreaTranslations_Name",
                table: "AreaTranslations",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_BillingCategoryTranslations_EntityId_LanguageCode",
                table: "BillingCategoryTranslations",
                columns: new[] { "EntityId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BillingCategoryTranslations_LanguageCode",
                table: "BillingCategoryTranslations",
                column: "LanguageCode");

            migrationBuilder.CreateIndex(
                name: "IX_BillingCategoryTranslations_Name",
                table: "BillingCategoryTranslations",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_BillingServiceTypeTranslations_EntityId_LanguageCode",
                table: "BillingServiceTypeTranslations",
                columns: new[] { "EntityId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BillingServiceTypeTranslations_LanguageCode",
                table: "BillingServiceTypeTranslations",
                column: "LanguageCode");

            migrationBuilder.CreateIndex(
                name: "IX_BillingServiceTypeTranslations_Name",
                table: "BillingServiceTypeTranslations",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_ChannelTranslations_EntityId_LanguageCode",
                table: "ChannelTranslations",
                columns: new[] { "EntityId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChannelTranslations_LanguageCode",
                table: "ChannelTranslations",
                column: "LanguageCode");

            migrationBuilder.CreateIndex(
                name: "IX_ChannelTranslations_Name",
                table: "ChannelTranslations",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_CityTranslations_EntityId_LanguageCode",
                table: "CityTranslations",
                columns: new[] { "EntityId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CityTranslations_LanguageCode",
                table: "CityTranslations",
                column: "LanguageCode");

            migrationBuilder.CreateIndex(
                name: "IX_CityTranslations_Name",
                table: "CityTranslations",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_ContactTypeTranslations_EntityId_LanguageCode",
                table: "ContactTypeTranslations",
                columns: new[] { "EntityId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContactTypeTranslations_LanguageCode",
                table: "ContactTypeTranslations",
                column: "LanguageCode");

            migrationBuilder.CreateIndex(
                name: "IX_ContactTypeTranslations_Name",
                table: "ContactTypeTranslations",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_CountryTranslations_EntityId_LanguageCode",
                table: "CountryTranslations",
                columns: new[] { "EntityId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CountryTranslations_LanguageCode",
                table: "CountryTranslations",
                column: "LanguageCode");

            migrationBuilder.CreateIndex(
                name: "IX_CountryTranslations_Name",
                table: "CountryTranslations",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyTranslations_EntityId_LanguageCode",
                table: "CurrencyTranslations",
                columns: new[] { "EntityId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyTranslations_LanguageCode",
                table: "CurrencyTranslations",
                column: "LanguageCode");

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyTranslations_Name",
                table: "CurrencyTranslations",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_PartnerTypeTranslations_EntityId_LanguageCode",
                table: "PartnerTypeTranslations",
                columns: new[] { "EntityId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PartnerTypeTranslations_LanguageCode",
                table: "PartnerTypeTranslations",
                column: "LanguageCode");

            migrationBuilder.CreateIndex(
                name: "IX_PartnerTypeTranslations_Name",
                table: "PartnerTypeTranslations",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentCategoryTranslations_EntityId_LanguageCode",
                table: "PaymentCategoryTranslations",
                columns: new[] { "EntityId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentCategoryTranslations_LanguageCode",
                table: "PaymentCategoryTranslations",
                column: "LanguageCode");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentCategoryTranslations_Name",
                table: "PaymentCategoryTranslations",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_RegionTranslations_EntityId_LanguageCode",
                table: "RegionTranslations",
                columns: new[] { "EntityId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegionTranslations_LanguageCode",
                table: "RegionTranslations",
                column: "LanguageCode");

            migrationBuilder.CreateIndex(
                name: "IX_RegionTranslations_Name",
                table: "RegionTranslations",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AreaTranslations");

            migrationBuilder.DropTable(
                name: "BillingCategoryTranslations");

            migrationBuilder.DropTable(
                name: "BillingServiceTypeTranslations");

            migrationBuilder.DropTable(
                name: "ChannelTranslations");

            migrationBuilder.DropTable(
                name: "CityTranslations");

            migrationBuilder.DropTable(
                name: "ContactTypeTranslations");

            migrationBuilder.DropTable(
                name: "CountryTranslations");

            migrationBuilder.DropTable(
                name: "CurrencyTranslations");

            migrationBuilder.DropTable(
                name: "PartnerTypeTranslations");

            migrationBuilder.DropTable(
                name: "PaymentCategoryTranslations");

            migrationBuilder.DropTable(
                name: "RegionTranslations");

            migrationBuilder.CreateTable(
                name: "City_Name",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CityId = table.Column<int>(type: "integer", nullable: false),
                    LangCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_City_Name", x => x.Id);
                    table.ForeignKey(
                        name: "FK_City_Name_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_City_Name_CityId_LangCode",
                table: "City_Name",
                columns: new[] { "CityId", "LangCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_City_Name_LangCode",
                table: "City_Name",
                column: "LangCode");
        }
    }
}
