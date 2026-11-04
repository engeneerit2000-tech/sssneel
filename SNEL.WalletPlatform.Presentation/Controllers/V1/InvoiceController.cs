using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Common.Security;
using SNEL.WalletPlatform.Application.Invoices.Interfaces;
using SNEL.WalletPlatform.Application.Invoices.Models.Input;
using SNEL.WalletPlatform.Application.Invoices.Models.Output;
using SNEL.WalletPlatform.Presentation.Common;

namespace SNEL.WalletPlatform.Presentation.Controllers.V1;

[Authorize]
[Route("api/v1/invoices")]
[Authorize(Roles = $"{IdentityRoleConstants.PlatformAdmin},{IdentityRoleConstants.BillerAdmin},{IdentityRoleConstants.PspAdmin},{IdentityRoleConstants.Customer}")]
public sealed class InvoiceController(IInvoiceService invoiceService) : ApiControllerBase
{
    [HttpGet("customer-subscription")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<InvoiceOutputModel>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<InvoiceOutputModel>>>> GetCustomerSubscriptionInvoices(
        [FromQuery] InvoicesQueryInputModel query)
    {
        var result = await invoiceService.GetCustomerSubscriptionInvoicesAsync(query);
        return FromResult(result);
    }

    [HttpGet("{id:guid}/html-preview")]
    public async Task<IActionResult> GetHtmlPreview(Guid id, CancellationToken cancellationToken)
    {
        var result = await invoiceService.GetInvoiceHtmlPreviewAsync(id, cancellationToken);

        if (!result.IsSuccess)
            return StatusCode((int)result.StatusCode, new ApiResponse<string>(false, default, result.Message));

        return Content(result.Value!, "text/html");
    }

    //TODO: should change response from byte[] to base64
    [HttpGet("{id:guid}/invoice-pdf")]
    public async Task<IActionResult> GetInvoicePdf(Guid id, CancellationToken cancellationToken)
    {
        var result = await invoiceService.GetInvoicePdfAsync(id, cancellationToken);

        if (!result.IsSuccess)
            return StatusCode((int)result.StatusCode, new ApiResponse<byte[]>(false, default, result.Message));

        Response.AppendXMessageHeader(result.Message);

        return File(result.Value!, "application/pdf", "invoice.pdf");
    }
}
