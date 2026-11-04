using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SNEL.WalletPlatform.Domain.Payments;
using SNEL.WalletPlatform.Infrastructure.Persistence.Configurations.Extensions;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Configurations;

public sealed class PaymentEntityConfiguration : TrackedEntityConfiguration<PaymentEntity,Guid>
{
    public override void Configure(EntityTypeBuilder<PaymentEntity> builder)
    {
        base.Configure(builder);
        builder.Property(x => x.PaymentOrderId)
            .IsRequired();

        // TODO: confirm DeleteBehavior
        builder.HasOne(x => x.PaymentOrder)
            .WithMany(x => x.Payments)
            .HasForeignKey(x => x.PaymentOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.PaymentMethodId)
            .IsRequired();

        // TODO: confirm DeleteBehavior
        builder.HasOne(x => x.PaymentMethod)
            .WithMany(x => x.Payments)
            .HasForeignKey(x => x.PaymentMethodId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.CurrencyId)
            .IsRequired();

        // TODO: confirm DeleteBehavior
        builder.HasOne(x => x.Currency)
            .WithMany(x=>x.Payments)
            .HasForeignKey(x => x.CurrencyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.Tax)
            .HasPrecision(18, 2);

        builder.Property(x => x.Total)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.CustomerId).IsRequired(false);

        // TODO: confirm DeleteBehavior
        builder.HasOne(x => x.Customer)
            .WithMany(x=>x.Payments)
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.ChannelId)
            .IsRequired(false);

        // TODO: confirm DeleteBehavior
        builder.HasOne(x => x.Channel)
            .WithMany(x=>x.Payments)
            .HasForeignKey(x => x.ChannelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.Direction)
            .HasConversion<string>()
            .IsRequired(false);
        builder.Property(x => x.PhoneNumber)
            .HasConversion<string>()
            .IsRequired(false);
        builder.Property(x => x.StatusChanged)
            .IsRequired(false);

    }
}
