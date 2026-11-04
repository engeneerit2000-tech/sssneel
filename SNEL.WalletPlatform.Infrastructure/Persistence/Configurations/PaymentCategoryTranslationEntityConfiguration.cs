using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SNEL.WalletPlatform.Domain.Payments;
using SNEL.WalletPlatform.Infrastructure.Persistence.Configurations.Extensions;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Configurations;

public sealed class PaymentCategoryTranslationEntityConfiguration
    : LookupTranslationEntityConfiguration<PaymentCategoryTranslationEntity, int>
{
    public override void Configure(EntityTypeBuilder<PaymentCategoryTranslationEntity> builder)
    {
        base.Configure(builder);

        builder.HasOne(x => x.PaymentCategory)
            .WithMany(x => x.Translations)
            .HasForeignKey(x => x.EntityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
