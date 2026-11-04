using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SNEL.WalletPlatform.Domain.Payments;
using SNEL.WalletPlatform.Infrastructure.Persistence.Configurations.Extensions;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Configurations;

public sealed class PaymentOrderEntityConfiguration : TrackedEntityConfiguration<PaymentOrderEntity, Guid>
{
    public override void Configure(EntityTypeBuilder<PaymentOrderEntity> builder)
    {
        base.Configure(builder);
        builder.Property(x => x.CustomerId);

        // TODO: confirm DeleteBehavior
        builder.HasOne(x => x.Customer)
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.ChannelId)
            .IsRequired();

        // TODO: confirm DeleteBehavior
        builder.HasOne(x => x.Channel)
            .WithMany()
            .HasForeignKey(x => x.ChannelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.CurrencyId)
            .IsRequired();

        builder.HasOne(x => x.Currency)
            .WithMany(x=>x.PaymentOrders)
            .HasForeignKey(x => x.CurrencyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.Amount)
            .HasPrecision(18, 6)
            .IsRequired();

        builder.Property(x => x.TotalFees).HasPrecision(18, 6).IsRequired();
        builder.Property(x => x.TotalFeesTax).HasPrecision(18, 6).IsRequired();
        builder.Property(x => x.TotalCost).HasPrecision(18, 6).IsRequired();
        builder.Property(x => x.TotalCostTax).HasPrecision(18, 6).IsRequired();
        builder.Property(x => x.TotalCommission).HasPrecision(18, 6).IsRequired();
        builder.Property(x => x.TotalCommissionTax).HasPrecision(18, 6).IsRequired();
        builder.Property(x => x.TotalAmount).HasPrecision(18, 6).IsRequired();

        builder.HasMany(x => x.Items)
            .WithOne(x => x.PaymentOrder)
            .HasForeignKey(x => x.PaymentOrderId)
            .OnDelete(DeleteBehavior.Cascade); 
        builder.HasMany(x => x.Payments)
            .WithOne(x => x.PaymentOrder)
            .HasForeignKey(x => x.PaymentOrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}