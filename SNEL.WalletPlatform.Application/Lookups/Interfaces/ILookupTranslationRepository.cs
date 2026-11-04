using SNEL.WalletPlatform.Application.Lookups.Enums;
using SNEL.WalletPlatform.Application.Lookups.Models.Output;

namespace SNEL.WalletPlatform.Application.Lookups.Interfaces;

public interface ILookupTranslationRepository
{
    /// <summary>
    /// Checks whether the parent lookup row (e.g. Country with id=5) exists for the given <paramref name="type"/>.
    /// </summary>
    Task<bool> LookupExistsAsync(LookupType type, int lookupId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all translations attached to the given lookup row.
    /// </summary>
    Task<IReadOnlyList<LookupTranslationOutputModel>> GetAsync(
        LookupType type,
        int lookupId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts a new translation if none exists for (<paramref name="lookupId"/>, <paramref name="languageCode"/>)
    /// or updates the existing one's name. Returns the resulting row and a flag indicating which operation
    /// was performed (<c>true</c> = added, <c>false</c> = updated).
    /// </summary>
    Task<(LookupTranslationOutputModel Translation, bool WasAdded)> UpsertAsync(
        LookupType type,
        int lookupId,
        string languageCode,
        string name,
        CancellationToken cancellationToken = default);
}
