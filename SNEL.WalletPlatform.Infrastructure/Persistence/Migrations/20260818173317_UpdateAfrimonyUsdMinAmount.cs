using System.Globalization;
using Microsoft.EntityFrameworkCore.Migrations;
using SNEL.WalletPlatform.Infrastructure.Persistence.Seed;

#nullable disable

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class UpdateAfrimonyUsdMinAmount : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        var seedAt = AfrimonyPaymentSeedData.UpdatedAtUtc.ToString("yyyy-MM-dd HH:mm:sszzz");

        migrationBuilder.Sql(
            "UPDATE \"PaymentMethodCurrencies\" pmc " +
            $"SET \"Min\" = {AfrimonyPaymentSeedData.UsdMinAmount.ToString(CultureInfo.InvariantCulture)}, " +
            "\"UpdatedAtUtc\" = '" + seedAt + "', " +
            "\"UpdatedBy\" = 'SYSTEM' " +
            "FROM \"PaymentMethods\" pm " +
            "INNER JOIN \"PaymentProviders\" pp ON pm.\"PaymentProviderId\" = pp.\"Id\" " +
            "WHERE pmc.\"PaymentMethodId\" = pm.\"Id\" " +
            "AND pmc.\"CurrencyId\" = 2 " +
            $"AND pp.\"Code\" = '{AfrimonyPaymentSeedData.ProviderCode}' " +
            $"AND pm.\"Code\" = '{AfrimonyPaymentSeedData.MethodCode}';");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        var seedAt = AfrimonyPaymentSeedData.UpdatedAtUtc.ToString("yyyy-MM-dd HH:mm:sszzz");

        migrationBuilder.Sql(
            "UPDATE \"PaymentMethodCurrencies\" pmc " +
            "SET \"Min\" = 1, " +
            "\"UpdatedAtUtc\" = '" + seedAt + "', " +
            "\"UpdatedBy\" = 'SYSTEM' " +
            "FROM \"PaymentMethods\" pm " +
            "INNER JOIN \"PaymentProviders\" pp ON pm.\"PaymentProviderId\" = pp.\"Id\" " +
            "WHERE pmc.\"PaymentMethodId\" = pm.\"Id\" " +
            "AND pmc.\"CurrencyId\" = 2 " +
            $"AND pp.\"Code\" = '{AfrimonyPaymentSeedData.ProviderCode}' " +
            $"AND pm.\"Code\" = '{AfrimonyPaymentSeedData.MethodCode}';");
    }
}
