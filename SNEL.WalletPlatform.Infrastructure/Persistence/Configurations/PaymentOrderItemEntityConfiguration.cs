using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SNEL.WalletPlatform.Domain.Payments;
using SNEL.WalletPlatform.Infrastructure.Persistence.Configurations.Extensions;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Configurations;

public sealed class PaymentOrderItemEntityConfiguration : TrackedEntityConfiguration<PaymentOrderItemEntity,Guid>
{
    public override void Configure(EntityTypeBuilder<PaymentOrderItemEntity> builder)
    {
        base.Configure(builder);
        builder.Property(x => x.PaymentOrderId)
            .IsRequired(); 

        builder.Property(x => x.BillingServiceId)
            .IsRequired(false);

        builder.HasOne(x => x.BillingService)
            .WithMany()
            .HasForeignKey(x => x.BillingServiceId)
            .OnDelete(DeleteBehavior.Restrict); 

        builder.Property(x => x.CustomerSubscriptionId)
            .IsRequired(false);

        builder.HasOne(x => x.CustomerSubscription)
            .WithMany()
            .HasForeignKey(x => x.CustomerSubscriptionId)
            .OnDelete(DeleteBehavior.Restrict); 

        builder.Property(x => x.InvoiceId)
            .IsRequired(false);

        builder.HasOne(x => x.Invoice)
            .WithMany(x => x.PaymentOrderItems)
            .HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);

        // TODO: confirm DeleteBehavior
        builder.HasOne(x => x.PaymentOrder)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.PaymentOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.ChannelId);

        // TODO: confirm DeleteBehavior
        builder.HasOne(x => x.Channel)
            .WithMany()
            .HasForeignKey(x => x.ChannelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.ItemType)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.Amount)
            .HasPrecision(18, 6)
            .IsRequired();

        builder.Property(x => x.PaymentProviderFees).HasPrecision(18, 6).IsRequired();
        builder.Property(x => x.PaymentProviderFeesTax).HasPrecision(18, 6).IsRequired();
        builder.Property(x => x.BillerFees).HasPrecision(18, 6).IsRequired();
        builder.Property(x => x.BillerFeesTax).HasPrecision(18, 6).IsRequired();
        builder.Property(x => x.PaymentProviderCost).HasPrecision(18, 6).IsRequired();
        builder.Property(x => x.PaymentProviderCostTax).HasPrecision(18, 6).IsRequired();
        builder.Property(x => x.BillerCost).HasPrecision(18, 6).IsRequired();
        builder.Property(x => x.BillerCostTax).HasPrecision(18, 6).IsRequired();
        builder.Property(x => x.PaymentProviderCommission).HasPrecision(18, 6).IsRequired();
        builder.Property(x => x.PaymentProviderCommissionTax).HasPrecision(18, 6).IsRequired();
        builder.Property(x => x.BillerCommission).HasPrecision(18, 6).IsRequired();
        builder.Property(x => x.BillerCommissionTax).HasPrecision(18, 6).IsRequired();
        builder.Property(x => x.TotalFees).HasPrecision(18, 6).IsRequired();
        builder.Property(x => x.TotalFeesTax).HasPrecision(18, 6).IsRequired();
        builder.Property(x => x.TotalCost).HasPrecision(18, 6).IsRequired();
        builder.Property(x => x.TotalCostTax).HasPrecision(18, 6).IsRequired();
        builder.Property(x => x.TotalCommission).HasPrecision(18, 6).IsRequired();
        builder.Property(x => x.TotalCommissionTax).HasPrecision(18, 6).IsRequired();
        builder.Property(x => x.TotalAmount).HasPrecision(18, 6).IsRequired();
        
        builder.Property(x => x.CurrencyId)
            .IsRequired(false);

        builder.HasOne(x => x.Currency)
            .WithMany(x=>x.PaymentOrderItems)
            .HasForeignKey(x => x.CurrencyId)
            .OnDelete(DeleteBehavior.Restrict);


        builder.Property(x => x.ExternalRef)
            .HasMaxLength(255)
            .IsRequired(false);

        builder.Property(x => x.WebResponse)
            .HasColumnType("text")
            .IsRequired(false);

        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => new { x.InvoiceId, x.ItemType, x.Status });
    }
}
