using Microsoft.EntityFrameworkCore.Migrations;
using SNEL.WalletPlatform.Infrastructure.Persistence.Seed;

#nullable disable

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class UpdateAfrimonyProductionConfig : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        var configJson = AfrimonyPaymentSeedData.BuildPaymentMethodConfigJson()
            .Replace("'", "''", StringComparison.Ordinal);
        var seedAt = AfrimonyPaymentSeedData.UpdatedAtUtc.ToString("yyyy-MM-dd HH:mm:sszzz");

        migrationBuilder.Sql(
            "UPDATE \"PaymentMethods\" pm " +
            "SET \"Config\" = '" + configJson + "'::jsonb, " +
            "\"UpdatedAtUtc\" = '" + seedAt + "', " +
            "\"UpdatedBy\" = 'SYSTEM' " +
            "FROM \"PaymentProviders\" pp " +
            "WHERE pm.\"PaymentProviderId\" = pp.\"Id\" " +
            $"AND pp.\"Code\" = '{AfrimonyPaymentSeedData.ProviderCode}' " +
            $"AND pm.\"Code\" = '{AfrimonyPaymentSeedData.MethodCode}';");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        var legacyConfigJson = """
            {
              "fields": {
                "afrimony_base_url": "https://api.sandbox.afrimoney.cd",
                "oauth_url": "https://api.sandbox.afrimoney.cd/v1/oauth2/",
                "oauth_basic": "c2U3OE9JcnB4cEVPeDhDRHdBcjBQY3pnb213YTp6ejN0bEhhWndjdnZtM1JPZHVDanhyQk95ZlFh",
                "id_value": "0907877635",
                "mpin": "1235",
                "product_id": "12",
                "initiator": "transactor",
                "bearer_code": "USSD"
              },
              "status_polling": {
                "total_attempts": 10,
                "initial_delay_seconds": 600,
                "interval_seconds": 60
              }
            }
            """
            .Replace("'", "''", StringComparison.Ordinal);
        var seedAt = AfrimonyPaymentSeedData.UpdatedAtUtc.ToString("yyyy-MM-dd HH:mm:sszzz");

        migrationBuilder.Sql(
            "UPDATE \"PaymentMethods\" pm " +
            "SET \"Config\" = '" + legacyConfigJson + "'::jsonb, " +
            "\"UpdatedAtUtc\" = '" + seedAt + "', " +
            "\"UpdatedBy\" = 'SYSTEM' " +
            "FROM \"PaymentProviders\" pp " +
            "WHERE pm.\"PaymentProviderId\" = pp.\"Id\" " +
            $"AND pp.\"Code\" = '{AfrimonyPaymentSeedData.ProviderCode}' " +
            $"AND pm.\"Code\" = '{AfrimonyPaymentSeedData.MethodCode}';");
    }
}
