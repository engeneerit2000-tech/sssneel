using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SNEL.WalletPlatform.Domain.Accounting;
using SNEL.WalletPlatform.Infrastructure.Persistence.Configurations.Extensions;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Configurations;

public sealed class JournalLineEntityConfiguration : TrackedEntityConfiguration<JournalLineEntity,Guid>
{
    public override void Configure(EntityTypeBuilder<JournalLineEntity> builder)
    {
        base.Configure(builder);
        builder.Property(x => x.JournalId).IsRequired();
        builder.HasOne(x => x.Journal)
            .WithMany(x=>x.Lines)
            .HasForeignKey(x => x.JournalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.JournalLineTypeId).IsRequired();
        builder.HasOne(x => x.JournalLineType)
            .WithMany()
            .HasForeignKey(x => x.JournalLineTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.AccountId).IsRequired();
        builder.HasOne(x => x.Account)
            .WithMany()
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.Debit)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(x => x.Credit)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(x => x.Balance)
            .HasPrecision(18, 4)
            .IsRequired(); 
        builder.Property(x => x.Order).IsRequired();

        builder.HasOne(x => x.PlatformInvoice)
            .WithMany()
            .HasForeignKey(x => x.PlatformInvoiceId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

