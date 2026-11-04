using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SNEL.WalletPlatform.Domain.Lookups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SNEL.WalletPlatform.Infrastructure.Persistence.Configurations.Extensions;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Configurations;

public sealed class PaymentMethodPurposeTranslationEntityConfiguration : LookupTranslationEntityConfiguration<PaymentMethodPurposeTranslationEntity, int>
{
    public override void Configure(EntityTypeBuilder<PaymentMethodPurposeTranslationEntity> builder)
    {
        base.Configure(builder);

        builder.HasOne(x => x.PaymentMethodPurpose)
            .WithMany(x => x.Translations)
            .HasForeignKey(x => x.EntityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
