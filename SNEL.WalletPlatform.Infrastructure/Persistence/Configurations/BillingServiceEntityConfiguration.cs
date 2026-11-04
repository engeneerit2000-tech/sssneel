using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SNEL.WalletPlatform.Domain.Billing;
using SNEL.WalletPlatform.Infrastructure.Persistence.Configurations.Extensions;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Configurations;

public sealed class BillingServiceEntityConfiguration : TrackedEntityConfiguration<BillingServiceEntity, long>
{
    public override void Configure(EntityTypeBuilder<BillingServiceEntity> builder)
    {
        base.Configure(builder);
        builder.Property(x => x.Id).UseIdentityByDefaultColumn().HasIdentityOptions(startValue: 1000);
        builder.Property(x => x.BillingCategoryId)
            .IsRequired();

        builder.Property(x => x.BillerId)
            .IsRequired();

        builder.Property(x => x.Active)
            .IsRequired();

        builder.Property(x => x.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.ServiceTypeId)
            .IsRequired();

        builder.Property(x => x.SupportPartial)
            .IsRequired();

        builder.Property(x => x.SupportInquiry)
            .IsRequired();
        builder.Property(x => x.SupportCustomActions)
            .IsRequired();
        builder.Property(x => x.IsTransactional)
            .IsRequired();
        builder.Property(x => x.BillingRefFormat)
            .HasMaxLength(200);

        builder.HasIndex(x => new { x.BillerId, x.Code }).IsUnique();
        builder.HasIndex(x => x.BillingCategoryId);
        builder.HasIndex(x => x.ServiceTypeId);
        builder.HasIndex(x => x.Active);

        builder.HasOne(x => x.BillingCategory)
            .WithMany(x => x.BillingServices)
            .HasForeignKey(x => x.BillingCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Biller)
            .WithMany(x => x.BillingServices)
            .HasForeignKey(x => x.BillerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ServiceType)
            .WithMany(x => x.BillingServices)
            .HasForeignKey(x => x.ServiceTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Invoices)
            .WithOne(x => x.BillingService)
            .HasForeignKey(x => x.BillingServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Currencies)
            .WithOne(x => x.BillingService)
            .HasForeignKey(x => x.BillingServiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.CustomerSubscriptions)
            .WithOne(x => x.BillingService)
            .HasForeignKey(x => x.BillingServiceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}