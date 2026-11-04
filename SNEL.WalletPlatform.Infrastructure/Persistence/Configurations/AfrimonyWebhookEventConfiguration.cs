using SNEL.WalletPlatform.Domain.Webhooks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SNEL.WalletPlatform.Infrastructure.Persistence.Configurations.Extensions;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Configurations;

public sealed class AfrimonyWebhookEventConfiguration : TrackedEntityConfiguration<AfrimonyWebhookEventEntity, Guid>
{
    public override void Configure(EntityTypeBuilder<AfrimonyWebhookEventEntity> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.MmTransactionId)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(x => x.MmTransactionId);

        builder.Property(x => x.ExternalId)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.TxnStatus)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.SenderMsisdn)
            .HasMaxLength(50);

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .IsRequired();
    }
}
