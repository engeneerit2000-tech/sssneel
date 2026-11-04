using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SNEL.WalletPlatform.Domain.Billing;
using SNEL.WalletPlatform.Infrastructure.Persistence.Configurations.Extensions;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Configurations;

public sealed class BillerConfigEntityConfiguration : TrackedEntityConfiguration<BillerConfigEntity, long>
{
    public override void Configure(EntityTypeBuilder<BillerConfigEntity> builder)
    {
        base.Configure(builder);
        builder.ToTable("BillerConfigs");

        builder.Property(x => x.Id).UseIdentityByDefaultColumn().HasIdentityOptions(startValue: 1000);
        builder.Property(x => x.BillerCode)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.BillerName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Value)
            .HasMaxLength(500)
            .IsRequired();

        builder.HasIndex(x => new { x.BillerCode, x.Name })
            .IsUnique();
    }
}
