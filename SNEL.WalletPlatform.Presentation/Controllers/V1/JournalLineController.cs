using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SNEL.WalletPlatform.Application.Accounting.Interfaces;
using SNEL.WalletPlatform.Application.Accounting.Models.Input;
using SNEL.WalletPlatform.Application.Accounting.Models.Output;
using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Common.Security;
using SNEL.WalletPlatform.Presentation.Common;

namespace SNEL.WalletPlatform.Presentation.Controllers.V1;

[Authorize]
[Route("api/v1/journal-lines")]
[Authorize(Roles = IdentityRoleConstants.Customer)]
public sealed class JournalLineController(IJournalLineService journalLineService) : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<JournalLineOutputModel>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<JournalLineOutputModel>>>> GetJournalLines(
        [FromQuery] JournalLinesQueryInputModel query)
    {
        var result = await journalLineService.GetJournalLinesAsync(query, HttpContext.RequestAborted);
        return FromResult(result);
    }
}
