using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SNEL.WalletPlatform.Domain.Partners;
using SNEL.WalletPlatform.Infrastructure.Persistence.Configurations.Extensions;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Configurations;

public sealed class ContactTypeTranslationEntityConfiguration
    : LookupTranslationEntityConfiguration<ContactTypeTranslationEntity, int>
{
    public override void Configure(EntityTypeBuilder<ContactTypeTranslationEntity> builder)
    {
        base.Configure(builder);

        builder.HasOne(x => x.ContactType)
            .WithMany(x => x.Translations)
            .HasForeignKey(x => x.EntityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
