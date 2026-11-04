using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Common.Security;
using SNEL.WalletPlatform.Application.Lookups.Enums;
using SNEL.WalletPlatform.Application.Lookups.Interfaces;
using SNEL.WalletPlatform.Application.Lookups.Models.Input;
using SNEL.WalletPlatform.Application.Lookups.Models.Output;
using SNEL.WalletPlatform.Presentation.Common;

namespace SNEL.WalletPlatform.Presentation.Controllers.V1;

/// <summary>
/// Admin-only endpoints for managing translations of lookup rows
/// (countries, regions, cities, areas, currencies, channels, etc.).
/// </summary>
[Authorize(Roles = IdentityRoleConstants.Admin)]
[Route("api/v1/lookup-translations")]
public sealed class LookupTranslationsController(
    ILookupTranslationService lookupTranslationService) : Controllers.ApiControllerBase
{
    [HttpGet("{type}/{lookupId:int}")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<LookupTranslationOutputModel>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<LookupTranslationOutputModel>>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<LookupTranslationOutputModel>>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LookupTranslationOutputModel>>>> Get(
        [FromRoute] LookupType type,
        [FromRoute] int lookupId,
        CancellationToken cancellationToken)
    {
        var result = await lookupTranslationService.GetAsync(type, lookupId, cancellationToken);
        return FromResult(result);
    }

    /// <summary>
    /// Adds a translation if none exists for the given language, otherwise updates it.
    /// Returns 201 on create and 200 on update.
    /// </summary>
    [HttpPut("{type}/{lookupId:int}/{languageCode}")]
    [ProducesResponseType(typeof(ApiResponse<LookupTranslationOutputModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LookupTranslationOutputModel>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<LookupTranslationOutputModel>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<LookupTranslationOutputModel>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<LookupTranslationOutputModel>>> Upsert(
        [FromRoute] LookupType type,
        [FromRoute] int lookupId,
        [FromRoute] string languageCode,
        [FromBody] UpsertLookupTranslationInputModel request,
        CancellationToken cancellationToken)
    {
        var result = await lookupTranslationService.UpsertAsync(
            type,
            lookupId,
            languageCode,
            request?.Name ?? string.Empty,
            cancellationToken);
        return FromResult(result);
    }
}
