using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SNEL.WalletPlatform.Application.Accounting.Interfaces;
using SNEL.WalletPlatform.Application.Accounting.Models.Input;
using SNEL.WalletPlatform.Application.Accounting.Models.Output;
using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Common.Security;
using SNEL.WalletPlatform.Application.Partners.Interfaces;
using SNEL.WalletPlatform.Application.Partners.Models.Input;
using SNEL.WalletPlatform.Application.Partners.Models.Output;
using SNEL.WalletPlatform.Presentation.Common;

namespace SNEL.WalletPlatform.Presentation.Controllers.V1;

[Authorize]
[Route("api/v1/partners")]
[Authorize(Roles = $"{IdentityRoleConstants.PlatformAdmin},{IdentityRoleConstants.BillerAdmin},{IdentityRoleConstants.PspAdmin},{IdentityRoleConstants.Customer}")]

public sealed class PartnerController(IPartnerService partnerService) : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<PartnerOutputModel>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<PartnerOutputModel>>>> GetPartners(
        [FromQuery] PartnersQueryInputModel query)
    {
        var result = await partnerService.GetPartnersAsync(query);
        return FromResult(result);
    }

    [HttpGet("related-partners")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<PartnerOutputModel>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<PartnerOutputModel>>>> GetRelatedPartners(
        [FromQuery] RelatedPartnersQueryInputModel query)
    {
        var result = await partnerService.GetVisiblePartnersAsync(query);
        return FromResult(result);
    }
}
