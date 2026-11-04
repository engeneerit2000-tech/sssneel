using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SNEL.WalletPlatform.Domain.Common;
using SNEL.WalletPlatform.Domain.Common.Base;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Configurations.Extensions;

public abstract class LookupEntityConfiguration<TEntity, TKey> : IEntityTypeConfiguration<TEntity>
    where TEntity : LookupEntity<TKey>
    where TKey : notnull
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.Active)
            .IsRequired();

        builder.HasIndex(x => x.Name);
        ConfigureCodeIndex(builder);
    }

    public virtual void ConfigureCodeIndex(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasIndex(x => x.Code).IsUnique();
    }
}