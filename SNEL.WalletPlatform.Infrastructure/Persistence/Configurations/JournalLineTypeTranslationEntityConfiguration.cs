using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SNEL.WalletPlatform.Domain.Accounting;
using SNEL.WalletPlatform.Infrastructure.Persistence.Configurations.Extensions;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Configurations;

public sealed class JournalLineTypeTranslationEntityConfiguration : LookupTranslationEntityConfiguration<JournalLineTypeTranslationEntity,int>
{
    public override void Configure(EntityTypeBuilder<JournalLineTypeTranslationEntity> builder)
    {
        base.Configure(builder);
    
        builder.HasOne(x => x.JournalLineType)
            .WithMany(x => x.Translations)
            .HasForeignKey(x => x.EntityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

