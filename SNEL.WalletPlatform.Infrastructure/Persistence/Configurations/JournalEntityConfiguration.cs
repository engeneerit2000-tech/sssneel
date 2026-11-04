using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SNEL.WalletPlatform.Domain.Accounting;
using SNEL.WalletPlatform.Infrastructure.Persistence.Configurations.Extensions;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Configurations;

public sealed class JournalEntityConfiguration : TrackedEntityConfiguration<JournalEntity,Guid>
{
    public override void Configure(EntityTypeBuilder<JournalEntity> builder)
    {
        base.Configure(builder);
        builder.Property(x => x.JournalTypeId).IsRequired();
        builder.HasOne(x => x.JournalType)
            .WithMany()
            .HasForeignKey(x => x.JournalTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.Description)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.Amount)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(x => x.SourceTypeId).HasMaxLength(50);
        builder.Property(x => x.SourceRef).HasMaxLength(256);

        builder.Property(x => x.JournalTimestamp)
            .IsRequired();

        builder.Property(x => x.SourceTimestamp);
        
        builder.HasMany(x => x.Lines)
            .WithOne(x => x.Journal)
            .HasForeignKey(x => x.JournalId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Property(x => x.Order).IsRequired();

    }
}

