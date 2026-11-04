using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SNEL.WalletPlatform.Domain.Billing;
using SNEL.WalletPlatform.Infrastructure.Persistence.Configurations.Extensions;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Configurations;

public sealed class InvoiceEntityConfiguration : TrackedEntityConfiguration<InvoiceEntity,Guid>
{
    public override void Configure(EntityTypeBuilder<InvoiceEntity> builder)
    {
        base.Configure(builder);
        builder.Property(x => x.BillingServiceId).IsRequired();

        builder.Property(x => x.BillingRef)
            .HasMaxLength(150)
            .IsRequired();
        builder.Property(x => x.OriginInvoiceId);
        builder.Property(x => x.InvoiceRef)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.AmountOutstanding)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.AmountPaid)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.AmountTotal)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.CurrencyId).IsRequired();
        builder.Property(x => x.IssueDate).IsRequired();
        builder.Property(x => x.DueDate);

        builder.Property(x => x.Status)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.InvoiceInfo)
            .HasMaxLength(4000);

        builder.Property(x => x.OriginCustomerId)
            .HasMaxLength(50);

        builder.Property(x => x.PaymentMismatchFlag)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(x => x.InvoiceRef).IsUnique();
        builder.HasIndex(x => x.BillingRef);
        builder.HasIndex(x => x.BillingServiceId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.AmountOutstanding);
        builder.HasIndex(x => new { x.BillingServiceId, x.BillingRef });

        builder.HasOne(x => x.BillingService)
            .WithMany(x => x.Invoices)
            .HasForeignKey(x => x.BillingServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Currency)
            .WithMany(x => x.Invoices)
            .HasForeignKey(x => x.CurrencyId)
            .OnDelete(DeleteBehavior.Restrict);

    }
}
