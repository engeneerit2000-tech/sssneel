using Microsoft.EntityFrameworkCore;
using SNEL.WalletPlatform.Domain.Payments;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Seed;

public static class PaymentProviderRouterSeedExtensions
{
    private static readonly DateTimeOffset SeedTimestamp =
        new(2026, 8, 19, 12, 0, 0, TimeSpan.Zero);

    public static ModelBuilder SeedPaymentProviderRouters(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PaymentProviderRouterEntity>().HasData(
            new
            {
                Id = 1,
                CountryCode = "243",
                CompanyPrefix = "90",
                PaymentProviderCode = AfrimonyPaymentSeedData.ProviderCode,
                PaymentMethodCode = AfrimonyPaymentSeedData.MethodCode,
                CreatedAtUtc = SeedTimestamp,
                UpdatedAtUtc = SeedTimestamp,
                CreatedBy = "SYSTEM",
                UpdatedBy = "SYSTEM"
            },
            new
            {
                Id = 2,
                CountryCode = "243",
                CompanyPrefix = "91",
                PaymentProviderCode = AfrimonyPaymentSeedData.ProviderCode,
                PaymentMethodCode = AfrimonyPaymentSeedData.MethodCode,
                CreatedAtUtc = SeedTimestamp,
                UpdatedAtUtc = SeedTimestamp,
                CreatedBy = "SYSTEM",
                UpdatedBy = "SYSTEM"
            });

        return modelBuilder;
    }
}
