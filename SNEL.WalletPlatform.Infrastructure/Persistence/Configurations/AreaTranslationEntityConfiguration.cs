using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SNEL.WalletPlatform.Domain.Lookups;
using SNEL.WalletPlatform.Infrastructure.Persistence.Configurations.Extensions;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Configurations;

public sealed class AreaTranslationEntityConfiguration : LookupTranslationEntityConfiguration<AreaTranslationEntity, int>
{
    public override void Configure(EntityTypeBuilder<AreaTranslationEntity> builder)
    {
        base.Configure(builder);

        builder.HasOne(x => x.Area)
            .WithMany(x => x.Translations)
            .HasForeignKey(x => x.EntityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
