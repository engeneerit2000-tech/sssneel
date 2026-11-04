using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SNEL.WalletPlatform.Domain.Common;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Configurations.Extensions;

public abstract class TrackedEntityConfiguration<TEntity, TKey> : IEntityTypeConfiguration<TEntity>
    where TEntity : TrackedEntity<TKey>
    where TKey : notnull
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.UpdatedAtUtc)
            .IsRequired();

        builder.Property(x => x.CreatedBy)
            .HasMaxLength(128);

        builder.Property(x => x.UpdatedBy)
            .HasMaxLength(128);
    }
}