using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SNEL.WalletPlatform.Domain.Auth;
using SNEL.WalletPlatform.Infrastructure.Persistence.Configurations.Extensions;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Configurations;

public sealed class OtpChallengeEntityConfiguration : TrackedEntityConfiguration<OtpChallengeEntity, Guid>
{
    public override void Configure(EntityTypeBuilder<OtpChallengeEntity> builder)
    {
        builder.ToTable("OtpChallenges");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.MobileNo)
            .IsRequired();

        builder.Property(x => x.OtpHash)
            .IsRequired();

        builder.Property(x => x.AttemptCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.Purpose)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.ExpiresAt)
            .IsRequired();

        builder.HasIndex(x => x.MobileNo);

        builder.HasIndex(x => new { x.Id, x.Purpose, x.Status })
            .IsUnique();
    }
}
