using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SNEL.WalletPlatform.Domain.Notifications;
using SNEL.WalletPlatform.Infrastructure.Persistence.Configurations.Extensions;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Configurations;

public sealed class CampaignEntityConfiguration : TrackedEntityConfiguration<CampaignEntity, Guid>
{
    public override void Configure(EntityTypeBuilder<CampaignEntity> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(x => x.AudienceGroupCode)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.StartAtUtc)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasDefaultValue(CampaignStatus.Scheduled);

        builder.Property(x => x.LastProcessedCustomerId);

        builder.Property(x => x.NotificationsStartedAtUtc);

        builder.Property(x => x.NotificationsCompletedAtUtc);

        builder.HasIndex(x => x.StartAtUtc);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.AudienceGroupCode);
    }
}
