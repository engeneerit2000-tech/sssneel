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

public sealed class ConfirmationModeTranslationEntityConfiguration : LookupTranslationEntityConfiguration<ConfirmationModeTranslationEntity, int>
{
    public override void Configure(EntityTypeBuilder<ConfirmationModeTranslationEntity> builder)
    {
        base.Configure(builder);

        builder.HasOne(x => x.ConfirmationMode)
            .WithMany(x => x.Translations)
            .HasForeignKey(x => x.EntityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
