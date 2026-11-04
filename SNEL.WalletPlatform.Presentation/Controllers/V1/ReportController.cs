using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SNEL.WalletPlatform.Application.Accounting.Interfaces;
using SNEL.WalletPlatform.Application.Accounting.Models.Input;
using SNEL.WalletPlatform.Application.Accounting.Models.Output;
using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Common.Security;
using SNEL.WalletPlatform.Application.Payments.Interfaces;
using SNEL.WalletPlatform.Application.Payments.Models.Input;
using SNEL.WalletPlatform.Application.Payments.Models.Output;
using SNEL.WalletPlatform.Application.Payments.Services;
using SNEL.WalletPlatform.Application.Test;
using SNEL.WalletPlatform.Presentation.Common;

namespace SNEL.WalletPlatform.Presentation.Controllers.V1;

[Authorize]
[Route("api/v1/reports")]
[Authorize(Roles = $"{IdentityRoleConstants.PlatformAdmin},{IdentityRoleConstants.BillerAdmin},{IdentityRoleConstants.PspAdmin},{IdentityRoleConstants.WalletOwner}")]
public sealed class ReportController(IJournalLineService journalLineService, IPaymentService paymentService) : ApiControllerBase
{
    [HttpPost("journals/journal-lines-statement")]
    [ProducesResponseType(typeof(ApiResponse<PagedResultWithSummary<JournalLineStatementOutputModel, JournalLineStatementSummary>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResultWithSummary<JournalLineStatementOutputModel, JournalLineStatementSummary>>>> GetJournalLineStatement(
        [FromBody] JournalLinesStatementInputModel query)
    {
        var result = await journalLineService.GetJournalLineStatementAsync(query);
        return FromResult(result);
    }
    [HttpPost("journals/journal-lines-statement/export")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<FileContentResult> ExportJournalLineStatement(
        [FromBody] JournalLinesStatementInputModel query)
    {
        var result = await journalLineService.ExportJournalLineStatementAsync(query);
        return result.Value!;
    }
    [HttpPost("payments/completed-topup/by-currency")]
    [Authorize(Roles = $"{IdentityRoleConstants.PlatformAdmin},{IdentityRoleConstants.PspAdmin},{IdentityRoleConstants.WalletOwner}")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<CompletedTopupByCurrencyOutputModel>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CompletedTopupByCurrencyOutputModel>>>> GetCompletedTopupByCurrency(
        [FromBody] CompletedTopupByCurrencyInputModel model,
        CancellationToken cancellationToken = default)
    {
        var response = await paymentService.GetCompletedTopupByCurrencyAsync(model, cancellationToken);
        return FromResult(response);
    }

    [HttpPost("payments/completed-topup/by-currency/export")]
    [Authorize(Roles = $"{IdentityRoleConstants.PlatformAdmin},{IdentityRoleConstants.PspAdmin},{IdentityRoleConstants.WalletOwner}")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<FileContentResult> ExportCompletedTopupByCurrency(
        [FromBody] CompletedTopupByCurrencyInputModel model,
        CancellationToken cancellationToken = default)
    {
        var response = await paymentService.ExportCompletedTopupByCurrencyAsync(model, cancellationToken);
        return response.Value!;
    }

    [HttpPost("payments/completed-topup/by-currency/export-rows")]
    [Authorize(Roles = $"{IdentityRoleConstants.PlatformAdmin},{IdentityRoleConstants.PspAdmin},{IdentityRoleConstants.WalletOwner}")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<FileContentResult> ExportCompletedTopupByCurrencyRows(
        [FromBody] CompletedTopupByCurrencyInputModel model,
        CancellationToken cancellationToken = default)
    {
        var response = await paymentService.ExportCompletedTopupByCurrencyRowsAsync(model, cancellationToken);
        return response.Value!;
    }

    [HttpPost("payments/completed-topup/by-currency/rows")]
    [Authorize(Roles = $"{IdentityRoleConstants.PlatformAdmin},{IdentityRoleConstants.PspAdmin},{IdentityRoleConstants.WalletOwner}")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<PaymentOutputModel>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<PaymentOutputModel>>>> GetCompletedTopupByCurrencyRowsList(
        [FromBody] CompletedTopupByCurrencyRowsListInputModel model,
        CancellationToken cancellationToken = default)
    {
        var response = await paymentService.GetCompletedTopupByCurrencyRowsListAsync(model, cancellationToken);
        return FromResult(response);
    }

    [HttpPost("payments/completed-topup/by-date")]
    [Authorize(Roles = $"{IdentityRoleConstants.PlatformAdmin},{IdentityRoleConstants.PspAdmin},{IdentityRoleConstants.WalletOwner}")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<CompletedTopupByDateOutputModel>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CompletedTopupByDateOutputModel>>>> GetCompletedTopupByDate(
        [FromBody] CompletedTopupByDateInputModel model,
        CancellationToken cancellationToken = default)
    {
        var response = await paymentService.GetCompletedTopupByDateAsync(model, cancellationToken);
        return FromResult(response);
    }

    [HttpPost("payments/completed-topup/by-date/export")]
    [Authorize(Roles = $"{IdentityRoleConstants.PlatformAdmin},{IdentityRoleConstants.PspAdmin},{IdentityRoleConstants.WalletOwner}")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<FileContentResult> ExportCompletedTopupByDate(
        [FromBody] CompletedTopupByDateInputModel model,
        CancellationToken cancellationToken = default)
    {
        var response = await paymentService.ExportCompletedTopupByDateAsync(model, cancellationToken);
        return response.Value!;
    }

    [HttpPost("payments/completed-topup/by-date/export-rows")]
    [Authorize(Roles = $"{IdentityRoleConstants.PlatformAdmin},{IdentityRoleConstants.PspAdmin},{IdentityRoleConstants.WalletOwner}")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<FileContentResult> ExportCompletedTopupByDateRows(
        [FromBody] CompletedTopupByDateInputModel model,
        CancellationToken cancellationToken = default)
    {
        var response = await paymentService.ExportCompletedTopupByDateRowsAsync(model, cancellationToken);
        return response.Value!;
    }

    [HttpPost("payments/completed-topup/by-date/rows")]
    [Authorize(Roles = $"{IdentityRoleConstants.PlatformAdmin},{IdentityRoleConstants.PspAdmin},{IdentityRoleConstants.WalletOwner}")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<PaymentOutputModel>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<PaymentOutputModel>>>> GetCompletedTopupByDateRowsList(
        [FromBody] CompletedTopupByDateRowsListInputModel model,
        CancellationToken cancellationToken = default)
    {
        var response = await paymentService.GetCompletedTopupByDateRowsListAsync(model, cancellationToken);
        return FromResult(response);
    }

    [HttpPost("payments/completed-prepaid-paybill/by-date")]
    [Authorize(Roles = $"{IdentityRoleConstants.PlatformAdmin},{IdentityRoleConstants.BillerAdmin}")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<CompletedPrepaidPayBillByDateOutputModel>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CompletedPrepaidPayBillByDateOutputModel>>>> GetCompletedPrepaidPayBillByDate(
        [FromBody] CompletedPrepaidPayBillByDateInputModel model,
        CancellationToken cancellationToken = default)
    {
        var response = await paymentService.GetCompletedPrepaidPayBillByDateAsync(model, cancellationToken);
        return FromResult(response);
    }

    [HttpPost("payments/completed-prepaid-paybill/by-date/export")]
    [Authorize(Roles = $"{IdentityRoleConstants.PlatformAdmin},{IdentityRoleConstants.BillerAdmin}")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<FileContentResult> ExportCompletedPrepaidPayBillByDate(
        [FromBody] CompletedPrepaidPayBillByDateInputModel model,
        CancellationToken cancellationToken = default)
    {
        var response = await paymentService.ExportCompletedPrepaidPayBillByDateAsync(model, cancellationToken);
        return response.Value!;
    }

    [HttpPost("payments/completed-prepaid-paybill/by-date/export-rows")]
    [Authorize(Roles = $"{IdentityRoleConstants.PlatformAdmin},{IdentityRoleConstants.BillerAdmin}")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<FileContentResult> ExportCompletedPrepaidPayBillByDateRows(
        [FromBody] CompletedPrepaidPayBillByDateInputModel model,
        CancellationToken cancellationToken = default)
    {
        var response = await paymentService.ExportCompletedPrepaidPayBillByDateRowsAsync(model, cancellationToken);
        return response.Value!;
    }

    [HttpPost("payments/completed-prepaid-paybill/by-date/rows")]
    [Authorize(Roles = $"{IdentityRoleConstants.PlatformAdmin},{IdentityRoleConstants.BillerAdmin}")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<CompletedPrepaidPayBillByDateRowOutputModel>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<CompletedPrepaidPayBillByDateRowOutputModel>>>> GetCompletedPrepaidPayBillByDateRowsList(
        [FromBody] CompletedPrepaidPayBillByDateRowsListInputModel model,
        CancellationToken cancellationToken = default)
    {
        var response = await paymentService.GetCompletedPrepaidPayBillByDateRowsListAsync(model, cancellationToken);
        return FromResult(response);
    }

    [HttpPost("payments/completed-postpaid-paybill/by-date")]
    [Authorize(Roles = $"{IdentityRoleConstants.PlatformAdmin},{IdentityRoleConstants.BillerAdmin}")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<CompletedPostpaidPayBillByDateOutputModel>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CompletedPostpaidPayBillByDateOutputModel>>>> GetCompletedPostpaidPayBillByDate(
        [FromBody] CompletedPostpaidPayBillByDateInputModel model,
        CancellationToken cancellationToken = default)
    {
        var response = await paymentService.GetCompletedPostpaidPayBillByDateAsync(model, cancellationToken);
        return FromResult(response);
    }

    [HttpPost("payments/completed-postpaid-paybill/by-date/export")]
    [Authorize(Roles = $"{IdentityRoleConstants.PlatformAdmin},{IdentityRoleConstants.BillerAdmin}")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<FileContentResult> ExportCompletedPostpaidPayBillByDate(
        [FromBody] CompletedPostpaidPayBillByDateInputModel model,
        CancellationToken cancellationToken = default)
    {
        var response = await paymentService.ExportCompletedPostpaidPayBillByDateAsync(model, cancellationToken);
        return response.Value!;
    }

    [HttpPost("payments/completed-postpaid-paybill/by-date/export-rows")]
    [Authorize(Roles = $"{IdentityRoleConstants.PlatformAdmin},{IdentityRoleConstants.BillerAdmin}")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<FileContentResult> ExportCompletedPostpaidPayBillByDateRows(
        [FromBody] CompletedPostpaidPayBillByDateInputModel model,
        CancellationToken cancellationToken = default)
    {
        var response = await paymentService.ExportCompletedPostpaidPayBillByDateRowsAsync(model, cancellationToken);
        return response.Value!;
    }

    [HttpPost("payments/completed-postpaid-paybill/by-date/rows")]
    [Authorize(Roles = $"{IdentityRoleConstants.PlatformAdmin},{IdentityRoleConstants.BillerAdmin}")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<CompletedPostpaidPayBillByDateRowOutputModel>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<CompletedPostpaidPayBillByDateRowOutputModel>>>> GetCompletedPostpaidPayBillByDateRowsList(
        [FromBody] CompletedPostpaidPayBillByDateRowsListInputModel model,
        CancellationToken cancellationToken = default)
    {
        var response = await paymentService.GetCompletedPostpaidPayBillByDateRowsListAsync(model, cancellationToken);
        return FromResult(response);
    }

    [HttpPost("payments/completed-postpaid-paybill/by-center")]
    [Authorize(Roles = $"{IdentityRoleConstants.PlatformAdmin},{IdentityRoleConstants.BillerAdmin}")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<CompletedPostpaidPayBillByCenterOutputModel>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CompletedPostpaidPayBillByCenterOutputModel>>>> GetCompletedPostpaidPayBillByCenter(
        [FromBody] CompletedPostpaidPayBillByCenterInputModel model,
        CancellationToken cancellationToken = default)
    {
        var response = await paymentService.GetCompletedPostpaidPayBillByCenterAsync(model, cancellationToken);
        return FromResult(response);
    }

    [HttpPost("payments/completed-postpaid-paybill/by-center/export")]
    [Authorize(Roles = $"{IdentityRoleConstants.PlatformAdmin},{IdentityRoleConstants.BillerAdmin}")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<FileContentResult> ExportCompletedPostpaidPayBillByCenter(
        [FromBody] CompletedPostpaidPayBillByCenterInputModel model,
        CancellationToken cancellationToken = default)
    {
        var response = await paymentService.ExportCompletedPostpaidPayBillByCenterAsync(model, cancellationToken);
        return response.Value!;
    }

    [HttpPost("payments/completed-postpaid-paybill/by-center/export-rows")]
    [Authorize(Roles = $"{IdentityRoleConstants.PlatformAdmin},{IdentityRoleConstants.BillerAdmin}")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<FileContentResult> ExportCompletedPostpaidPayBillByCenterRows(
        [FromBody] CompletedPostpaidPayBillByCenterInputModel model,
        CancellationToken cancellationToken = default)
    {
        var response = await paymentService.ExportCompletedPostpaidPayBillByCenterRowsAsync(model, cancellationToken);
        return response.Value!;
    }

    [HttpPost("payments/completed-postpaid-paybill/by-center/rows")]
    [Authorize(Roles = $"{IdentityRoleConstants.PlatformAdmin},{IdentityRoleConstants.BillerAdmin}")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<CompletedPostpaidPayBillByCenterRowOutputModel>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<CompletedPostpaidPayBillByCenterRowOutputModel>>>> GetCompletedPostpaidPayBillByCenterRowsList(
        [FromBody] CompletedPostpaidPayBillByCenterRowsListInputModel model,
        CancellationToken cancellationToken = default)
    {
        var response = await paymentService.GetCompletedPostpaidPayBillByCenterRowsListAsync(model, cancellationToken);
        return FromResult(response);
    }

    [HttpPost("payments/topupRate/group")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<TopupRateOutputModel>>>> TopupRate([FromBody] TopupRateInputModel model)
    {
        var response = await paymentService.GetTopupRate(model);
        return FromResult(response);
    }
    [HttpPost("payments/topupRate/list")]
    public async Task<ActionResult<ApiResponse<PagedResult<PaymentOutputModel>>>> TopupRateList([FromBody] TopupRateListInputModel model)
    {
        var response = await paymentService.GetTopupRateList(model);
        return FromResult(response);
    }
    [HttpPost("payments/topupRate/export")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<FileContentResult> ExportTopupRateList(TopupRateExportInputModel model)
    {
        var response = await paymentService.ExportTopupRateList(model);        
        return response.Value;
    }
    [HttpPost("payments/topupRate/group-export")]
    public async Task<FileContentResult> ExportTopupRate([FromBody] TopupRateInputModel model)
    {
        var response = await paymentService.ExportTopupRate(model);
        return response.Value;
    }
    [HttpPost("payments/paymentRate/group")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PaymentRateOutputModel>>>> PaymentRate([FromBody] PaymentRateInputModel model)
    {
        var response = await paymentService.GetPaymentRate(model);
        return FromResult(response);
    }    
    [HttpPost("payments/paymentRate/list")]
    public async Task<ActionResult<ApiResponse<PagedResult<PaymentOrderItemOutputModel>>>> PaymentRateList([FromBody] PaymentRateListInputModel model)
    {
        var response = await paymentService.GetPaymentRateList(model);
        return FromResult(response);
    }    
    [HttpPost("payments/paymentRate/export")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<FileContentResult> ExportPaymentList(PaymentRateExportInputModel model)
    {
        var response = await paymentService.ExportPaymentList(model);
        return response.Value;
    }

    [HttpPost("payments/paymentRate/group-export")]
    public async Task<FileContentResult> ExportPaymentRate([FromBody] PaymentRateInputModel model)
    {
        var response = await paymentService.ExportPaymentRate(model);
        return response.Value;
    }
}
