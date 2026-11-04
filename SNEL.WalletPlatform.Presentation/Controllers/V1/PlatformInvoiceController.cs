using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SNEL.WalletPlatform.Application.Accounting.Interfaces;
using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Common.Security;
using SNEL.WalletPlatform.Presentation.Common;

namespace SNEL.WalletPlatform.Presentation.Controllers.V1;

[Authorize]
[Route("api/v1/platform-invoices")]
[Authorize(Roles = $"{IdentityRoleConstants.PlatformAdmin},{IdentityRoleConstants.BillerAdmin},{IdentityRoleConstants.PspAdmin},{IdentityRoleConstants.Customer}")]
public sealed class PlatformInvoiceController(IPlatformInvoiceService platformInvoiceService) : ApiControllerBase
{
    [HttpGet("{platformInvoiceId:long}/invoice-pdf")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<byte[]>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<byte[]>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPlatformInvoicePdf(
        [FromRoute] long platformInvoiceId,
        CancellationToken cancellationToken)
    {
        var acceptLanguage = Request.Headers.AcceptLanguage.ToString();
        var result = await platformInvoiceService.GetPlatformInvoicePdfAsync(
            platformInvoiceId,
            acceptLanguage,
            cancellationToken);

        if (!result.IsSuccess)
        {
            return StatusCode((int)result.StatusCode, new ApiResponse<byte[]>(false, default, result.Message));
        }

        Response.AppendXMessageHeader(result.Message);

        return File(result.Value!, "application/pdf", $"platform-invoice-{platformInvoiceId}.pdf");
    }
}
