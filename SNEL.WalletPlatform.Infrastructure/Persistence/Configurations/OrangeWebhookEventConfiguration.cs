using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SNEL.WalletPlatform.Domain.Webhooks;
using SNEL.WalletPlatform.Infrastructure.Persistence.Configurations.Extensions;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Configurations;

public sealed class OrangeWebhookEventConfiguration : TrackedEntityConfiguration<OrangeWebhookEventEntity, Guid>
{
    public override void Configure(EntityTypeBuilder<OrangeWebhookEventEntity> builder)
    {
        base.Configure(builder);
        builder.Property(x => x.StatusCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Message).HasMaxLength(1000);
        builder.Property(x => x.TransactionId).HasMaxLength(200).IsRequired();
        builder.HasIndex(x => x.TransactionId);
        builder.Property(x => x.OrangeTransactionId).HasMaxLength(200);
        builder.Property(x => x.Type).HasMaxLength(50);
        builder.Property(x => x.PeerId).HasMaxLength(100);
        builder.Property(x => x.PeerIdType).HasMaxLength(50);
        builder.Property(x => x.Amount).HasPrecision(18, 2);
        builder.Property(x => x.Currency).HasMaxLength(10).IsRequired();
        builder.Property(x => x.PosId).HasMaxLength(100);
        builder.Property(x => x.Country).HasMaxLength(10);
        builder.Property(x => x.Status).HasConversion<string>().IsRequired();
    }
}
