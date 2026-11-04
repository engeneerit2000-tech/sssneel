using Microsoft.Extensions.Localization;
using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Common.Localization;
using SNEL.WalletPlatform.Application.Lookups.Enums;
using SNEL.WalletPlatform.Application.Lookups.Interfaces;
using SNEL.WalletPlatform.Application.Lookups.Models.Output;

namespace SNEL.WalletPlatform.Application.Lookups.Services;

public sealed class LookupTranslationService(
    ILookupTranslationRepository repository,
    IStringLocalizer<SharedResource> localizer) : ILookupTranslationService
{
    /// <summary>
    /// Locales in which lookup translations may be stored. Kept in sync with
    /// <c>Program.cs</c>'s <c>SupportedCultures</c> so an admin cannot create a
    /// row the request-localization pipeline would never pick up.
    /// </summary>
    private static readonly HashSet<string> SupportedLanguages =
        new(StringComparer.OrdinalIgnoreCase) { "en", "fr" };

    public async Task<Result<IReadOnlyList<LookupTranslationOutputModel>>> GetAsync(
        LookupType type,
        int lookupId,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(typeof(LookupType), type))
        {
            return Result<IReadOnlyList<LookupTranslationOutputModel>>.Failure(
                localizer[LocalizationKeys.LookupTranslationTypeInvalid],
                AppHttpStatusCodes.BadRequest);
        }

        var exists = await repository.LookupExistsAsync(type, lookupId, cancellationToken);
        if (!exists)
        {
            return Result<IReadOnlyList<LookupTranslationOutputModel>>.Failure(
                localizer[LocalizationKeys.LookupTranslationLookupNotFound, type, lookupId],
                AppHttpStatusCodes.NotFound);
        }

        var translations = await repository.GetAsync(type, lookupId, cancellationToken);
        return Result<IReadOnlyList<LookupTranslationOutputModel>>.Success(translations);
    }

    public async Task<Result<LookupTranslationOutputModel>> UpsertAsync(
        LookupType type,
        int lookupId,
        string languageCode,
        string name,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(typeof(LookupType), type))
        {
            return Result<LookupTranslationOutputModel>.Failure(
                localizer[LocalizationKeys.LookupTranslationTypeInvalid],
                AppHttpStatusCodes.BadRequest);
        }

        if (string.IsNullOrWhiteSpace(languageCode))
        {
            return Result<LookupTranslationOutputModel>.Failure(
                localizer[LocalizationKeys.LookupTranslationLanguageRequired],
                AppHttpStatusCodes.BadRequest);
        }

        var normalizedLanguage = languageCode.Trim().ToLowerInvariant();
        if (!SupportedLanguages.Contains(normalizedLanguage))
        {
            return Result<LookupTranslationOutputModel>.Failure(
                localizer[LocalizationKeys.LookupTranslationLanguageNotSupported, languageCode],
                AppHttpStatusCodes.BadRequest);
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return Result<LookupTranslationOutputModel>.Failure(
                localizer[LocalizationKeys.LookupTranslationNameRequired],
                AppHttpStatusCodes.BadRequest);
        }

        var exists = await repository.LookupExistsAsync(type, lookupId, cancellationToken);
        if (!exists)
        {
            return Result<LookupTranslationOutputModel>.Failure(
                localizer[LocalizationKeys.LookupTranslationLookupNotFound, type, lookupId],
                AppHttpStatusCodes.NotFound);
        }

        var (translation, wasAdded) = await repository.UpsertAsync(
            type,
            lookupId,
            normalizedLanguage,
            name.Trim(),
            cancellationToken);

        var message = wasAdded
            ? localizer[LocalizationKeys.LookupTranslationAdded]
            : localizer[LocalizationKeys.LookupTranslationUpdated];
        var statusCode = wasAdded ? AppHttpStatusCodes.Created : AppHttpStatusCodes.Ok;

        return Result<LookupTranslationOutputModel>.Success(translation, message, statusCode);
    }
}
