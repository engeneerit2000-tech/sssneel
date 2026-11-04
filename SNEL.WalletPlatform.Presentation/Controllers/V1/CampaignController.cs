using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Common.Security;
using SNEL.WalletPlatform.Application.Notifications.Interfaces;
using SNEL.WalletPlatform.Application.Notifications.Models.Input;
using SNEL.WalletPlatform.Application.Notifications.Models.Output;
using SNEL.WalletPlatform.Presentation.Common;

namespace SNEL.WalletPlatform.Presentation.Controllers.V1;

[Route("api/v1/campaigns")]
[Authorize]
public sealed class CampaignController(ICampaignService campaignService) : Controllers.ApiControllerBase
{
    [HttpGet]
    [Authorize(Roles = IdentityRoleConstants.PlatformAdmin)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<CampaignOutputModel>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<CampaignOutputModel>>>> GetCampaigns(
        CancellationToken cancellationToken = default)
    {
        var result = await campaignService.GetAllAsync(cancellationToken);
        return FromResult(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = IdentityRoleConstants.PlatformAdmin)]
    [ProducesResponseType(typeof(ApiResponse<CampaignOutputModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CampaignOutputModel>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CampaignOutputModel>>> GetCampaignById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await campaignService.GetByIdAsync(id, cancellationToken);
        return FromResult(result);
    }

    [HttpPost]
    [Authorize(Roles = IdentityRoleConstants.PlatformAdmin)]
    [ProducesResponseType(typeof(ApiResponse<CampaignOutputModel>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<CampaignOutputModel>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<CampaignOutputModel>>> CreateCampaign(
        [FromBody] CreateCampaignInputModel request,
        CancellationToken cancellationToken = default)
    {
        var result = await campaignService.CreateAsync(request, cancellationToken);
        if (!result.IsSuccess)
        {
            return FromResult(result);
        }

        return CreatedAtAction(
            nameof(GetCampaignById),
            new { id = result.Value!.Id },
            new ApiResponse<CampaignOutputModel>(AppHttpStatusCodes.Created, result.Message, result.Value));
    }

    [HttpPost("{id:guid}")]
    [Authorize(Roles = IdentityRoleConstants.PlatformAdmin)]
    [ProducesResponseType(typeof(ApiResponse<CampaignOutputModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CampaignOutputModel>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<CampaignOutputModel>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CampaignOutputModel>>> UpdateCampaign(
        Guid id,
        [FromBody] UpdateCampaignInputModel request,
        CancellationToken cancellationToken = default)
    {
        var result = await campaignService.UpdateAsync(id, request, cancellationToken);
        return FromResult(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = IdentityRoleConstants.PlatformAdmin)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteCampaign(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await campaignService.DeleteAsync(id, cancellationToken);
        return FromResult(result);
    }
}
