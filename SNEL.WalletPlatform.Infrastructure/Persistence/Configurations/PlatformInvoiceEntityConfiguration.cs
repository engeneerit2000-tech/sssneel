using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SNEL.WalletPlatform.Domain.Accounting;
using SNEL.WalletPlatform.Infrastructure.Persistence.Configurations.Extensions;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Configurations;

public sealed class PlatformInvoiceEntityConfiguration : TrackedEntityConfiguration<PlatformInvoiceEntity, long>
{
    public override void Configure(EntityTypeBuilder<PlatformInvoiceEntity> builder)
    {
        base.Configure(builder);
        builder.Property(x => x.Id)
            .UseIdentityByDefaultColumn();

        builder.Property(x => x.PartnerId).IsRequired();
        builder.HasOne(x => x.Partner)
            .WithMany()
            .HasForeignKey(x => x.PartnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.CurrencyId).IsRequired();
        builder.HasOne(x => x.Currency)
            .WithMany()
            .HasForeignKey(x => x.CurrencyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.DescriptionTemplateKey)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.DescriptionParametersJson)
            .HasMaxLength(2000);

        builder.Property(x => x.InvoiceDate).IsRequired();

        builder.Property(x => x.Amount).HasPrecision(18, 6).IsRequired();
        builder.Property(x => x.Tax).HasPrecision(18, 6).IsRequired();
        builder.Property(x => x.Total).HasPrecision(18, 6).IsRequired();

        builder.ToTable("PlatformInvoices");
    }
}
