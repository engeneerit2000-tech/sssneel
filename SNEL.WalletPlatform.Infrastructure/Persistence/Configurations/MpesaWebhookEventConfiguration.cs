using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SNEL.WalletPlatform.Domain.Webhooks;
using SNEL.WalletPlatform.Infrastructure.Persistence.Configurations.Extensions;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Configurations;

public sealed class MpesaWebhookEventConfiguration : TrackedEntityConfiguration<MpesaWebhookEventEntity, Guid>
{
    public override void Configure(EntityTypeBuilder<MpesaWebhookEventEntity> builder)
    {
        base.Configure(builder);
        builder.Property(x => x.OriginalConversationId).HasMaxLength(200);
        builder.Property(x => x.ThirdPartyConversationId).HasMaxLength(200).IsRequired();
        builder.HasIndex(x => x.ThirdPartyConversationId);
        builder.Property(x => x.TransactionId).HasMaxLength(200);
        builder.Property(x => x.ResultCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.ResultDescription).HasMaxLength(1000);
        builder.Property(x => x.Status).HasConversion<string>().IsRequired();
    }
}
