using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SNEL.WalletPlatform.Domain.Customers;
using SNEL.WalletPlatform.Infrastructure.Persistence.Configurations.Extensions;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Configurations;

public sealed class CustomerSubscriptionEntityConfiguration : TrackedEntityConfiguration<CustomerSubscriptionEntity,Guid>
{
    public override void Configure(EntityTypeBuilder<CustomerSubscriptionEntity> builder)
    {
        base.Configure(builder);
        builder.Property(x => x.CustomerId)
            .IsRequired();

        builder.Property(x => x.BillingServiceId)
            .IsRequired();

        builder.Property(x => x.CustomerRef)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.BillingRefValue)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(x => x.CustomerId);
        builder.HasIndex(x => x.BillingServiceId);
        builder.HasIndex(x => new { x.CustomerId, x.BillingServiceId, x.BillingRefValue })
            .IsUnique();

        builder.HasOne(x => x.Customer)
            .WithMany(x => x.Subscriptions)
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.BillingService)
            .WithMany(x => x.CustomerSubscriptions)
            .HasForeignKey(x => x.BillingServiceId)
            .OnDelete(DeleteBehavior.Restrict);

    }
}
