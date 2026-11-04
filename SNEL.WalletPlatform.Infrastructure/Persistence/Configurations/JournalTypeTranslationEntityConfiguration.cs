using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SNEL.WalletPlatform.Domain.Accounting;
using SNEL.WalletPlatform.Infrastructure.Persistence.Configurations.Extensions;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Configurations;

public sealed class JournalTypeTranslationEntityConfiguration : LookupTranslationEntityConfiguration<JournalTypeTranslationEntity,int>
{
    public override void Configure(EntityTypeBuilder<JournalTypeTranslationEntity> builder)
    {
        base.Configure(builder);
    
        builder.HasOne(x => x.JournalType)
            .WithMany(x => x.Translations)
            .HasForeignKey(x => x.EntityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

