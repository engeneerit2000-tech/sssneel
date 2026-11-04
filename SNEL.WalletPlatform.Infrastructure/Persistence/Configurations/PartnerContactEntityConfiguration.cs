using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SNEL.WalletPlatform.Domain.Partners;
using SNEL.WalletPlatform.Infrastructure.Persistence.Configurations.Extensions;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Configurations;

public sealed class PartnerContactEntityConfiguration : TrackedEntityConfiguration<PartnerContactEntity,long>
{
    public override void Configure(EntityTypeBuilder<PartnerContactEntity> builder)
    {
        base.Configure(builder);
        builder.Property(x => x.Id).UseIdentityByDefaultColumn().HasIdentityOptions(startValue: 1000);
        builder.Property(x => x.PartnerId)
            .IsRequired();

        builder.Property(x => x.ContactTypeId)
            .IsRequired();

        builder.Property(x => x.ContactInfo)
            .HasMaxLength(256)
            .IsRequired();

        builder.HasOne(x => x.ContactType)
            .WithMany(x => x.PartnerContacts)
            .HasForeignKey(x => x.ContactTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.PartnerId);
        builder.HasIndex(x => x.ContactTypeId);

    }
}
