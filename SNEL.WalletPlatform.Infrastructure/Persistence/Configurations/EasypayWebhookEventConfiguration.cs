using SNEL.WalletPlatform.Domain.Webhooks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SNEL.WalletPlatform.Infrastructure.Persistence.Configurations.Extensions;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Configurations;

public sealed class EasypayWebhookEventConfiguration : TrackedEntityConfiguration<EasypayWebhookEventEntity, Guid>
{
    public override void Configure(EntityTypeBuilder<EasypayWebhookEventEntity> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.ReferenceId)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(x => x.ReferenceId);

        builder.Property(x => x.TransactionId)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Currency)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.Provider)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.Phone)
            .HasMaxLength(50);

        builder.Property(x => x.TelecomId)
            .HasMaxLength(200);

        builder.Property(x => x.Type)
            .HasMaxLength(50);

        builder.Property(x => x.Reason)
            .HasMaxLength(500);
    }
}

