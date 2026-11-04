using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SNEL.WalletPlatform.Domain.Lookups;
using SNEL.WalletPlatform.Infrastructure.Persistence.Configurations.Extensions;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Configurations;

public sealed class AreaEntityConfiguration : LookupEntityConfiguration<AreaEntity,int>
{
    public override void Configure(EntityTypeBuilder<AreaEntity> builder)
    {
        base.Configure(builder);
        builder.Property(x => x.CityId)
            .IsRequired();

        builder.HasOne(x => x.City)
            .WithMany(x => x.Areas)
            .HasForeignKey(x => x.CityId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasIndex(x => x.Name);
    }
    public override void ConfigureCodeIndex(EntityTypeBuilder<AreaEntity> builder)
    {
        builder.HasIndex(x => new { x.CityId, x.Code }).IsUnique();
    }
}
