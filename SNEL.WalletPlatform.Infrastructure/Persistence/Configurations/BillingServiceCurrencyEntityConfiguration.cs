using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SNEL.WalletPlatform.Domain.Billing;
using SNEL.WalletPlatform.Infrastructure.Persistence.Configurations.Extensions;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Configurations;

public sealed class BillingServiceCurrencyEntityConfiguration : TrackedEntityConfiguration<BillingServiceCurrencyEntity,long>
{
    public override void Configure(EntityTypeBuilder<BillingServiceCurrencyEntity> builder)
    {
        base.Configure(builder);
        builder.Property(x => x.Id).UseIdentityByDefaultColumn().HasIdentityOptions(startValue: 1000);
        builder.Property(x => x.CurrencyId).IsRequired();
        builder.Property(x => x.BillingServiceId).IsRequired();

        builder.Property(x => x.Min)
            .HasPrecision(18, 6)
            .IsRequired();

        builder.Property(x => x.Max)
            .HasPrecision(18, 6)
            .IsRequired();

        builder.Property(x => x.CommissionType)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.CommissionValue)
            .HasPrecision(18, 6)
            .IsRequired();

        builder.Property(x => x.FeesType)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.FeesValue)
            .HasPrecision(18, 6)
            .IsRequired();

        builder.Property(x => x.CostType)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.CostValue)
            .HasPrecision(18, 6)
            .IsRequired();

        builder.Property(x => x.BankAccountId);
        builder.Property(x => x.Scale) .IsRequired();
        builder.Property(x => x.VAT)
            .HasPrecision(18, 6)
            .IsRequired();
        builder.Property(x => x.AutoSettleJournal)
            .IsRequired();
        builder.Property(x => x.Active)
            .IsRequired();

        builder.HasIndex(x => new { x.BillingServiceId, x.CurrencyId }).IsUnique();
        builder.HasIndex(x => x.Active);

        builder.HasOne(x => x.BillingService)
            .WithMany(x => x.Currencies)
            .HasForeignKey(x => x.BillingServiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Currency)
            .WithMany(x => x.BillingServiceCurrencies)
            .HasForeignKey(x => x.CurrencyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.ApplyType)
            .HasConversion<string>()
            .IsRequired();

    }
}
