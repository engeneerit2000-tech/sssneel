using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SNEL.WalletPlatform.Domain.Billing;
using SNEL.WalletPlatform.Infrastructure.Persistence.Configurations.Extensions;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Configurations;

public sealed class BillingServiceTranslationEntityConfiguration
    : LookupTranslationEntityConfiguration<BillingServiceTranslationEntity, long>
{
    public override void Configure(EntityTypeBuilder<BillingServiceTranslationEntity> builder)
    {
        base.Configure(builder);

        builder.HasOne(x => x.BillingService)
            .WithMany(x => x.Translations)
            .HasForeignKey(x => x.EntityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
