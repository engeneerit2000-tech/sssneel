using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SNEL.WalletPlatform.Domain.Billing;
using SNEL.WalletPlatform.Infrastructure.Persistence.Configurations.Extensions;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Configurations;

public sealed class InvoiceStagingEntityConfiguration : TrackedEntityConfiguration<InvoiceStagingEntity,Guid>
{
    public override void Configure(EntityTypeBuilder<InvoiceStagingEntity> builder)
    {
        base.Configure(builder);
        builder.Property(x => x.JobKey).HasMaxLength(128);
        builder.Property(x => x.SourceSystem).HasMaxLength(50);
        builder.HasIndex(x => new { x.JobKey, x.SourceSystem });

        builder.Property(x => x.InvoiceNum).HasMaxLength(150);
        builder.Property(x => x.InvoiceDate);
        builder.Property(x => x.InvoiceUpdateDate);
        builder.Property(x => x.RegionCode).HasMaxLength(50);
        builder.Property(x => x.DistrictCode).HasMaxLength(50);
        builder.Property(x => x.CustomerId).HasPrecision(18, 0);
        builder.Property(x => x.CustomerNum).HasMaxLength(150);
        builder.Property(x => x.CustomerName).HasMaxLength(500);
        builder.Property(x => x.CustomerTypeCode).HasMaxLength(50);
        builder.Property(x => x.CustomerTypeName).HasMaxLength(200);
        builder.Property(x => x.TariffType).HasMaxLength(50);
        builder.Property(x => x.BillingPeriod).HasPrecision(18, 0);
        builder.Property(x => x.Consumption).HasPrecision(18, 4);
        builder.Property(x => x.BilledConsumption).HasPrecision(18, 4);
        builder.Property(x => x.InvoiceCurrencyCode).HasMaxLength(10);
        builder.Property(x => x.AmountUsd).HasPrecision(18, 2);
        builder.Property(x => x.AmountFc).HasPrecision(18, 2);
        builder.Property(x => x.PaidUsd).HasPrecision(18, 2);
        builder.Property(x => x.PaidFc).HasPrecision(18, 2);
        builder.Property(x => x.PaymentUpdateDate);
        builder.Property(x => x.Vat).HasPrecision(18, 2);
        builder.Property(x => x.ProvincialTax).HasPrecision(18, 2);
        
        builder.HasIndex(x => x.OriginInvoiceId);
        builder.Property(x => x.CenterDesignation).HasMaxLength(200);
        builder.Property(x => x.Pa).HasMaxLength(50);
        builder.Property(x => x.PointOfSaleName).HasMaxLength(200);
        builder.Property(x => x.SubstationCode).HasMaxLength(50);
        builder.Property(x => x.ClientPropertyNumber).HasMaxLength(100);
        builder.Property(x => x.ClientStreet).HasMaxLength(200);
        builder.Property(x => x.ClientCommune).HasMaxLength(100);
        builder.Property(x => x.MeterNumber).HasPrecision(18, 4);
        builder.Property(x => x.CalculationCoefficient).HasPrecision(18, 4);
        builder.Property(x => x.PreviousBillingIndex).HasPrecision(18, 4);
        builder.Property(x => x.CurrentBillingIndex).HasPrecision(18, 4);
        builder.Property(x => x.DiscountCode).HasPrecision(18, 4);
        builder.Property(x => x.AmountExcludingTaxUsd).HasPrecision(18, 2);
        builder.Property(x => x.AmountExcludingTaxFc).HasPrecision(18, 2);
        builder.Property(x => x.SubtotalUsd).HasPrecision(18, 2);
        builder.Property(x => x.SubtotalFc).HasPrecision(18, 2);
        builder.Property(x => x.BillingYear);
        builder.Property(x => x.BillingMonth);

        builder.HasIndex(x => x.InvoiceNum);
        builder.HasIndex(x => x.InvoiceUpdateDate);

    }
}
