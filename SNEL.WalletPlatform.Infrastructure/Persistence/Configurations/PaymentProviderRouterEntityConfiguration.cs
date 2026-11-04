using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SNEL.WalletPlatform.Domain.Payments;
using SNEL.WalletPlatform.Infrastructure.Persistence.Configurations.Extensions;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Configurations;

public sealed class PaymentProviderRouterEntityConfiguration : TrackedEntityConfiguration<PaymentProviderRouterEntity, int>
{
    public override void Configure(EntityTypeBuilder<PaymentProviderRouterEntity> builder)
    {
        base.Configure(builder);
        builder.Property(x => x.Id).UseIdentityByDefaultColumn().HasIdentityOptions(startValue: 1000);
        builder.Property(x => x.PaymentProviderCode)
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(x => x.PaymentMethodCode)
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(x => x.CountryCode)
            .HasMaxLength(3)
            .IsRequired();
        builder.Property(x => x.CompanyPrefix)
            .HasMaxLength(5);
        
    }
}
