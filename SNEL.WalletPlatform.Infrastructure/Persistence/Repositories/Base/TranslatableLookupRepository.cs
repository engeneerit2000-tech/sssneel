using Microsoft.EntityFrameworkCore;
using SNEL.WalletPlatform.Application.Interfaces.Repositories.Base;
using SNEL.WalletPlatform.Domain.Common;
using SNEL.WalletPlatform.Domain.Common.Base;
using SNEL.WalletPlatform.Infrastructure.Persistence;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Repositories.Base;

public abstract class TranslatableLookupRepository<TEntity, TTranslationEntity, TKey>
    : BaseRepository<TEntity, TKey>, ITranslatableLookupRepository<TEntity, TKey>
    where TEntity : LookupEntity<TKey>, ITranslatableEntity<TTranslationEntity, TKey>,new()
    where TTranslationEntity : class, ITranslationEntity<TKey>
    where TKey : notnull
{
    protected TranslatableLookupRepository(ApplicationDbContext context) : base(context)
    {
    }

    public virtual async Task<IReadOnlyList<TEntity>> GetActiveAsync(string LanguageCode,
        CancellationToken cancellationToken = default)
    {
        var languageCode = LanguageCode.ToLowerInvariant();

        return await DbSet
            .AsNoTracking()
            .Where(x => x.Active)
            .Select(x => new TEntity
            {
                Id = x.Id,
                Code = x.Code,
                Active = x.Active,
                Name = x.Translations
                    .Where(t => t.LanguageCode == languageCode)
                    .Select(t => t.Name)
                    .FirstOrDefault() ?? x.Name
            })
            .ToListAsync(cancellationToken)
            ;
    }
    public virtual async Task<TEntity?> GetByCodeAsync(string LanguageCode,
        string code,
        CancellationToken cancellationToken = default)
    {
        var languageCode = LanguageCode.ToLowerInvariant();

        return await DbSet
            .AsNoTracking()
            .Where(x=>x.Code==code)
            .Select(x => new TEntity
            {
                Id = x.Id,
                Code = x.Code,
                Active = x.Active,
                Name = x.Translations
                    .Where(t => t.LanguageCode == languageCode)
                    .Select(t => t.Name)
                    .FirstOrDefault() ?? x.Name
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
