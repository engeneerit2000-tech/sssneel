using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SNEL.WalletPlatform.Domain.Lookups;
using SNEL.WalletPlatform.Infrastructure.Persistence.Configurations.Extensions;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Configurations;

public sealed class CurrencyTranslationEntityConfiguration : LookupTranslationEntityConfiguration<CurrencyTranslationEntity, int>
{
    public override void Configure(EntityTypeBuilder<CurrencyTranslationEntity> builder)
    {
        base.Configure(builder);

        builder.HasOne(x => x.Currency)
            .WithMany(x => x.Translations)
            .HasForeignKey(x => x.EntityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
