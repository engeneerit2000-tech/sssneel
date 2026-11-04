using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SNEL.WalletPlatform.Domain.Customers;
using SNEL.WalletPlatform.Infrastructure.Persistence.Configurations.Extensions;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Configurations;

public sealed class CustomerEntityConfiguration : TrackedEntityConfiguration<CustomerEntity,Guid>
{
    public override void Configure(EntityTypeBuilder<CustomerEntity> builder)
    {
        base.Configure(builder);
        builder.Property(x => x.PartnerId).IsRequired();

        builder.HasIndex(x => x.PartnerId)
            .IsUnique();

        builder.HasOne(x => x.Partner)
            .WithOne(x => x.Customer)
            .HasForeignKey<CustomerEntity>(x => x.PartnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.Address)
            .HasMaxLength(500);

        builder.Property(x => x.AreaId);

        builder.HasOne(x => x.Area)
            .WithMany(x => x.Customers)
            .HasForeignKey(x => x.AreaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.HasMany(x => x.RegistrationHistories)
            .WithOne(x => x.Customer)
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Subscriptions)
            .WithOne(x => x.Customer)
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
