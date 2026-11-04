using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using SNEL.WalletPlatform.Application;
using SNEL.WalletPlatform.Application.Common.Localization;
using SNEL.WalletPlatform.Application.Common.Security;
using SNEL.WalletPlatform.Application.Jobs.Interfaces;
using SNEL.WalletPlatform.Presentation.Common;
using SNEL.WalletPlatform.Presentation.Controllers.V1.Models;

namespace SNEL.WalletPlatform.Presentation.Controllers.V1;

[Route("api/v1/jobs")]
public sealed class JobsController(
    IManualJobTriggerService manualJobTrigger,
    IStringLocalizer<SharedResource> localizer) : ApiControllerBase
{
    /// <summary>
    /// Trigger the Sync GCO En Cours Factures job to run once immediately for the given date range (Oracle pull-invoices → staging → sync).
    /// </summary>
    [HttpPost("biller-rollover/trigger")]
    [ProducesResponseType(typeof(ApiResponse<TriggerJobResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TriggerJobResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<TriggerJobResult>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TriggerJobResult>>> TriggerBillerRollover(
        [FromBody] TriggerBillerRolloverRequest request,
        CancellationToken cancellationToken)
    {
        if (request.End < request.Start)
            return BadRequest(new ApiResponse<TriggerJobResult>(false, default, localizer[LocalizationKeys.JobsEndMustBeOnOrAfterStart]));

        var triggered = await manualJobTrigger.TriggerBillerRolloverAsync(
            request.Start,
            request.End,
            request.Regions,
            request.RegisterNotificationOnPull,
            cancellationToken);
        if (!triggered)
            return NotFound(new ApiResponse<TriggerJobResult>(false, default, localizer[LocalizationKeys.JobsNotRegisteredOrNotAvailable]));
        return Ok(new ApiResponse<TriggerJobResult>(true, new TriggerJobResult(true, localizer[LocalizationKeys.JobsTriggeredSuccessfully])));
    }
    /// <summary>
    /// Trigger the Sync GCO En Cours Factures job to run once immediately for the given date range (Oracle pull-invoices → staging → sync).
    /// </summary>
    [HttpPost("sync-gco-en-cours-factures/trigger")]
    [ProducesResponseType(typeof(ApiResponse<TriggerJobResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TriggerJobResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<TriggerJobResult>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TriggerJobResult>>> TriggerSyncGcoEnCoursFactures(
        [FromBody] TriggerSyncGcoEnCoursFacturesRequest request,
        CancellationToken cancellationToken)
    {
        if (request.End < request.Start)
            return BadRequest(new ApiResponse<TriggerJobResult>(false, default, localizer[LocalizationKeys.JobsEndMustBeOnOrAfterStart]));

        var triggered = await manualJobTrigger.TriggerSyncGcoEnCoursFacturesAsync(request.Start, request.End, cancellationToken);
        if (!triggered)
            return NotFound(new ApiResponse<TriggerJobResult>(false, default, localizer[LocalizationKeys.JobsNotRegisteredOrNotAvailable]));
        return Ok(new ApiResponse<TriggerJobResult>(true, new TriggerJobResult(true, localizer[LocalizationKeys.JobsTriggeredSuccessfully])));
    }

    /// <summary>
    /// Trigger the Sync Biller Customers job to run once immediately (Oracle GCO_RACCORDEMENTS → staging → merge). No date range.
    /// </summary>
    [HttpPost("sync-biller-customers/trigger")]
    [ProducesResponseType(typeof(ApiResponse<TriggerJobResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TriggerJobResult>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TriggerJobResult>>> TriggerSyncBillerCustomers(
        CancellationToken cancellationToken)
    {
        var triggered = await manualJobTrigger.TriggerSyncBillerCustomersAsync(cancellationToken);
        if (!triggered)
            return NotFound(new ApiResponse<TriggerJobResult>(false, default, localizer[LocalizationKeys.JobsNotRegisteredOrNotAvailable]));
        return Ok(new ApiResponse<TriggerJobResult>(true, new TriggerJobResult(true, localizer[LocalizationKeys.JobsTriggeredSuccessfully])));
    }

    /// <summary>
    /// Trigger the Expire Stale Pending Payments job once immediately (pending payments older than the stale window are marked expired).
    /// </summary>
    [HttpPost("expire-stale-pending-payments/trigger")]
    [ProducesResponseType(typeof(ApiResponse<TriggerJobResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TriggerJobResult>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TriggerJobResult>>> TriggerExpireStalePendingPayments(
        [FromQuery] int? pageSize = null,
        [FromQuery] int? staleAfterMinutes = null,
        CancellationToken cancellationToken = default)
    {
        var triggered = await manualJobTrigger.TriggerExpireStalePendingPaymentsAsync(
            pageSize,
            staleAfterMinutes,
            cancellationToken);
        if (!triggered)
            return NotFound(new ApiResponse<TriggerJobResult>(false, default, localizer[LocalizationKeys.JobsNotRegisteredOrNotAvailable]));
        return Ok(new ApiResponse<TriggerJobResult>(true, new TriggerJobResult(true, localizer[LocalizationKeys.JobsTriggeredSuccessfully])));
    }

    /// <summary>
    /// Trigger the Send New Invoice Notifications job once immediately.
    /// </summary>
    [HttpPost("send-new-invoice-notifications/trigger")]
    [ProducesResponseType(typeof(ApiResponse<TriggerJobResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TriggerJobResult>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TriggerJobResult>>> TriggerSendNewInvoiceNotifications(
        [FromQuery] int? pageSize = null,
        CancellationToken cancellationToken = default)
    {
        var triggered = await manualJobTrigger.TriggerSendNewInvoiceNotificationsAsync(pageSize, cancellationToken);
        if (!triggered)
            return NotFound(new ApiResponse<TriggerJobResult>(false, default, localizer[LocalizationKeys.JobsNotRegisteredOrNotAvailable]));
        return Ok(new ApiResponse<TriggerJobResult>(true, new TriggerJobResult(true, localizer[LocalizationKeys.JobsTriggeredSuccessfully])));
    }

    /// <summary>
    /// Trigger the Send Invoice Due Reminder job once immediately.
    /// Sends a payment reminder to customers whose invoices are due within the next <c>daysBefore</c> days.
    /// </summary>
    [HttpPost("send-invoice-due-reminder/trigger")]
    [ProducesResponseType(typeof(ApiResponse<TriggerJobResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TriggerJobResult>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TriggerJobResult>>> TriggerSendInvoiceDueReminder(
        [FromQuery] int? pageSize = null,
        [FromQuery] int? daysBefore = null,
        CancellationToken cancellationToken = default)
    {
        var triggered = await manualJobTrigger.TriggerSendInvoiceDueReminderAsync(pageSize, daysBefore, cancellationToken);
        if (!triggered)
            return NotFound(new ApiResponse<TriggerJobResult>(false, default, localizer[LocalizationKeys.JobsNotRegisteredOrNotAvailable]));
        return Ok(new ApiResponse<TriggerJobResult>(true, new TriggerJobResult(true, localizer[LocalizationKeys.JobsTriggeredSuccessfully])));
    }

    [HttpGet("connect-africa-eos/trigger")]
    [ProducesResponseType(typeof(ApiResponse<TriggerJobResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TriggerJobResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<TriggerJobResult>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TriggerJobResult>>> TriggerSyncGcoEnCoursFactures1(        
        CancellationToken cancellationToken)
    {
        var triggered = await manualJobTrigger.TriggerConnectAfricaEOS(DateTime.Now.AddHours(-5), DateTime.Now.AddHours(5), cancellationToken);
        if (!triggered)
            return NotFound(new ApiResponse<TriggerJobResult>(false, default, localizer[LocalizationKeys.JobsNotRegisteredOrNotAvailable]));
        return Ok(new ApiResponse<TriggerJobResult>(true, new TriggerJobResult(true, localizer[LocalizationKeys.JobsTriggeredSuccessfully])));
    }

    [HttpGet("sync-in-pending-payments-status/trigger")]
    [ProducesResponseType(typeof(ApiResponse<TriggerJobResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TriggerJobResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<TriggerJobResult>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TriggerJobResult>>> TriggerSyncInPendingPaymentsStatus(
        [FromQuery] int? pageSize = null,
        [FromQuery] int? minimumAgeMinutes = null,
        CancellationToken cancellationToken = default)
    {
        var triggered = await manualJobTrigger.TriggerSyncInPendingPaymentsStatusJobAsync(pageSize, minimumAgeMinutes, cancellationToken);
        if (!triggered)
            return NotFound(new ApiResponse<TriggerJobResult>(false, default, localizer[LocalizationKeys.JobsNotRegisteredOrNotAvailable]));
        return Ok(new ApiResponse<TriggerJobResult>(true, new TriggerJobResult(true, localizer[LocalizationKeys.JobsTriggeredSuccessfully])));
    }
}

public sealed record TriggerJobResult(bool Triggered, string Message);
