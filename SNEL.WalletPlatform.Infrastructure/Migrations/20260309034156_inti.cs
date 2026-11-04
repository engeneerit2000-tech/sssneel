using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SNEL.WalletPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class inti : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccountTypes",
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
                    table.PrimaryKey("PK_AccountTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BillingCategories",
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
                    table.PrimaryKey("PK_BillingCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BillingServiceTypes",
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
                    table.PrimaryKey("PK_BillingServiceTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Channels",
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
                    table.PrimaryKey("PK_Channels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContactTypes",
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
                    table.PrimaryKey("PK_ContactTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Countries",
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
                    table.PrimaryKey("PK_Countries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Currencies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Abbreviation = table.Column<string>(type: "text", nullable: false),
                    Minor = table.Column<int>(type: "integer", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Currencies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Genders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Genders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InterswitchWebhookEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Event = table.Column<string>(type: "text", nullable: false),
                    Uuid = table.Column<string>(type: "text", nullable: false),
                    Timestamp = table.Column<long>(type: "bigint", nullable: false),
                    DataJson = table.Column<JsonElement>(type: "jsonb", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterswitchWebhookEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceStagings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OracleInvoiceId = table.Column<decimal>(type: "numeric(18,0)", precision: 18, scale: 0, nullable: true),
                    InvoiceNum = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    InvoiceDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    InvoiceUpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CustomerId = table.Column<decimal>(type: "numeric(18,0)", precision: 18, scale: 0, nullable: true),
                    CustomerNum = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    CustomerName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CustomerTypeCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CustomerTypeName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    TarifType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BillingPeriod = table.Column<decimal>(type: "numeric(18,0)", precision: 18, scale: 0, nullable: true),
                    Consumption = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    BilledConsuption = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    InvoiceCurr = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    AmountUsd = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    AmountFc = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    PaidUsd = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    PaidFc = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    PaymentUpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Vat = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    ProvincialTax = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    PaymentsJson = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceStagings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JournalLineTypes",
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
                    table.PrimaryKey("PK_JournalLineTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JournalTypes",
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
                    table.PrimaryKey("PK_JournalTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PartnerTypes",
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
                    table.PrimaryKey("PK_PartnerTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentCategories",
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
                    table.PrimaryKey("PK_PaymentCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AccountTypeTranslations",
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
                    table.PrimaryKey("PK_AccountTypeTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountTypeTranslations_AccountTypes_EntityId",
                        column: x => x.EntityId,
                        principalTable: "AccountTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Regions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CountryId = table.Column<int>(type: "integer", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Regions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Regions_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IamRef = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SecondName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ThirdName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    GenderId = table.Column<int>(type: "integer", nullable: false),
                    MobileNo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    NationalNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Genders_GenderId",
                        column: x => x.GenderId,
                        principalTable: "Genders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Journals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JournalTypeId = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    SourceType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SourceRef = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    JournalTimestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Journals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Journals_JournalTypes_JournalTypeId",
                        column: x => x.JournalTypeId,
                        principalTable: "JournalTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Partners",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PartnerTypeId = table.Column<int>(type: "integer", nullable: false),
                    LegalName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Partners", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Partners_PartnerTypes_PartnerTypeId",
                        column: x => x.PartnerTypeId,
                        principalTable: "PartnerTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Cities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RegionId = table.Column<int>(type: "integer", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cities_Regions_RegionId",
                        column: x => x.RegionId,
                        principalTable: "Regions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserCloudMessagingTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenId = table.Column<string>(type: "text", nullable: false),
                    PlatformType = table.Column<int>(type: "integer", nullable: false),
                    DeviceName = table.Column<string>(type: "text", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCloudMessagingTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserCloudMessagingTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AccountTypeId = table.Column<int>(type: "integer", nullable: false),
                    PartnerId = table.Column<long>(type: "bigint", nullable: false),
                    CurrencyId = table.Column<int>(type: "integer", nullable: false),
                    Balance = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Accounts_AccountTypes_AccountTypeId",
                        column: x => x.AccountTypeId,
                        principalTable: "AccountTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Accounts_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Accounts_Partners_PartnerId",
                        column: x => x.PartnerId,
                        principalTable: "Partners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Billers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PartnerId = table.Column<long>(type: "bigint", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Billers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Billers_Partners_PartnerId",
                        column: x => x.PartnerId,
                        principalTable: "Partners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PartnerContacts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PartnerId = table.Column<long>(type: "bigint", nullable: false),
                    ContactTypeId = table.Column<int>(type: "integer", nullable: false),
                    ContactInfo = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartnerContacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartnerContacts_ContactTypes_ContactTypeId",
                        column: x => x.ContactTypeId,
                        principalTable: "ContactTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PartnerContacts_Partners_PartnerId",
                        column: x => x.PartnerId,
                        principalTable: "Partners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentProviders",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PartnerId = table.Column<long>(type: "bigint", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentProviders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentProviders_Partners_PartnerId",
                        column: x => x.PartnerId,
                        principalTable: "Partners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Areas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CityId = table.Column<int>(type: "integer", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Areas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Areas_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

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

            migrationBuilder.CreateTable(
                name: "JournalLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JournalId = table.Column<Guid>(type: "uuid", nullable: false),
                    JournalLineTypeId = table.Column<int>(type: "integer", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    Debit = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Credit = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Balance = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JournalLines_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JournalLines_JournalLineTypes_JournalLineTypeId",
                        column: x => x.JournalLineTypeId,
                        principalTable: "JournalLineTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JournalLines_Journals_JournalId",
                        column: x => x.JournalId,
                        principalTable: "Journals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BillingServices",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BillingCategoryId = table.Column<int>(type: "integer", nullable: false),
                    BillerId = table.Column<long>(type: "bigint", nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ServiceTypeId = table.Column<int>(type: "integer", nullable: false),
                    SupportPartial = table.Column<bool>(type: "boolean", nullable: false),
                    SupportInquiry = table.Column<bool>(type: "boolean", nullable: false),
                    BillingRefFormat = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillingServices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BillingServices_Billers_BillerId",
                        column: x => x.BillerId,
                        principalTable: "Billers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BillingServices_BillingCategories_BillingCategoryId",
                        column: x => x.BillingCategoryId,
                        principalTable: "BillingCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BillingServices_BillingServiceTypes_ServiceTypeId",
                        column: x => x.ServiceTypeId,
                        principalTable: "BillingServiceTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PaymentMethods",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PaymentCategoryId = table.Column<int>(type: "integer", nullable: false),
                    PaymentProviderId = table.Column<long>(type: "bigint", nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CheckoutConfig = table.Column<string>(type: "jsonb", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentMethods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentMethods_PaymentCategories_PaymentCategoryId",
                        column: x => x.PaymentCategoryId,
                        principalTable: "PaymentCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaymentMethods_PaymentProviders_PaymentProviderId",
                        column: x => x.PaymentProviderId,
                        principalTable: "PaymentProviders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartnerId = table.Column<long>(type: "bigint", nullable: false),
                    AreaId = table.Column<int>(type: "integer", nullable: true),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Language = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Customers_Areas_AreaId",
                        column: x => x.AreaId,
                        principalTable: "Areas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Customers_Partners_PartnerId",
                        column: x => x.PartnerId,
                        principalTable: "Partners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Customers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BillingServiceCurrencies",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CurrencyId = table.Column<int>(type: "integer", nullable: false),
                    BillingServiceId = table.Column<long>(type: "bigint", nullable: false),
                    Min = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Max = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CommissionType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CommissionValue = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    FeesType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FeesValue = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    PlatformShareType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PlatformShareValue = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillingServiceCurrencies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BillingServiceCurrencies_BillingServices_BillingServiceId",
                        column: x => x.BillingServiceId,
                        principalTable: "BillingServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BillingServiceCurrencies_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BillingServiceId = table.Column<long>(type: "bigint", nullable: false),
                    BillingRef = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    InvoiceRef = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    AmountOutstanding = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AmountPaid = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AmountTotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CurrencyId = table.Column<int>(type: "integer", nullable: false),
                    IssueDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DueDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    InvoiceInfo = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    PaymentMismatchFlag = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invoices_BillingServices_BillingServiceId",
                        column: x => x.BillingServiceId,
                        principalTable: "BillingServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invoices_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PaymentMethodCurrencies",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CurrencyId = table.Column<int>(type: "integer", nullable: false),
                    PaymentMethodId = table.Column<long>(type: "bigint", nullable: false),
                    Min = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Max = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    FeesType = table.Column<int>(type: "integer", nullable: true),
                    FeesValue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    CostType = table.Column<int>(type: "integer", nullable: true),
                    CostValue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    PlatformShareType = table.Column<int>(type: "integer", nullable: true),
                    PlatformShareValue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentMethodCurrencies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentMethodCurrencies_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaymentMethodCurrencies_PaymentMethods_PaymentMethodId",
                        column: x => x.PaymentMethodId,
                        principalTable: "PaymentMethods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CustomerNotificationPreferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    NotificationChannel = table.Column<int>(type: "integer", nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerNotificationPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerNotificationPreferences_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomerRegistrationHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    StatusTimestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    StatusNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerRegistrationHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerRegistrationHistories_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomerRegistrationHistories_Users_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CustomerSubscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    BillingServiceId = table.Column<long>(type: "bigint", nullable: false),
                    CustomerRef = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    BillingRefValue = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NickName = table.Column<string>(type: "text", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerSubscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerSubscriptions_BillingServices_BillingServiceId",
                        column: x => x.BillingServiceId,
                        principalTable: "BillingServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CustomerSubscriptions_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChannelId = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CurrencyId = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Fees = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Tax = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentOrders_Channels_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "Channels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaymentOrders_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaymentOrders_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PaymentOrderItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChannelId = table.Column<int>(type: "integer", nullable: false),
                    ItemType = table.Column<int>(type: "integer", nullable: false),
                    BillingServiceId = table.Column<long>(type: "bigint", nullable: true),
                    CustomerSubscriptionId = table.Column<Guid>(type: "uuid", nullable: true),
                    BillingRefValue = table.Column<string>(type: "text", nullable: true),
                    InvoiceId = table.Column<Guid>(type: "uuid", nullable: true),
                    WalletAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Fees = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Tax = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentOrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentOrderItems_BillingServices_BillingServiceId",
                        column: x => x.BillingServiceId,
                        principalTable: "BillingServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaymentOrderItems_Channels_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "Channels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaymentOrderItems_CustomerSubscriptions_CustomerSubscriptio~",
                        column: x => x.CustomerSubscriptionId,
                        principalTable: "CustomerSubscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaymentOrderItems_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaymentOrderItems_PaymentOrders_PaymentOrderId",
                        column: x => x.PaymentOrderId,
                        principalTable: "PaymentOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentMethodId = table.Column<long>(type: "bigint", nullable: false),
                    CurrencyId = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Tax = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ExternalRef = table.Column<string>(type: "text", nullable: true),
                    PaymentRef = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payments_PaymentMethods_PaymentMethodId",
                        column: x => x.PaymentMethodId,
                        principalTable: "PaymentMethods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payments_PaymentOrders_PaymentOrderId",
                        column: x => x.PaymentOrderId,
                        principalTable: "PaymentOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PaymentTxns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CurrencyId = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ExternalRef = table.Column<string>(type: "text", nullable: true),
                    ReqBody = table.Column<string>(type: "text", nullable: true),
                    ResBody = table.Column<string>(type: "text", nullable: true),
                    ResCode = table.Column<string>(type: "text", nullable: true),
                    ResMessage = table.Column<string>(type: "text", nullable: true),
                    PaymentEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentTxns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentTxns_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaymentTxns_Payments_PaymentEntityId",
                        column: x => x.PaymentEntityId,
                        principalTable: "Payments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PaymentTxns_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "AccountTypes",
                columns: new[] { "Id", "Active", "Code", "Name", "Order" },
                values: new object[,]
                {
                    { 1, true, "ASSETS", "Assets", 1 },
                    { 2, true, "EXPENSES", "Expenses", 2 },
                    { 3, true, "LIABILITY", "Liability", 3 },
                    { 4, true, "REVENUE", "Revenue", 4 }
                });

            migrationBuilder.InsertData(
                table: "BillingCategories",
                columns: new[] { "Id", "Active", "Code", "Name", "Order" },
                values: new object[] { 1, true, "UTILITY", "Utility", 1 });

            migrationBuilder.InsertData(
                table: "BillingServiceTypes",
                columns: new[] { "Id", "Active", "Code", "Name", "Order" },
                values: new object[,]
                {
                    { 1, true, "PREPAID", "Prepaid", 1 },
                    { 2, true, "POSTPAID", "Postpaid", 2 },
                    { 3, true, "BOTH", "Both", 3 }
                });

            migrationBuilder.InsertData(
                table: "Channels",
                columns: new[] { "Id", "Active", "Code", "Name", "Order" },
                values: new object[,]
                {
                    { 1, true, "WEB", "Web", 1 },
                    { 2, true, "MOBILE", "Mobile", 2 }
                });

            migrationBuilder.InsertData(
                table: "ContactTypes",
                columns: new[] { "Id", "Active", "Code", "Name", "Order" },
                values: new object[,]
                {
                    { 1, true, "PHONE", "Phone", 1 },
                    { 2, true, "MOBILE", "Mobile", 2 },
                    { 3, true, "EMAIL", "Email", 3 },
                    { 4, true, "POST", "Post ", 4 },
                    { 5, true, "FAX", "Fax", 5 }
                });

            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "Id", "Active", "Code", "Name", "Order" },
                values: new object[] { 1, true, "COD", "Democratic Republic of the Congo", 1 });

            migrationBuilder.InsertData(
                table: "Currencies",
                columns: new[] { "Id", "Abbreviation", "Active", "Code", "Minor", "Name", "Order" },
                values: new object[,]
                {
                    { 1, "CDF", true, "976", 2, "Congolese Franc", 1 },
                    { 2, "USD", true, "840", 2, "US Dollar", 2 }
                });

            migrationBuilder.InsertData(
                table: "Genders",
                columns: new[] { "Id", "Active", "Code", "Name", "Order" },
                values: new object[,]
                {
                    { 1, true, "MALE", "Male", 1 },
                    { 2, true, "FEMALE", "Female", 2 }
                });

            migrationBuilder.InsertData(
                table: "PartnerTypes",
                columns: new[] { "Id", "Active", "Code", "Name", "Order" },
                values: new object[,]
                {
                    { 1, true, "BILLER", "Biller", 1 },
                    { 2, true, "PSP", "Payment Provider", 2 },
                    { 3, true, "CUSTOMER", "Customer", 3 }
                });

            migrationBuilder.InsertData(
                table: "PaymentCategories",
                columns: new[] { "Id", "Active", "Code", "Name", "Order" },
                values: new object[] { 1, true, "WALLET", "Wallet", 1 });

            migrationBuilder.InsertData(
                table: "AccountTypeTranslations",
                columns: new[] { "Id", "EntityId", "LanguageCode", "Name" },
                values: new object[,]
                {
                    { 1, 1, "FR", "Actifs" },
                    { 2, 2, "FR", "Charges" },
                    { 3, 3, "FR", "Passif" },
                    { 4, 4, "FR", "Produits" }
                });

            migrationBuilder.InsertData(
                table: "Regions",
                columns: new[] { "Id", "Active", "Code", "CountryId", "Name", "Order" },
                values: new object[] { 1, true, "COD", 1, "Democratic Republic of the Congo", 1 });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "Active", "Code", "Name", "Order", "RegionId" },
                values: new object[] { 1, true, "KINSHASA", "Kinshasa", 1, 1 });

            migrationBuilder.InsertData(
                table: "Areas",
                columns: new[] { "Id", "Active", "CityId", "Code", "Name", "Order" },
                values: new object[,]
                {
                    { 1, true, 1, "EAST", "East", 1 },
                    { 2, true, 1, "WEST", "West", 2 },
                    { 3, true, 1, "NORTH", "North", 3 },
                    { 4, true, 1, "SOUTH", "South", 4 },
                    { 5, true, 1, "CENTER", "Center", 5 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_AccountTypeId",
                table: "Accounts",
                column: "AccountTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_CurrencyId",
                table: "Accounts",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_PartnerId",
                table: "Accounts",
                column: "PartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountTypes_Code",
                table: "AccountTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccountTypes_Name",
                table: "AccountTypes",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_AccountTypeTranslations_EntityId_LanguageCode",
                table: "AccountTypeTranslations",
                columns: new[] { "EntityId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccountTypeTranslations_LanguageCode",
                table: "AccountTypeTranslations",
                column: "LanguageCode");

            migrationBuilder.CreateIndex(
                name: "IX_AccountTypeTranslations_Name",
                table: "AccountTypeTranslations",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Areas_CityId_Code",
                table: "Areas",
                columns: new[] { "CityId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Areas_Name",
                table: "Areas",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Billers_Code",
                table: "Billers",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Billers_PartnerId",
                table: "Billers",
                column: "PartnerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BillingCategories_Code",
                table: "BillingCategories",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BillingCategories_Name",
                table: "BillingCategories",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_BillingServiceCurrencies_Active",
                table: "BillingServiceCurrencies",
                column: "Active");

            migrationBuilder.CreateIndex(
                name: "IX_BillingServiceCurrencies_BillingServiceId_CurrencyId",
                table: "BillingServiceCurrencies",
                columns: new[] { "BillingServiceId", "CurrencyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BillingServiceCurrencies_CurrencyId",
                table: "BillingServiceCurrencies",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_BillingServices_Active",
                table: "BillingServices",
                column: "Active");

            migrationBuilder.CreateIndex(
                name: "IX_BillingServices_BillerId_Code",
                table: "BillingServices",
                columns: new[] { "BillerId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BillingServices_BillingCategoryId",
                table: "BillingServices",
                column: "BillingCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_BillingServices_ServiceTypeId",
                table: "BillingServices",
                column: "ServiceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_BillingServiceTypes_Code",
                table: "BillingServiceTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BillingServiceTypes_Name",
                table: "BillingServiceTypes",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Channels_Code",
                table: "Channels",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Channels_Name",
                table: "Channels",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Cities_Name",
                table: "Cities",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Cities_RegionId_Code",
                table: "Cities",
                columns: new[] { "RegionId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_City_Name_CityId_LangCode",
                table: "City_Name",
                columns: new[] { "CityId", "LangCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_City_Name_LangCode",
                table: "City_Name",
                column: "LangCode");

            migrationBuilder.CreateIndex(
                name: "IX_ContactTypes_Code",
                table: "ContactTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContactTypes_Name",
                table: "ContactTypes",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Countries_Code",
                table: "Countries",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Countries_Name",
                table: "Countries",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Currencies_Code",
                table: "Currencies",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Currencies_Name",
                table: "Currencies",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerNotificationPreferences_CustomerId_NotificationChan~",
                table: "CustomerNotificationPreferences",
                columns: new[] { "CustomerId", "NotificationChannel" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerRegistrationHistories_CustomerId",
                table: "CustomerRegistrationHistories",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerRegistrationHistories_EmployeeId",
                table: "CustomerRegistrationHistories",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerRegistrationHistories_StatusTimestamp",
                table: "CustomerRegistrationHistories",
                column: "StatusTimestamp");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_AreaId",
                table: "Customers",
                column: "AreaId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_PartnerId",
                table: "Customers",
                column: "PartnerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_UserId",
                table: "Customers",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerSubscriptions_BillingServiceId",
                table: "CustomerSubscriptions",
                column: "BillingServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerSubscriptions_CustomerId",
                table: "CustomerSubscriptions",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerSubscriptions_CustomerId_BillingServiceId_BillingRe~",
                table: "CustomerSubscriptions",
                columns: new[] { "CustomerId", "BillingServiceId", "BillingRefValue" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InterswitchWebhookEvents_Event",
                table: "InterswitchWebhookEvents",
                column: "Event");

            migrationBuilder.CreateIndex(
                name: "IX_InterswitchWebhookEvents_Uuid",
                table: "InterswitchWebhookEvents",
                column: "Uuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_BillingRef",
                table: "Invoices",
                column: "BillingRef");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_BillingServiceId",
                table: "Invoices",
                column: "BillingServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_CurrencyId",
                table: "Invoices",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_InvoiceRef",
                table: "Invoices",
                column: "InvoiceRef",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_Status",
                table: "Invoices",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceStagings_InvoiceNum",
                table: "InvoiceStagings",
                column: "InvoiceNum");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceStagings_InvoiceUpdateDate",
                table: "InvoiceStagings",
                column: "InvoiceUpdateDate");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceStagings_OracleInvoiceId",
                table: "InvoiceStagings",
                column: "OracleInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalLines_AccountId",
                table: "JournalLines",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalLines_JournalId",
                table: "JournalLines",
                column: "JournalId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalLines_JournalLineTypeId",
                table: "JournalLines",
                column: "JournalLineTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalLineTypes_Code",
                table: "JournalLineTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JournalLineTypes_Name",
                table: "JournalLineTypes",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Journals_JournalTypeId",
                table: "Journals",
                column: "JournalTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalTypes_Code",
                table: "JournalTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JournalTypes_Name",
                table: "JournalTypes",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_PartnerContacts_ContactTypeId",
                table: "PartnerContacts",
                column: "ContactTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PartnerContacts_PartnerId",
                table: "PartnerContacts",
                column: "PartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Partners_Active",
                table: "Partners",
                column: "Active");

            migrationBuilder.CreateIndex(
                name: "IX_Partners_PartnerTypeId",
                table: "Partners",
                column: "PartnerTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PartnerTypes_Code",
                table: "PartnerTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PartnerTypes_Name",
                table: "PartnerTypes",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentCategories_Code",
                table: "PaymentCategories",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentCategories_Name",
                table: "PaymentCategories",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethodCurrencies_CurrencyId_PaymentMethodId",
                table: "PaymentMethodCurrencies",
                columns: new[] { "CurrencyId", "PaymentMethodId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethodCurrencies_PaymentMethodId",
                table: "PaymentMethodCurrencies",
                column: "PaymentMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethods_Code",
                table: "PaymentMethods",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethods_PaymentCategoryId",
                table: "PaymentMethods",
                column: "PaymentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethods_PaymentProviderId",
                table: "PaymentMethods",
                column: "PaymentProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentOrderItems_BillingServiceId",
                table: "PaymentOrderItems",
                column: "BillingServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentOrderItems_ChannelId",
                table: "PaymentOrderItems",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentOrderItems_CustomerSubscriptionId",
                table: "PaymentOrderItems",
                column: "CustomerSubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentOrderItems_InvoiceId",
                table: "PaymentOrderItems",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentOrderItems_PaymentOrderId",
                table: "PaymentOrderItems",
                column: "PaymentOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentOrders_ChannelId",
                table: "PaymentOrders",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentOrders_CurrencyId",
                table: "PaymentOrders",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentOrders_CustomerId",
                table: "PaymentOrders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentProviders_Code",
                table: "PaymentProviders",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentProviders_PartnerId",
                table: "PaymentProviders",
                column: "PartnerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_CurrencyId",
                table: "Payments",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PaymentMethodId",
                table: "Payments",
                column: "PaymentMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PaymentOrderId",
                table: "Payments",
                column: "PaymentOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTxns_CurrencyId",
                table: "PaymentTxns",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTxns_PaymentEntityId",
                table: "PaymentTxns",
                column: "PaymentEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTxns_PaymentId",
                table: "PaymentTxns",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_Regions_CountryId_Code",
                table: "Regions",
                columns: new[] { "CountryId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Regions_Name",
                table: "Regions",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_UserCloudMessagingTokens_UserId",
                table: "UserCloudMessagingTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_GenderId",
                table: "Users",
                column: "GenderId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_IamRef",
                table: "Users",
                column: "IamRef",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_MobileNo",
                table: "Users",
                column: "MobileNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_NationalNo",
                table: "Users",
                column: "NationalNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountTypeTranslations");

            migrationBuilder.DropTable(
                name: "BillingServiceCurrencies");

            migrationBuilder.DropTable(
                name: "City_Name");

            migrationBuilder.DropTable(
                name: "CustomerNotificationPreferences");

            migrationBuilder.DropTable(
                name: "CustomerRegistrationHistories");

            migrationBuilder.DropTable(
                name: "InterswitchWebhookEvents");

            migrationBuilder.DropTable(
                name: "InvoiceStagings");

            migrationBuilder.DropTable(
                name: "JournalLines");

            migrationBuilder.DropTable(
                name: "PartnerContacts");

            migrationBuilder.DropTable(
                name: "PaymentMethodCurrencies");

            migrationBuilder.DropTable(
                name: "PaymentOrderItems");

            migrationBuilder.DropTable(
                name: "PaymentTxns");

            migrationBuilder.DropTable(
                name: "UserCloudMessagingTokens");

            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "JournalLineTypes");

            migrationBuilder.DropTable(
                name: "Journals");

            migrationBuilder.DropTable(
                name: "ContactTypes");

            migrationBuilder.DropTable(
                name: "CustomerSubscriptions");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "AccountTypes");

            migrationBuilder.DropTable(
                name: "JournalTypes");

            migrationBuilder.DropTable(
                name: "BillingServices");

            migrationBuilder.DropTable(
                name: "PaymentMethods");

            migrationBuilder.DropTable(
                name: "PaymentOrders");

            migrationBuilder.DropTable(
                name: "Billers");

            migrationBuilder.DropTable(
                name: "BillingCategories");

            migrationBuilder.DropTable(
                name: "BillingServiceTypes");

            migrationBuilder.DropTable(
                name: "PaymentCategories");

            migrationBuilder.DropTable(
                name: "PaymentProviders");

            migrationBuilder.DropTable(
                name: "Channels");

            migrationBuilder.DropTable(
                name: "Currencies");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "Areas");

            migrationBuilder.DropTable(
                name: "Partners");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Cities");

            migrationBuilder.DropTable(
                name: "PartnerTypes");

            migrationBuilder.DropTable(
                name: "Genders");

            migrationBuilder.DropTable(
                name: "Regions");

            migrationBuilder.DropTable(
                name: "Countries");
        }
    }
}
