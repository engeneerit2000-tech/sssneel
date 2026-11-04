using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SNEL.WalletPlatform.Domain.Lookups;
using SNEL.WalletPlatform.Infrastructure.Persistence.Configurations.Extensions;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Configurations;

public sealed class GenderTranslationEntityConfiguration : LookupTranslationEntityConfiguration<GenderTranslationEntity, int>
{
    public override void Configure(EntityTypeBuilder<GenderTranslationEntity> builder)
    {
        base.Configure(builder);

        builder.HasOne(x => x.Gender)
            .WithMany(x => x.Translations)
            .HasForeignKey(x => x.EntityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
