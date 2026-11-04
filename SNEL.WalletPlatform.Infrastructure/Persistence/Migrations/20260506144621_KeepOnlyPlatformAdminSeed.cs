using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class KeepOnlyPlatformAdminSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserNotificationPreferences",
                keyColumn: "Id",
                keyValue: new Guid("0da1a5bb-8ca2-41d3-8a6c-5d82a8d48f97"));

            migrationBuilder.DeleteData(
                table: "UserNotificationPreferences",
                keyColumn: "Id",
                keyValue: new Guid("1cc713d5-d80a-4204-b1db-dbd3afd98ed5"));

            migrationBuilder.DeleteData(
                table: "UserNotificationPreferences",
                keyColumn: "Id",
                keyValue: new Guid("38a7dd67-e77d-4fa0-a6c7-f4cd30f4deea"));

            migrationBuilder.DeleteData(
                table: "UserNotificationPreferences",
                keyColumn: "Id",
                keyValue: new Guid("7fdf79d5-a54c-4fef-a0f2-d70fea0f3d0e"));

            migrationBuilder.DeleteData(
                table: "UserNotificationPreferences",
                keyColumn: "Id",
                keyValue: new Guid("bc0d14f2-b5f3-4ba1-a6be-f8ba423f98ea"));

            migrationBuilder.DeleteData(
                table: "UserNotificationPreferences",
                keyColumn: "Id",
                keyValue: new Guid("f1402a8e-1523-46e4-9d64-e77f2ff4a81b"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("98f00742-f0d4-4ca8-a4ca-24769e585f02"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("db8dd87c-1dc0-4d95-9b2f-540d75ea80bf"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAtUtc", "CreatedBy", "Email", "FirstName", "GenderId", "IamRef", "Language", "LastName", "MobileNo", "NationalNo", "PartnerId", "SecondName", "Status", "ThirdName", "UpdatedAtUtc", "UpdatedBy", "Username" },
                values: new object[,]
                {
                    { new Guid("98f00742-f0d4-4ca8-a4ca-24769e585f02"), new DateTimeOffset(new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "SYSTEM", "biller.admin@snel.local", "Biller", 1, "seed-biller-admin", "en", "Admin", "+243900000013", "BILLER_ADMIN", 13L, null, 1, null, new DateTimeOffset(new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "SYSTEM", "biller.admin" },
                    { new Guid("db8dd87c-1dc0-4d95-9b2f-540d75ea80bf"), new DateTimeOffset(new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "SYSTEM", "psp.admin@snel.local", "PSP", 1, "seed-psp-admin", "en", "Admin", "+243900000011", "PSP_ADMIN", 11L, null, 1, null, new DateTimeOffset(new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "SYSTEM", "psp.admin" }
                });

            migrationBuilder.InsertData(
                table: "UserNotificationPreferences",
                columns: new[] { "Id", "Active", "CreatedAtUtc", "CreatedBy", "NotificationChannel", "UpdatedAtUtc", "UpdatedBy", "UserId" },
                values: new object[,]
                {
                    { new Guid("0da1a5bb-8ca2-41d3-8a6c-5d82a8d48f97"), true, new DateTimeOffset(new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "SYSTEM", 1, new DateTimeOffset(new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "SYSTEM", new Guid("db8dd87c-1dc0-4d95-9b2f-540d75ea80bf") },
                    { new Guid("1cc713d5-d80a-4204-b1db-dbd3afd98ed5"), true, new DateTimeOffset(new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "SYSTEM", 2, new DateTimeOffset(new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "SYSTEM", new Guid("db8dd87c-1dc0-4d95-9b2f-540d75ea80bf") },
                    { new Guid("38a7dd67-e77d-4fa0-a6c7-f4cd30f4deea"), true, new DateTimeOffset(new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "SYSTEM", 2, new DateTimeOffset(new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "SYSTEM", new Guid("98f00742-f0d4-4ca8-a4ca-24769e585f02") },
                    { new Guid("7fdf79d5-a54c-4fef-a0f2-d70fea0f3d0e"), true, new DateTimeOffset(new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "SYSTEM", 3, new DateTimeOffset(new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "SYSTEM", new Guid("db8dd87c-1dc0-4d95-9b2f-540d75ea80bf") },
                    { new Guid("bc0d14f2-b5f3-4ba1-a6be-f8ba423f98ea"), true, new DateTimeOffset(new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "SYSTEM", 3, new DateTimeOffset(new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "SYSTEM", new Guid("98f00742-f0d4-4ca8-a4ca-24769e585f02") },
                    { new Guid("f1402a8e-1523-46e4-9d64-e77f2ff4a81b"), true, new DateTimeOffset(new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "SYSTEM", 1, new DateTimeOffset(new DateTime(2026, 4, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "SYSTEM", new Guid("98f00742-f0d4-4ca8-a4ca-24769e585f02") }
                });
        }
    }
}
