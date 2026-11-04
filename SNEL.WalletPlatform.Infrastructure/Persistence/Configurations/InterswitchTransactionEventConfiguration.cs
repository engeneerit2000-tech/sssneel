using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SNEL.WalletPlatform.Domain.Webhooks;
using SNEL.WalletPlatform.Infrastructure.Persistence.Configurations.Extensions;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Configurations;

public sealed class InterswitchTransactionEventConfiguration : TrackedEntityConfiguration<InterswitchWebhookEventEntity,Guid>
{
    public override void Configure(EntityTypeBuilder<InterswitchWebhookEventEntity> builder)
    {
        base.Configure(builder);
        builder.Property(x => x.Event)
            .IsRequired();

        builder.Property(x => x.Uuid)
            .IsRequired();

        builder.Property(x => x.Timestamp)
            .IsRequired();

        builder.Property(x => x.RemittanceAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.BankCode)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.SplitAccounts)
            .HasColumnType("jsonb");

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .IsRequired();
    }
}