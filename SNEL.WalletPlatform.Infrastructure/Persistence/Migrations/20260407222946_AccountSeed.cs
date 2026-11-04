using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AccountSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AccountTypes",
                columns: new[] { "Id", "Active", "Code", "Name", "Order" },
                values: new object[] { 5, true, "BANK", "Bank", 5 });

            migrationBuilder.InsertData(
                table: "PartnerTypes",
                columns: new[] { "Id", "Active", "Code", "Name", "Order" },
                values: new object[] { 4, true, "PLATFORM", "Platform", 4 });

            migrationBuilder.InsertData(
                table: "AccountTypeTranslations",
                columns: new[] { "Id", "EntityId", "LanguageCode", "Name" },
                values: new object[] { 5, 5, "fr", "Banque" });

            migrationBuilder.InsertData(
                table: "PartnerTypeTranslations",
                columns: new[] { "Id", "EntityId", "LanguageCode", "Name" },
                values: new object[] { 4, 4, "fr", "Plateforme" });

            migrationBuilder.InsertData(
                table: "Partners",
                columns: new[] { "Id", "Active", "CreatedAtUtc", "CreatedBy", "DisplayName", "LegalName", "PartnerTypeId", "UpdatedAtUtc", "UpdatedBy" },
                values: new object[] { 1L, true, new DateTimeOffset(new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "SYSTEM", "SNEL Wallet Platform", "SNEL Wallet Platform", 4, new DateTimeOffset(new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "SYSTEM" });

            migrationBuilder.InsertData(
                table: "Accounts",
                columns: new[] { "Id", "AccountTypeId", "Balance", "CreatedAtUtc", "CreatedBy", "CurrencyId", "Name", "Order", "PartnerId", "UpdatedAtUtc", "UpdatedBy", "Version" },
                values: new object[,]
                {
                    { new Guid("0f3e0d8d-2748-4afd-966b-1c25a8981867"), 3, 0m, new DateTimeOffset(new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "SYSTEM", 2, "TAX Payable", 2, 1L, new DateTimeOffset(new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "SYSTEM", 0 },
                    { new Guid("20391f85-3759-426b-8490-71b6d6d05df2"), 3, 0m, new DateTimeOffset(new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "SYSTEM", 1, "TAX Payable", 1, 1L, new DateTimeOffset(new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "SYSTEM", 0 },
                    { new Guid("28fefd2d-9222-4e3e-86cb-15d6c2979c9e"), 4, 0m, new DateTimeOffset(new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "SYSTEM", 2, "Platform Revenue", 2, 1L, new DateTimeOffset(new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "SYSTEM", 0 },
                    { new Guid("45eebeaf-e6f1-4934-9010-21d7c9ab69d3"), 2, 0m, new DateTimeOffset(new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "SYSTEM", 1, "TAX Expenses", 1, 1L, new DateTimeOffset(new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "SYSTEM", 0 },
                    { new Guid("4eda20e4-094d-4e44-bb9d-6757d9bb4490"), 5, 0m, new DateTimeOffset(new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "SYSTEM", 2, "Platform Bank Account", 2, 1L, new DateTimeOffset(new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "SYSTEM", 0 },
                    { new Guid("5116ca27-8161-4a59-84de-eb824aacd5a1"), 2, 0m, new DateTimeOffset(new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "SYSTEM", 2, "TAX Expenses", 2, 1L, new DateTimeOffset(new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "SYSTEM", 0 },
                    { new Guid("703751f9-32ef-4460-b315-840353d05234"), 2, 0m, new DateTimeOffset(new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "SYSTEM", 2, "Payment Cost", 2, 1L, new DateTimeOffset(new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "SYSTEM", 0 },
                    { new Guid("7638d5e8-1a70-459e-a918-0606ab1671ac"), 2, 0m, new DateTimeOffset(new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "SYSTEM", 1, "Payment Cost", 1, 1L, new DateTimeOffset(new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "SYSTEM", 0 },
                    { new Guid("a60adf4a-fc8b-440f-a69a-0dc16b2c7ff4"), 1, 0m, new DateTimeOffset(new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "SYSTEM", 2, "Platform Cash", 2, 1L, new DateTimeOffset(new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "SYSTEM", 0 },
                    { new Guid("ae7c6cc2-3566-460b-aca5-f35439dc874a"), 2, 0m, new DateTimeOffset(new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "SYSTEM", 1, "TOPUP Cost", 1, 1L, new DateTimeOffset(new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "SYSTEM", 0 },
                    { new Guid("b63f912e-cdfa-482c-88a0-7a6deadb189a"), 2, 0m, new DateTimeOffset(new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "SYSTEM", 2, "TOPUP Cost", 2, 1L, new DateTimeOffset(new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "SYSTEM", 0 },
                    { new Guid("b68d7723-039b-4699-a3ad-fcc53a33795a"), 4, 0m, new DateTimeOffset(new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "SYSTEM", 1, "Platform Revenue", 1, 1L, new DateTimeOffset(new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "SYSTEM", 0 },
                    { new Guid("da5d1565-6352-487e-b17c-e819c168616f"), 1, 0m, new DateTimeOffset(new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "SYSTEM", 1, "Platform Cash", 1, 1L, new DateTimeOffset(new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "SYSTEM", 0 },
                    { new Guid("ea6a0d62-4c39-4f97-9aa6-318c8fb6c5fb"), 5, 0m, new DateTimeOffset(new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "SYSTEM", 1, "Platform Bank Account", 1, 1L, new DateTimeOffset(new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "SYSTEM", 0 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AccountTypeTranslations",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("0f3e0d8d-2748-4afd-966b-1c25a8981867"));

            migrationBuilder.DeleteData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("20391f85-3759-426b-8490-71b6d6d05df2"));

            migrationBuilder.DeleteData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("28fefd2d-9222-4e3e-86cb-15d6c2979c9e"));

            migrationBuilder.DeleteData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("45eebeaf-e6f1-4934-9010-21d7c9ab69d3"));

            migrationBuilder.DeleteData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("4eda20e4-094d-4e44-bb9d-6757d9bb4490"));

            migrationBuilder.DeleteData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("5116ca27-8161-4a59-84de-eb824aacd5a1"));

            migrationBuilder.DeleteData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("703751f9-32ef-4460-b315-840353d05234"));

            migrationBuilder.DeleteData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("7638d5e8-1a70-459e-a918-0606ab1671ac"));

            migrationBuilder.DeleteData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("a60adf4a-fc8b-440f-a69a-0dc16b2c7ff4"));

            migrationBuilder.DeleteData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("ae7c6cc2-3566-460b-aca5-f35439dc874a"));

            migrationBuilder.DeleteData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("b63f912e-cdfa-482c-88a0-7a6deadb189a"));

            migrationBuilder.DeleteData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("b68d7723-039b-4699-a3ad-fcc53a33795a"));

            migrationBuilder.DeleteData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("da5d1565-6352-487e-b17c-e819c168616f"));

            migrationBuilder.DeleteData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("ea6a0d62-4c39-4f97-9aa6-318c8fb6c5fb"));

            migrationBuilder.DeleteData(
                table: "PartnerTypeTranslations",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "AccountTypes",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Partners",
                keyColumn: "Id",
                keyValue: 1L);

            migrationBuilder.DeleteData(
                table: "PartnerTypes",
                keyColumn: "Id",
                keyValue: 4);
        }
    }
}
