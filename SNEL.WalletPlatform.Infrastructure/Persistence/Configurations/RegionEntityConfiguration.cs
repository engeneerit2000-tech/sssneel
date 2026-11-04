using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SNEL.WalletPlatform.Domain.Lookups;
using SNEL.WalletPlatform.Infrastructure.Persistence.Configurations.Extensions;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Configurations;

public sealed class RegionEntityConfiguration : LookupEntityConfiguration<RegionEntity,int>
{
    public override void Configure(EntityTypeBuilder<RegionEntity> builder)
    {
        base.Configure(builder);
        builder.Property(x => x.CountryId)
            .IsRequired();

        builder.HasOne(x => x.Country)
            .WithMany(x => x.Regions)
            .HasForeignKey(x => x.CountryId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public override void ConfigureCodeIndex(EntityTypeBuilder<RegionEntity> builder)
    {
        builder.HasIndex(x => new { x.CountryId, x.Code }).IsUnique();
    }
}
