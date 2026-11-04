using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SNEL.WalletPlatform.Application.Lookups.Interfaces;
using SNEL.WalletPlatform.Application.Lookups.Models.Output;
using SNEL.WalletPlatform.Presentation.Common;

namespace SNEL.WalletPlatform.Presentation.Controllers.V1;

[AllowAnonymous]
[Route("api/v1/lookups")]
public sealed class LookupsController(ILookupService lookupService, IPlatformConstantService platformConstantService) : Controllers.ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<AllLookupsOutputModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AllLookupsOutputModel>>> GetAll()
    {
        var result = await lookupService.GetAllAsync();
        return FromResult(result);
    }

    [HttpGet("latest-mobile-version")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<string>>> GetLatestMobileVersionAsync()
    {
        var result = await platformConstantService.GetLatestMobileVersionAsync();
        return FromResult(result);
    }

    [HttpGet("platform-constants")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<PlatformConstantOutputModel>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PlatformConstantOutputModel>>>> GetPlatformConstantsAsync(
        CancellationToken cancellationToken)
    {
        var result = await platformConstantService.GetAllAsync(cancellationToken);
        return FromResult(result);
    }
}
