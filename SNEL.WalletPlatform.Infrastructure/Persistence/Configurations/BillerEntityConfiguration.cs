using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SNEL.WalletPlatform.Domain.Billing;
using SNEL.WalletPlatform.Infrastructure.Persistence.Configurations.Extensions;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Configurations;

public sealed class BillerEntityConfiguration : TrackedEntityConfiguration<BillerEntity,long>
{
    public override void Configure(EntityTypeBuilder<BillerEntity> builder)
    {
        base.Configure(builder);
        builder.Property(x => x.Id).UseIdentityByDefaultColumn().HasIdentityOptions(startValue: 1000);
        builder.Property(x => x.PartnerId)
            .IsRequired();

        builder.Property(x => x.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.PartnerId).IsUnique();
       

        builder.HasOne(x => x.Partner)
            .WithMany(x => x.Billers)
            .HasForeignKey(x => x.PartnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.BillingServices)
            .WithOne(x => x.Biller)
            .HasForeignKey(x => x.BillerId)
            .OnDelete(DeleteBehavior.Cascade);

    }
}
