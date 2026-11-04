using Microsoft.EntityFrameworkCore.Migrations;
using SNEL.WalletPlatform.Infrastructure.Persistence.Seed;

#nullable disable

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class MoveAkibaPayManagementAndPollingToPaymentMethodConfig : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        var configJson = AkibaPayPaymentSeedData.BuildPaymentMethodConfigJson()
            .Replace("'", "''", StringComparison.Ordinal);
        var seedAt = AkibaPayPaymentSeedData.UpdatedAtUtc.ToString("yyyy-MM-dd HH:mm:sszzz");

        migrationBuilder.Sql(
            "UPDATE \"PaymentMethods\" pm " +
            "SET \"Config\" = '" + configJson + "'::jsonb, " +
            "\"UpdatedAtUtc\" = '" + seedAt + "', " +
            "\"UpdatedBy\" = 'SYSTEM' " +
            "FROM \"PaymentProviders\" pp " +
            "WHERE pm.\"PaymentProviderId\" = pp.\"Id\" " +
            $"AND pp.\"Code\" = '{AkibaPayPaymentSeedData.ProviderCode}' " +
            $"AND pm.\"Code\" = '{AkibaPayPaymentSeedData.MethodCode}';");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        var legacyConfigJson = """
            {
              "fields": {
                "akibapay_base_url": "https://akibapay.brainstormtechs.com/api/gateway/",
                "merchant_id": "08de9200-5e9f-4660-8f54-327591d06a7e",
                "merchant_pass": "IbnjCLbL9Y31",
                "callback_url": "https://sneldev-walletplatform.fivectech.com/api/v1/webhooks/akibapay"
              }
            }
            """
            .Replace("'", "''", StringComparison.Ordinal);
        var seedAt = AkibaPayPaymentSeedData.UpdatedAtUtc.ToString("yyyy-MM-dd HH:mm:sszzz");

        migrationBuilder.Sql(
            "UPDATE \"PaymentMethods\" pm " +
            "SET \"Config\" = '" + legacyConfigJson + "'::jsonb, " +
            "\"UpdatedAtUtc\" = '" + seedAt + "', " +
            "\"UpdatedBy\" = 'SYSTEM' " +
            "FROM \"PaymentProviders\" pp " +
            "WHERE pm.\"PaymentProviderId\" = pp.\"Id\" " +
            $"AND pp.\"Code\" = '{AkibaPayPaymentSeedData.ProviderCode}' " +
            $"AND pm.\"Code\" = '{AkibaPayPaymentSeedData.MethodCode}';");
    }
}
