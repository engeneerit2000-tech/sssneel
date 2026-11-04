using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddQueryPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAtUtc", "CreatedBy", "Email", "FirstName", "GenderId", "IamRef", "Language", "LastName", "MobileNo", "NationalNo", "PartnerId", "SecondName", "Status", "ThirdName", "UpdatedAtUtc", "UpdatedBy", "Username" },
                values: new object[] { new Guid("c9f95a1f-a0ad-4fb6-9a11-62d2d2e6ad01"), new DateTimeOffset(new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "SYSTEM", "platform.admin@snel.local", "Platform", 1, "seed-platform-admin", "en", "Admin", "+243900000001", "PLATFORM_ADMIN", 1L, null, 1, null, new DateTimeOffset(new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "SYSTEM", "platform.admin" });

            migrationBuilder.InsertData(
                table: "UserNotificationPreferences",
                columns: new[] { "Id", "Active", "CreatedAtUtc", "CreatedBy", "NotificationChannel", "UpdatedAtUtc", "UpdatedBy", "UserId" },
                values: new object[,]
                {
                    { new Guid("32a97d01-bf0f-4d6f-9f42-61c8f547f8aa"), true, new DateTimeOffset(new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "SYSTEM", 3, new DateTimeOffset(new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "SYSTEM", new Guid("c9f95a1f-a0ad-4fb6-9a11-62d2d2e6ad01") },
                    { new Guid("6dca5ca7-cdb4-43e6-af3f-7ece37bfec4f"), true, new DateTimeOffset(new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "SYSTEM", 2, new DateTimeOffset(new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "SYSTEM", new Guid("c9f95a1f-a0ad-4fb6-9a11-62d2d2e6ad01") },
                    { new Guid("e6f34f71-09dc-4f31-a295-cf9ea4f43601"), true, new DateTimeOffset(new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "SYSTEM", 1, new DateTimeOffset(new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "SYSTEM", new Guid("c9f95a1f-a0ad-4fb6-9a11-62d2d2e6ad01") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Users_MobileNo",
                table: "Users",
                column: "MobileNo");

            migrationBuilder.CreateIndex(
                name: "IX_Users_NationalNo",
                table: "Users",
                column: "NationalNo");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Status",
                table: "Users",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username");

            migrationBuilder.CreateIndex(
                name: "IX_UserCloudMessagingTokens_ExpiresAt",
                table: "UserCloudMessagingTokens",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_UserCloudMessagingTokens_TokenId",
                table: "UserCloudMessagingTokens",
                column: "TokenId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentOrderItems_Status",
                table: "PaymentOrderItems",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_AmountOutstanding",
                table: "Invoices",
                column: "AmountOutstanding");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_BillingServiceId_BillingRef",
                table: "Invoices",
                columns: new[] { "BillingServiceId", "BillingRef" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_MobileNo",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_NationalNo",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_Status",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_Username",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_UserCloudMessagingTokens_ExpiresAt",
                table: "UserCloudMessagingTokens");

            migrationBuilder.DropIndex(
                name: "IX_UserCloudMessagingTokens_TokenId",
                table: "UserCloudMessagingTokens");

            migrationBuilder.DropIndex(
                name: "IX_PaymentOrderItems_Status",
                table: "PaymentOrderItems");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_AmountOutstanding",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_BillingServiceId_BillingRef",
                table: "Invoices");

            migrationBuilder.DeleteData(
                table: "UserNotificationPreferences",
                keyColumn: "Id",
                keyValue: new Guid("32a97d01-bf0f-4d6f-9f42-61c8f547f8aa"));

            migrationBuilder.DeleteData(
                table: "UserNotificationPreferences",
                keyColumn: "Id",
                keyValue: new Guid("6dca5ca7-cdb4-43e6-af3f-7ece37bfec4f"));

            migrationBuilder.DeleteData(
                table: "UserNotificationPreferences",
                keyColumn: "Id",
                keyValue: new Guid("e6f34f71-09dc-4f31-a295-cf9ea4f43601"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("c9f95a1f-a0ad-4fb6-9a11-62d2d2e6ad01"));
        }
    }
}
