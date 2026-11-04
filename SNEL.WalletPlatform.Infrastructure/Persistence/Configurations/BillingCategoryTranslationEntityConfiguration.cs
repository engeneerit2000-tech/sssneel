using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SNEL.WalletPlatform.Domain.Billing;
using SNEL.WalletPlatform.Infrastructure.Persistence.Configurations.Extensions;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Configurations;

public sealed class BillingCategoryTranslationEntityConfiguration
    : LookupTranslationEntityConfiguration<BillingCategoryTranslationEntity, int>
{
    public override void Configure(EntityTypeBuilder<BillingCategoryTranslationEntity> builder)
    {
        base.Configure(builder);

        builder.HasOne(x => x.BillingCategory)
            .WithMany(x => x.Translations)
            .HasForeignKey(x => x.EntityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
