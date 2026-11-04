using Microsoft.EntityFrameworkCore;
using SNEL.WalletPlatform.Domain.Common;
using SNEL.WalletPlatform.Domain.Common.Base;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Seed;

public static class LookupSeedExtensions
{
    public static ModelBuilder SeedLookupWithTranslations<TEntity, TTranslationEntity, TKey>(
        this ModelBuilder modelBuilder,
        object[] lookups,
        object[] translations)
        where TEntity : LookupEntity<TKey>, ITranslatableEntity<TTranslationEntity, TKey>
        where TTranslationEntity : TranslationEntity<TKey>
        where TKey : notnull
    {
        modelBuilder.Entity<TEntity>().HasData(lookups);
        modelBuilder.Entity<TTranslationEntity>().HasData(translations);

        return modelBuilder;
    }
}
