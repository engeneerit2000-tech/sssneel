using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SNEL.WalletPlatform.Application.Common.Security;
using SNEL.WalletPlatform.Application.Billers.Interfaces;
using SNEL.WalletPlatform.Application.Billers.Models.Input;
using SNEL.WalletPlatform.Application.Billers.Models.Output;
using SNEL.WalletPlatform.Application.BillingServices.Interfaces;
using SNEL.WalletPlatform.Application.BillingServices.Models.Input;
using SNEL.WalletPlatform.Application.BillingServices.Models.Output;
using SNEL.WalletPlatform.Application.BillingServices.Services;
using SNEL.WalletPlatform.Presentation.Common;

namespace SNEL.WalletPlatform.Presentation.Controllers.V1;

[Authorize]
[Route("api/v1/billing")]
public sealed class BillingController(
    IBillingServiceService billingServiceService,
    IBillerService billerService) : Controllers.ApiControllerBase
{
    [HttpGet("services")]
    [Authorize(Roles = $"{IdentityRoleConstants.PlatformAdmin},{IdentityRoleConstants.BillerAdmin},{IdentityRoleConstants.Customer}")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<BillingServiceOutputModel>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<BillingServiceOutputModel>>>> GetBillingServices(
        [FromQuery] bool? isActive = true,
        CancellationToken cancellationToken = default)
    {
        var acceptLanguage = Request.Headers.AcceptLanguage.ToString();
        var result = await billingServiceService.GetBillingServicesAsync(acceptLanguage, isActive, cancellationToken);
        return FromResult(result);
    }

    [HttpGet("inquire")]
    [ProducesResponseType(typeof(ApiResponse<InquireOutputModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<InquireOutputModel>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<InquireOutputModel>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<InquireOutputModel>>> Inquire(
        [FromQuery] BillingInquiryInputModel query)
    {
        var result = await billingServiceService.Inquire(
            query.CustomerSubscriptionId,
            query.UnpaidOnly,
            HttpContext.RequestAborted);

        return FromResult(result);
    }

    [HttpGet("inquire-paged")]
    [ProducesResponseType(typeof(ApiResponse<PagedInquireOutputModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PagedInquireOutputModel>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<PagedInquireOutputModel>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PagedInquireOutputModel>>> InquirePaged(
        [FromQuery] PagedBillingInquiryInputModel query,
        CancellationToken cancellationToken)
    {
        var result = await billingServiceService.InquirePaged(query, cancellationToken);
        return FromResult(result);
    }

    [HttpGet("inquire-by-invoice-ref")]
    [ProducesResponseType(typeof(ApiResponse<InquireOutputModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<InquireOutputModel>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<InquireOutputModel>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<InquireOutputModel>>> InquireByInvoiceRef(
        [FromQuery] BillingInquiryByInvoiceRefInputModel query)
    {
        var result = await billingServiceService.InquireByInvoiceRef(
            query.InvoiceRef,
            query.UnpaidOnly,
            HttpContext.RequestAborted);

        return FromResult(result);
    }

    [HttpGet("inquire-prepaid-meter")]
    [ProducesResponseType(typeof(ApiResponse<PrepaidMeterInquiryOutputModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PrepaidMeterInquiryOutputModel>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<PrepaidMeterInquiryOutputModel>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PrepaidMeterInquiryOutputModel>>> InquirePrepaidMeter(
        [FromQuery] PrepaidMeterInquiryInputModel query)
    {
        var result = await billingServiceService.InquirePrepaidMeter(query.MeterNumber);
        return FromResult(result);
    }

    [HttpGet("get-custom-actions")]
    [ProducesResponseType(typeof(ApiResponse<InquireOutputModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<InquireOutputModel>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<InquireOutputModel>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<List<CustomActionsOutputModel>>>> GetCustomeActioins(
        [FromQuery] GetCustomActionsInputModel query)
    {
        var result = await billingServiceService.GetCustomActions(query.CustomerSubscriptionId);

        return FromResult(result);
    }

    [HttpPost("execute-custom-actions")]
    [ProducesResponseType(typeof(ApiResponse<InquireOutputModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<InquireOutputModel>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<InquireOutputModel>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> ExecuteCustomeActioins(
        [FromBody] ExecuteCustomActionsInputModel model)
    {
        var result = await billingServiceService.ExecuteCustomActions(model);

        return FromResult(result);
    }


    [HttpPost("billers")]
    [Authorize(Roles = $"{IdentityRoleConstants.PlatformAdmin},{IdentityRoleConstants.BillerAdmin}")]
    [ProducesResponseType(typeof(ApiResponse<AddBillerOutputModel>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<AddBillerOutputModel>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<AddBillerOutputModel>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<AddBillerOutputModel>>> AddBiller(
        [FromBody] AddBillerInputModel model,
        CancellationToken cancellationToken = default)
    {
        var result = await billerService.AddBillerAsync(model, cancellationToken);
        return FromResult(result);
    }

    [HttpGet("billers")]
    [Authorize(Roles = $"{IdentityRoleConstants.PlatformAdmin},{IdentityRoleConstants.BillerAdmin},{IdentityRoleConstants.Customer}")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<BillerOutputModel>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<BillerOutputModel>>>> GetBillers(
        CancellationToken cancellationToken = default)
    {
        var result = await billerService.GetAllAsync(cancellationToken);
        return FromResult(result);
    }

    [HttpGet("billers/{id:long}")]
    [ProducesResponseType(typeof(ApiResponse<BillerOutputModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<BillerOutputModel>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<BillerOutputModel>>> GetBillerById(
        long id,
        CancellationToken cancellationToken = default)
    {
        var result = await billerService.GetByIdAsync(id, cancellationToken);
        return FromResult(result);
    }

    [HttpPost("billers/{id:long}")]
    [Authorize(Roles = $"{IdentityRoleConstants.PlatformAdmin},{IdentityRoleConstants.BillerAdmin}")]
    [ProducesResponseType(typeof(ApiResponse<BillerOutputModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<BillerOutputModel>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<BillerOutputModel>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<BillerOutputModel>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<BillerOutputModel>>> UpdateBiller(
        long id,
        [FromBody] UpdateBillerInputModel model,
        CancellationToken cancellationToken = default)
    {
        var result = await billerService.UpdateAsync(id, model, cancellationToken);
        return FromResult(result);
    }

    [HttpDelete("billers/{id:long}")]
    [Authorize(Roles = $"{IdentityRoleConstants.PlatformAdmin},{IdentityRoleConstants.BillerAdmin}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteBiller(
        long id,
        CancellationToken cancellationToken = default)
    {
        var result = await billerService.DeleteAsync(id, cancellationToken);
        return FromResult(result);
    }
}
