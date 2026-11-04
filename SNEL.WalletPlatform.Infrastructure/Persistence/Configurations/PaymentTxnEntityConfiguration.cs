using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SNEL.WalletPlatform.Domain.Payments;
using SNEL.WalletPlatform.Infrastructure.Persistence.Configurations.Extensions;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Configurations;

public sealed class PaymentTxnEntityConfiguration : TrackedEntityConfiguration<PaymentTxnEntity,Guid>
{
    public override void Configure(EntityTypeBuilder<PaymentTxnEntity> builder)
    {
        base.Configure(builder);
        builder.Property(x => x.PaymentId)
            .IsRequired();

        builder.HasOne(x => x.Payment)
            .WithMany(p => p.PaymentTxns) 
            .HasForeignKey(x => x.PaymentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.CurrencyId)
            .IsRequired();

        builder.HasOne(x => x.Currency)
            .WithMany() 
            .HasForeignKey(x => x.CurrencyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.Type)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2)
            .IsRequired();
    }
}