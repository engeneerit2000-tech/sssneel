using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SNEL.WalletPlatform.Application.BillingServices.Models.Input;
using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Payments.Interfaces;
using SNEL.WalletPlatform.Application.Payments.Models.Input;
using SNEL.WalletPlatform.Application.Payments.Models.Output;
using SNEL.WalletPlatform.Application.Common.Security;
using SNEL.WalletPlatform.Domain.Payments;
using SNEL.WalletPlatform.Presentation.Common;
using SNEL.WalletPlatform.Application.Common.Models.Input;

namespace SNEL.WalletPlatform.Presentation.Controllers.V1;

[Authorize]
[Route("api/v1/payments")]
public sealed class PaymentController(
    IPaymentService paymentService) : Controllers.ApiControllerBase

{
    [HttpGet("check-status")]
    [ProducesResponseType(typeof(ApiResponse<PaymentStatus>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PaymentStatus>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<PaymentStatus>>> CheckPaymentStatus(
        [FromQuery] Guid PaymentId,
        CancellationToken cancellationToken = default)
    {
        //var acceptLanguage = Request.Headers.AcceptLanguage.ToString();
        var result = await paymentService.SyncPaymentStatusWithPsp(
            PaymentId);
        return FromResult(result);
    }

    [HttpGet("history")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<PaymentWithOrderAndItemsOutputModel>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<PaymentWithOrderAndItemsOutputModel>>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<PagedResult<PaymentWithOrderAndItemsOutputModel>>>> GetPaymentHistory(
        [FromQuery] PaymentHistoryQueryInputModel query,
        CancellationToken cancellationToken = default)
    {
        var acceptLanguage = Request.Headers.AcceptLanguage.ToString();
        var result = await paymentService.GetPaymentHistoryAsync(
            acceptLanguage,
            query,
            cancellationToken);
        return FromResult(result);
    }

    [HttpGet("scope-history")]
    [Authorize(Roles = $"{IdentityRoleConstants.PlatformAdmin},{IdentityRoleConstants.BillerAdmin},{IdentityRoleConstants.PspAdmin}")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<PaymentWithOrderAndItemsOutputModel>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<PaymentWithOrderAndItemsOutputModel>>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PagedResult<PaymentWithOrderAndItemsOutputModel>>>> GetMyPaymentHistory(
        [FromQuery] PaymentHistoryQueryInputModel query,
        CancellationToken cancellationToken = default)
    {
        var acceptLanguage = Request.Headers.AcceptLanguage.ToString();
        var result = await paymentService.GetScopePaymentHistoryAsync(
            acceptLanguage,
            query,
            cancellationToken);
        return FromResult(result);
    }
    [HttpGet("scope-history/export")]
    [Authorize(Roles = $"{IdentityRoleConstants.PlatformAdmin},{IdentityRoleConstants.BillerAdmin},{IdentityRoleConstants.PspAdmin}")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<PaymentWithOrderAndItemsOutputModel>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<PaymentWithOrderAndItemsOutputModel>>), StatusCodes.Status404NotFound)]
    public async Task<FileContentResult> ExportScopePaymentHistory(
        [FromQuery] PaymentHistoryQueryInputModel query,
        CancellationToken cancellationToken = default)
    {
        var acceptLanguage = Request.Headers.AcceptLanguage.ToString();
        var result = await paymentService.ExportScopePaymentHistoryAsync(acceptLanguage, query, cancellationToken);
        return result.Value;
    }
    [HttpGet("topup-history")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<PaymentWithOrderAndItemsOutputModel>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<PaymentWithOrderAndItemsOutputModel>>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<PagedResult<PaymentWithOrderAndItemsOutputModel>>>> GetTopUpPaymentHistory(
        [FromQuery] TopUpPaymentHistoryQueryInputModel query,
        CancellationToken cancellationToken = default)
    {
        var acceptLanguage = Request.Headers.AcceptLanguage.ToString();
        var result = await paymentService.GetTopUpPaymentHistoryAsync(acceptLanguage, query, cancellationToken);
        return FromResult(result);
    }

    [HttpGet("scope-topup-history")]
    [Authorize(Roles = $"{IdentityRoleConstants.PlatformAdmin},{IdentityRoleConstants.BillerAdmin},{IdentityRoleConstants.PspAdmin}")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<PaymentWithOrderAndItemsOutputModel>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<PaymentWithOrderAndItemsOutputModel>>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PagedResult<PaymentWithOrderAndItemsOutputModel>>>> GetScopeTopUpPaymentHistory(
        [FromQuery] TopUpPaymentHistoryQueryInputModel query,
        CancellationToken cancellationToken = default)
    {
        var acceptLanguage = Request.Headers.AcceptLanguage.ToString();
        var result = await paymentService.GetScopeTopUpPaymentHistoryAsync(acceptLanguage, query, cancellationToken);
        return FromResult(result);
    }
    [HttpGet("scope-topup-history/export")]
    [Authorize(Roles = $"{IdentityRoleConstants.PlatformAdmin},{IdentityRoleConstants.BillerAdmin},{IdentityRoleConstants.PspAdmin}")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<PaymentWithOrderAndItemsOutputModel>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<PaymentWithOrderAndItemsOutputModel>>), StatusCodes.Status404NotFound)]
    public async Task<FileContentResult> ExportScopeTopUpPaymentHistory(
        [FromQuery] TopUpPaymentHistoryQueryInputModel query,
        CancellationToken cancellationToken = default)
    {
        var acceptLanguage = Request.Headers.AcceptLanguage.ToString();
        var result = await paymentService.ExportScopeTopUpPaymentHistoryAsync(acceptLanguage, query, cancellationToken);
        return result.Value;
    }

    /// <summary>
    /// Gets payment details in the same shape as a single item from <c>GET /api/v1/payments/history</c>.
    /// </summary>
    [HttpGet("{paymentId:guid}/details")]
    [ProducesResponseType(typeof(ApiResponse<PaymentWithOrderAndItemsOutputModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PaymentWithOrderAndItemsOutputModel>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PaymentWithOrderAndItemsOutputModel>>> GetPaymentDetails(
        [FromRoute] Guid paymentId,
        CancellationToken cancellationToken)
    {
        var acceptLanguage = Request.Headers.AcceptLanguage.ToString();
        var result = await paymentService.GetPaymentDetailsAsync(acceptLanguage, paymentId, cancellationToken);
        return FromResult(result);
    }

    /// <summary>
    /// Gets the PSP transaction audit trail for the given payment.
    /// Access:
    ///   - PSP admin: payment's PSP provider belongs to admin's partner.
    ///   - Biller admin: at least one POI's BillingService belongs to admin's biller partner.
    /// Raw request/response bodies are only exposed to admins.
    /// </summary>
    [HttpGet("{paymentId:guid}/transactions")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<PaymentTxnOutputModel>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<PaymentTxnOutputModel>>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<PaymentTxnOutputModel>>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PagedResult<PaymentTxnOutputModel>>>> GetPaymentTransactions(
        [FromRoute] Guid paymentId,
        [FromQuery] PaymentTxnQueryInputModel query,
        CancellationToken cancellationToken)
    {
        var result = await paymentService.GetPaymentTransactionsAsync(paymentId, query, cancellationToken);
        return FromResult(result);
    }

    [HttpPost("{paymentId:guid}/manual-complete")]
    [Authorize(Roles = $"{IdentityRoleConstants.PlatformAdmin},{IdentityRoleConstants.BillerAdmin},{IdentityRoleConstants.PspAdmin}")]
    [ProducesResponseType(typeof(ApiResponse<PaymentStatus>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PaymentStatus>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<PaymentStatus>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<PaymentStatus>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<PaymentStatus>>> ManualCompletePayment(
        [FromRoute] Guid paymentId,
        CancellationToken cancellationToken = default)
    {
        var result = await paymentService.ManualCompletePaymentAsync(paymentId, cancellationToken);
        return FromResult(result);
    }

    [HttpPost("{paymentId:guid}/manual-apply")]
    [Authorize(Roles = $"{IdentityRoleConstants.PlatformAdmin},{IdentityRoleConstants.BillerAdmin},{IdentityRoleConstants.PspAdmin}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<bool>>> ManualApplyPayment(
        [FromRoute] Guid paymentId,
        CancellationToken cancellationToken = default)
    {
        var result = await paymentService.ManualApplyPaymentAsync(paymentId, cancellationToken);
        return FromResult(result);
    }

    [HttpPost("manual-reconcile-poi")]
    [Authorize(Roles = $"{IdentityRoleConstants.PlatformAdmin},{IdentityRoleConstants.BillerAdmin},{IdentityRoleConstants.PspAdmin}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<bool>>> ManualReconcilePoi(
        [FromBody] ManualReconcilePoiInputModel request,
        CancellationToken cancellationToken = default)
    {
        var result = await paymentService.ManualReconcilePoiAsync(request.PaymentId, request.ExternalRef, cancellationToken);
        return FromResult(result);
    }

    [HttpGet("{paymentId:guid}/payment-pdf")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<byte[]>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<byte[]>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<byte[]>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPaymentPdf([FromRoute] Guid paymentId, CancellationToken cancellationToken)
    {
        var acceptLanguage = Request.Headers.AcceptLanguage.ToString();
        var result = await paymentService.GetPaymentPdfAsync(paymentId, acceptLanguage, cancellationToken);

        if (!result.IsSuccess)
            return StatusCode((int)result.StatusCode, new ApiResponse<byte[]>(false, default, result.Message));

        Response.AppendXMessageHeader(result.Message);

        return File(result.Value!, "application/pdf", "payment.pdf");
    }

    [HttpPost("init-topup")]
    [ProducesResponseType(typeof(ApiResponse<PaymentInitializationOutputModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PaymentInitializationOutputModel>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<PaymentInitializationOutputModel>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<PaymentInitializationOutputModel>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PaymentInitializationOutputModel>>> InitTopup(
        [FromBody] PaymentInitializeInputModel request)
    {
        var result = await paymentService.InitTopupPayment(request);
        return FromResult(result);
    }
    
    [HttpPost("topup-summary")]
    [ProducesResponseType(typeof(ApiResponse<PaymentSummaryOutputModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PaymentSummaryOutputModel>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<PaymentSummaryOutputModel>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<PaymentSummaryOutputModel>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PaymentSummaryOutputModel>>> TopupSummary(
        [FromBody] PaymentInitializeInputModel request)
    {        
        var result = await paymentService.TopupSummary(request);
        return FromResult(result);
    }

    [HttpPost("meter-charge-summary")]
    [ProducesResponseType(typeof(ApiResponse<PaymentSummaryOutputModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PaymentSummaryOutputModel>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<PaymentSummaryOutputModel>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<PaymentSummaryOutputModel>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PaymentSummaryOutputModel>>> BillPaymentSummary(
        [FromBody] InitMeterChargePaymentInputModel request)
    {
        var result = await paymentService.MeterRechargePaymentSummary(request);
        return FromResult(result);
    }

    [HttpPost("init-meter-charge")]
    [ProducesResponseType(typeof(ApiResponse<PaymentInitializationOutputModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PaymentInitializationOutputModel>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<PaymentInitializationOutputModel>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<PaymentInitializationOutputModel>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PaymentInitializationOutputModel>>> InitMeterChargePayment(
        [FromBody] InitMeterChargePaymentInputModel request)
    {
        var result = await paymentService.InitMeterChargePayment(request);
        return FromResult(result);
    }
    
    
    [HttpPost("bill-payment-summary")]
    [ProducesResponseType(typeof(ApiResponse<PaymentSummaryOutputModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PaymentSummaryOutputModel>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<PaymentSummaryOutputModel>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<PaymentSummaryOutputModel>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PaymentSummaryOutputModel>>> BillPaymentSummary(
        [FromBody] PayBillsInputModel request)
    {
        var result = await paymentService.BillPaymentSummary(request);
        return FromResult(result);
    }

    [HttpPost("init-bill-payment")]
    [ProducesResponseType(typeof(ApiResponse<PaymentInitializationOutputModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PaymentInitializationOutputModel>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<PaymentInitializationOutputModel>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<PaymentInitializationOutputModel>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PaymentInitializationOutputModel>>> InitBillPayment(
        [FromBody] PayBillsInputModel request)
    {
        var result = await paymentService.InitBillPayment(request);
        return FromResult(result);
    }

    [HttpPost("confirm-bill-payment")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PaymentConfirmationOutputModel>>> ConfirmBillPayment(
        [FromBody] ConfirmBillPaymentInputModel request)
    {
        var result = await paymentService.ConfirmBillPayment(request.PaymentId);
        return FromResult(result);
    }

    /// <summary>
    /// Cancels a payment after init while it is still Pending or Processing. Final statuses cannot be cancelled.
    /// </summary>
    [HttpPost("{paymentId:guid}/cancel")]
    [ProducesResponseType(typeof(ApiResponse<PaymentConfirmationOutputModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PaymentConfirmationOutputModel>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<PaymentConfirmationOutputModel>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<PaymentConfirmationOutputModel>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<PaymentConfirmationOutputModel>>> CancelPaymentAfterInit(
        [FromRoute] Guid paymentId)
    {
        var result = await paymentService.TerminatePaymentAfterInitAsync(paymentId);
        return FromResult(result);
    }

    [HttpGet("for-review")]
    [Authorize(Roles = $"{IdentityRoleConstants.Admin},{IdentityRoleConstants.BillerAdmin}")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<PaymentOutputModel>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<PaymentOutputModel>>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<PagedResult<PaymentOutputModel>>>> GetPaymentsForReview(
        [FromQuery] PaymentReviewQueryInputModel model,
        CancellationToken cancellationToken)
    {
        var result = await paymentService.GetPaymentsForReviewAsync(model, cancellationToken);
        return FromResult(result);
    }
}
