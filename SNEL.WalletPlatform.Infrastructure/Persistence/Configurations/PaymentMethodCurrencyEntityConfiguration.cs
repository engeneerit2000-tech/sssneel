using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SNEL.WalletPlatform.Domain.Payments;
using SNEL.WalletPlatform.Infrastructure.Persistence.Configurations.Extensions;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Configurations;

public sealed class PaymentMethodCurrencyEntityConfiguration : TrackedEntityConfiguration<PaymentMethodCurrencyEntity,long>
{
    public override void Configure(EntityTypeBuilder<PaymentMethodCurrencyEntity> builder)
    {
        base.Configure(builder);
        builder.Property(x => x.Id).UseIdentityByDefaultColumn().HasIdentityOptions(startValue: 1000);
        builder.Property(x => x.CurrencyId)
            .IsRequired();

        // TODO: confirm DeleteBehavior
        builder.HasOne(x => x.Currency)
            .WithMany(x=>x.PaymentMethodCurrencies)
            .HasForeignKey(x => x.CurrencyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.PaymentMethodId)
            .IsRequired();

        // TODO: confirm DeleteBehavior
        builder.HasOne(x => x.PaymentMethod)
            .WithMany(x => x.PaymentMethodCurrencies)
            .HasForeignKey(x => x.PaymentMethodId)
            .OnDelete(DeleteBehavior.Restrict);


        // TODO: confirm
        builder.HasIndex(x => new { x.CurrencyId, x.PaymentMethodId })
            .IsUnique();

        builder.Property(x => x.Min)
            .HasPrecision(18, 6)
            .IsRequired();

        builder.Property(x => x.Max)
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
        builder.Property(x => x.MinimumAgeMinutes);

    }
}
