using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SNEL.WalletPlatform.Domain.Lookups;
using SNEL.WalletPlatform.Infrastructure.Persistence.Configurations.Extensions;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Configurations;

public sealed class CurrencyEntityConfiguration : LookupEntityConfiguration<CurrencyEntity,int>
{
    public override void Configure(EntityTypeBuilder<CurrencyEntity> builder)
    {
        base.Configure(builder);
        builder.Property(x => x.Abbreviation).IsRequired();
        builder.Property(x => x.Minor).IsRequired(); 
    }
}
