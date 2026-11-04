using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SNEL.WalletPlatform.Domain.Notifications;
using SNEL.WalletPlatform.Infrastructure.Persistence.Configurations.Extensions;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Configurations;

public sealed class CampaignTranslationEntityConfiguration
    : LookupTranslationEntityConfiguration<CampaignTranslationEntity, Guid>
{
    public override void Configure(EntityTypeBuilder<CampaignTranslationEntity> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Message)
            .IsRequired()
            .HasMaxLength(4000);

        builder.HasOne(x => x.Campaign)
            .WithMany(x => x.Translations)
            .HasForeignKey(x => x.EntityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
