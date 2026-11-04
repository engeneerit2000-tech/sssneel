using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SNEL.WalletPlatform.Domain.Billing;
using SNEL.WalletPlatform.Infrastructure.Persistence.Configurations.Extensions;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Configurations;

public sealed class NewInvoiceNotificationEntityConfiguration : TrackedEntityConfiguration<NewInvoiceNotificationEntity, Guid>
{
    public override void Configure(EntityTypeBuilder<NewInvoiceNotificationEntity> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.InvoiceId)
            .IsRequired();

        builder.Property(x => x.NotificationTriggerTime)
            .IsRequired();

        builder.Property(x => x.Processed)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.ProcessedAtUtc);

        builder.HasIndex(x => x.InvoiceId).IsUnique();
        builder.HasIndex(x => new { x.Processed, x.NotificationTriggerTime });

        builder.HasOne(x => x.Invoice)
            .WithMany()
            .HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
