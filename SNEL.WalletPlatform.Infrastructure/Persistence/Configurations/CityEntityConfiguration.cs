using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SNEL.WalletPlatform.Domain.Lookups;
using SNEL.WalletPlatform.Infrastructure.Persistence.Configurations.Extensions;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Configurations;

public sealed class CityEntityConfiguration : LookupEntityConfiguration<CityEntity,int>
{
    public override void Configure(EntityTypeBuilder<CityEntity> builder)
    {
        base.Configure(builder);
        builder.Property(x => x.RegionId)
            .IsRequired();
        builder.HasOne(x => x.Region)
            .WithMany(x => x.Cities)
            .HasForeignKey(x => x.RegionId)
            .OnDelete(DeleteBehavior.Restrict);

        
    }
    public override void ConfigureCodeIndex(EntityTypeBuilder<CityEntity> builder)
    {
        builder.HasIndex(x => new { x.RegionId, x.Code }).IsUnique();
    }
}
