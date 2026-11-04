using SNEL.WalletPlatform.Domain.Webhooks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SNEL.WalletPlatform.Infrastructure.Persistence.Configurations.Extensions;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Configurations;

public sealed class AkibaPayWebhookEventConfiguration : TrackedEntityConfiguration<AkibaPayWebhookEventEntity, Guid>
{
    public override void Configure(EntityTypeBuilder<AkibaPayWebhookEventEntity> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.AkibaReference)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(x => x.AkibaReference);

        builder.Property(x => x.MerchantReference)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.TransactionStatus)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Currency)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .IsRequired();
    }
}

