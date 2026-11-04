using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SNEL.WalletPlatform.Domain.Payments;
using SNEL.WalletPlatform.Infrastructure.Persistence.Configurations.Extensions;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Configurations;

public sealed class PaymentTypeTranslationEntityConfiguration
    : LookupTranslationEntityConfiguration<PaymentTypeTranslationEntity, int>
{
    public override void Configure(EntityTypeBuilder<PaymentTypeTranslationEntity> builder)
    {
        base.Configure(builder);

        builder.HasOne(x => x.PaymentType)
            .WithMany(x => x.Translations)
            .HasForeignKey(x => x.EntityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
