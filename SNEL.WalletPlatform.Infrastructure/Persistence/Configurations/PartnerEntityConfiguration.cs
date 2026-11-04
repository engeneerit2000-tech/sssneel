using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SNEL.WalletPlatform.Domain.Partners;
using SNEL.WalletPlatform.Infrastructure.Persistence.Configurations.Extensions;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Configurations;

public sealed class PartnerEntityConfiguration : TrackedEntityConfiguration<PartnerEntity,long>
{
    public override void Configure(EntityTypeBuilder<PartnerEntity> builder)
    {
        base.Configure(builder);
        builder.Property(x => x.Id).UseIdentityByDefaultColumn().HasIdentityOptions(startValue: 1000);
        builder.Property(x => x.PartnerTypeId)
            .IsRequired();

        builder.Property(x => x.LegalName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.DisplayName)
            .HasMaxLength(200);

        builder.Property(x => x.Active)
            .IsRequired();

        builder.HasIndex(x => x.PartnerTypeId);
        builder.HasIndex(x => x.Active);

        builder.HasOne(x => x.PartnerType)
            .WithMany(x => x.Partners)
            .HasForeignKey(x => x.PartnerTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Contacts)
            .WithOne(x => x.Partner)
            .HasForeignKey(x => x.PartnerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
