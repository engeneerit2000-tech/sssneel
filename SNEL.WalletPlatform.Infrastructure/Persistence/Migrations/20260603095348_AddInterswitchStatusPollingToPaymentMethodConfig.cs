using Microsoft.EntityFrameworkCore.Migrations;
using SNEL.WalletPlatform.Infrastructure.Persistence.Seed;

#nullable disable

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddInterswitchStatusPollingToPaymentMethodConfig : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        var statusPollingJson = InterswitchPaymentSeedData.DefaultStatusPollingJson
            .Replace("'", "''", StringComparison.Ordinal);

        migrationBuilder.Sql(
            "UPDATE \"PaymentMethods\" pm " +
            "SET \"Config\" = jsonb_set(" +
            "COALESCE(pm.\"Config\"::jsonb, '{}'::jsonb), " +
            "'{status_polling}', " +
            $"'{statusPollingJson}'::jsonb, " +
            "true), " +
            "\"UpdatedAtUtc\" = NOW(), " +
            "\"UpdatedBy\" = 'SYSTEM' " +
            "FROM \"PaymentProviders\" pp " +
            "WHERE pm.\"PaymentProviderId\" = pp.\"Id\" " +
            $"AND UPPER(pp.\"Code\") = '{InterswitchPaymentSeedData.ProviderCode}' " +
            "AND (pm.\"Config\" IS NULL OR NOT (pm.\"Config\"::jsonb ? 'status_polling'));");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            "UPDATE \"PaymentMethods\" pm " +
            "SET \"Config\" = pm.\"Config\"::jsonb - 'status_polling', " +
            "\"UpdatedAtUtc\" = NOW(), " +
            "\"UpdatedBy\" = 'SYSTEM' " +
            "FROM \"PaymentProviders\" pp " +
            "WHERE pm.\"PaymentProviderId\" = pp.\"Id\" " +
            $"AND UPPER(pp.\"Code\") = '{InterswitchPaymentSeedData.ProviderCode}' " +
            "AND pm.\"Config\" IS NOT NULL " +
            "AND (pm.\"Config\"::jsonb ? 'status_polling');");
    }
}
