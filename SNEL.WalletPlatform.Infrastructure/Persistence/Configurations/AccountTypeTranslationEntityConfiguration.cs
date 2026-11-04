using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SNEL.WalletPlatform.Domain.Accounting;
using SNEL.WalletPlatform.Infrastructure.Persistence.Configurations.Extensions;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Configurations;

public sealed class AccountTypeTranslationEntityConfiguration : LookupTranslationEntityConfiguration<AccountTypeTranslationEntity,int>
{
    public override void Configure(EntityTypeBuilder<AccountTypeTranslationEntity> builder)
    {
        base.Configure(builder);
    
        builder.HasOne(x => x.AccountType)
            .WithMany(x => x.Translations)
            .HasForeignKey(x => x.EntityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

