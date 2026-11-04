using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SNEL.WalletPlatform.Domain.Common;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Configurations.Extensions;

public abstract class LookupTranslationEntityConfiguration<TEntity, TKey> : IEntityTypeConfiguration<TEntity>
    where TEntity : TranslationEntity<TKey>
    where TKey : notnull
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.EntityId).IsRequired();
        builder.Property(x => x.LanguageCode).HasMaxLength(50).IsRequired();

        builder.Property(x => x.Name).HasMaxLength(255).IsRequired();


        builder.HasIndex(x => x.Name);
        builder.HasIndex(x => x.LanguageCode);
        builder.HasIndex(x => new { x.EntityId, x.LanguageCode }).IsUnique();
    }
}