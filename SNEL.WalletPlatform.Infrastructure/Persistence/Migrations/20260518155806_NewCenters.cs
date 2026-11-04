using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class NewCenters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("update \"Invoices\" set \"BillingRef\" = concat('06-03-', substring(\"BillingRef\", 4)) where \"BillingRef\" like '06-%' and \"BillingRef\" not like '06-03-%' and \"BillingRef\" not like '06-05-%'");
            migrationBuilder.Sql("update \"CustomerSubscriptions\" set \"BillingRefValue\" = concat('06-03-', substring(\"BillingRefValue\", 4)),\"CustomerRef\" = concat('06-03-', substring(\"BillingRefValue\", 4)) where \"BillingRefValue\" like '06-%' and \"BillingRefValue\" not like '06-05-%' and \"BillingRefValue\" not like '06-03-%'");
            migrationBuilder.Sql("update \"PaymentOrderItems\" set \"BillingRefValue\" = concat('06-03-', substring(\"BillingRefValue\", 4))\r\nwhere \r\n\"BillingRefValue\" like '06-%' and \"BillingRefValue\" not like '06-05-%'");
            migrationBuilder.Sql("update \"BillerCustomers\" set \"Number\" = concat('06-03-', \"Number\")");
            migrationBuilder.UpdateData(
                table: "Centers",
                keyColumn: "Id",
                keyValue: 1,
                column: "Code",
                value: "06-03");

            migrationBuilder.InsertData(
                table: "Centers",
                columns: new[] { "Id", "Active", "Code", "Name", "Order" },
                values: new object[] { 2, true, "06-05", "Kinshasa-Center", 2 });

            migrationBuilder.InsertData(
                table: "CenterTranslations",
                columns: new[] { "Id", "EntityId", "LanguageCode", "Name" },
                values: new object[] { 2, 2, "fr", "Kinshasa-Centre" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("update \"Invoices\" set \"BillingRef\" = concat('06-', substring(\"BillingRef\", 7)) where \"BillingRef\" like '06-03-%'");
            migrationBuilder.Sql("update \"CustomerSubscriptions\" set \"BillingRefValue\" = concat('06-', substring(\"BillingRefValue\", 7)), \"CustomerRef\" = concat('06-', substring(\"BillingRefValue\", 7)) where \"BillingRefValue\" like '06-03-%' or  \"BillingRefValue\" like '06-05-%'");
            migrationBuilder.Sql("update \"PaymentOrderItems\" set \"BillingRefValue\" = concat('06-', substring(\"BillingRefValue\", 7)) where \"BillingRefValue\" like '06-03-%'");

            migrationBuilder.DeleteData(
                table: "CenterTranslations",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Centers",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.UpdateData(
                table: "Centers",
                keyColumn: "Id",
                keyValue: 1,
                column: "Code",
                value: "06");
        }
    }
}
