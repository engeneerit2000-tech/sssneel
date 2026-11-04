using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SNEL.WalletPlatform.Application.BillingServices.Interfaces;
using SNEL.WalletPlatform.Application.BillingServices.Models.Input;
using SNEL.WalletPlatform.Application.BillingServices.Models.Output;
using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Common.Security;
using SNEL.WalletPlatform.Presentation.Common;

namespace SNEL.WalletPlatform.Presentation.Controllers.V1;

[Authorize]
[Route("api/v1/billing/services")]
public sealed class BillingServiceController(
    IBillingServiceService billingServiceService) : Controllers.ApiControllerBase
{
    [HttpPost]
    [Authorize(Roles = $"{IdentityRoleConstants.PlatformAdmin},{IdentityRoleConstants.BillerAdmin}")]
    [ProducesResponseType(typeof(ApiResponse<BillingServiceOutputModel>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<BillingServiceOutputModel>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<BillingServiceOutputModel>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<BillingServiceOutputModel>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<BillingServiceOutputModel>>> Create(
        [FromBody] CreateBillingServiceInputModel request,
        CancellationToken cancellationToken = default)
    {
        var result = await billingServiceService.CreateAsync(request, cancellationToken);
        if (!result.IsSuccess)
        {
            return FromResult(result);
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Value!.Id },
            new ApiResponse<BillingServiceOutputModel>(AppHttpStatusCodes.Created, result.Message, result.Value));
    }

    [HttpGet("{id:long}")]
    [Authorize(Roles = $"{IdentityRoleConstants.PlatformAdmin},{IdentityRoleConstants.Customer},{IdentityRoleConstants.BillerAdmin}")]
    [ProducesResponseType(typeof(ApiResponse<BillingServiceOutputModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<BillingServiceOutputModel>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<BillingServiceOutputModel>>> GetById(
        long id,
        CancellationToken cancellationToken = default)
    {
        var result = await billingServiceService.GetByIdAsync(id, cancellationToken);
        return FromResult(result);
    }

    [HttpPost("{id:long}")]
    [Authorize(Roles = $"{IdentityRoleConstants.PlatformAdmin},{IdentityRoleConstants.BillerAdmin}")]
    [ProducesResponseType(typeof(ApiResponse<BillingServiceOutputModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<BillingServiceOutputModel>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<BillingServiceOutputModel>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<BillingServiceOutputModel>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<BillingServiceOutputModel>>> Update(
        long id,
        [FromBody] UpdateBillingServiceInputModel request,
        CancellationToken cancellationToken = default)
    {
        var result = await billingServiceService.UpdateAsync(id, request, cancellationToken);
        return FromResult(result);
    }

    [HttpDelete("{id:long}")]
    [Authorize(Roles = $"{IdentityRoleConstants.PlatformAdmin},{IdentityRoleConstants.BillerAdmin}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(
        long id,
        CancellationToken cancellationToken = default)
    {
        var result = await billingServiceService.DeleteAsync(id, cancellationToken);
        return FromResult(result);
    }
}
