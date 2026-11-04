using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SNEL.WalletPlatform.Domain.Lookups;
using SNEL.WalletPlatform.Infrastructure.Persistence.Configurations.Extensions;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Configurations;

public sealed class PlatformConstantEntityConfiguration : LookupEntityConfiguration<PlatformConstantEntity, int>
{
    public override void Configure(EntityTypeBuilder<PlatformConstantEntity> builder)
    {
        base.Configure(builder);
        builder.Property(x => x.Group).IsRequired();
        builder.Property(x => x.Value).IsRequired();
    }
    public override void ConfigureCodeIndex(EntityTypeBuilder<PlatformConstantEntity> builder)
    {
        builder.HasIndex(x => new { x.Group, x.Code }).IsUnique();
    }
}
