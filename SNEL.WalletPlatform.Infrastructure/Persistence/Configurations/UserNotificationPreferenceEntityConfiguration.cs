using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SNEL.WalletPlatform.Domain.Users;
using SNEL.WalletPlatform.Infrastructure.Persistence.Configurations.Extensions;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Configurations;

public sealed class UserNotificationPreferenceEntityConfiguration
    : TrackedEntityConfiguration<UserNotificationPreferenceEntity,Guid>
{
    public override  void Configure(EntityTypeBuilder<UserNotificationPreferenceEntity> builder)
    {
        base.Configure(builder);
        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.NotificationChannel)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Active)
            .IsRequired();

        builder.HasIndex(x => new { x.UserId, x.NotificationChannel })
            .IsUnique();
    }
}
