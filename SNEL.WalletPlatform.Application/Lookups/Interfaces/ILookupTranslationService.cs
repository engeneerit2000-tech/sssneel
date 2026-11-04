using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Lookups.Enums;
using SNEL.WalletPlatform.Application.Lookups.Models.Output;

namespace SNEL.WalletPlatform.Application.Lookups.Interfaces;

public interface ILookupTranslationService
{
    Task<Result<IReadOnlyList<LookupTranslationOutputModel>>> GetAsync(
        LookupType type,
        int lookupId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Upserts a translation (adds if missing, updates the <c>Name</c> if present).
    /// Returns <c>201 Created</c> when a new row was inserted, otherwise <c>200 OK</c>.
    /// </summary>
    Task<Result<LookupTranslationOutputModel>> UpsertAsync(
        LookupType type,
        int lookupId,
        string languageCode,
        string name,
        CancellationToken cancellationToken = default);
}
