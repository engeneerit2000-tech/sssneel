using Elasticsearch.Net;
using Microsoft.AspNetCore.Mvc;
using SNEL.WalletPlatform.Application.Accounting.Interfaces;
using SNEL.WalletPlatform.Application.Accounting.Models.Output;
using SNEL.WalletPlatform.Application.BillingServices.Interfaces;
using SNEL.WalletPlatform.Application.BillingServices.Models.Output;
using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Payments.Interfaces;
using SNEL.WalletPlatform.Application.Payments.Models.Input;
using SNEL.WalletPlatform.Application.Payments.Models.Output;
using SNEL.WalletPlatform.Application.Payments.Services;
using SNEL.WalletPlatform.Application.Test;
using SNEL.WalletPlatform.Application.Test.Models;
using SNEL.WalletPlatform.Domain.Billing;
using SNEL.WalletPlatform.Infrastructure.Jobs;
using SNEL.WalletPlatform.Presentation.Common;

namespace SNEL.WalletPlatform.Presentation.Controllers.V1;

[Route("api/v1/test")]
[ApiController]
public class TestController(
    TestService testService,
    IBillingServiceService billingServiceService,
    IAccountingService accountingService,
    IInvoiceRetrieval   invoiceRetrieval,
    IPaymentService paymentService,
    ILogger<TestController> logger) : Controllers.ApiControllerBase
{

    [HttpPost("send-notification/{user_id}")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SendNotification(Guid user_id)
    {
        return Ok(await testService.SendNotification(user_id,true,null));
    }
    
    [HttpPost("send-notification-with-time/{user_id}")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SendNotificationWithTime(Guid user_id, bool? isTransactional  ,DateTime? sendingAt)
    {
        return Ok(await testService.SendNotification(user_id, isTransactional, sendingAt));
    }

    /// <summary>
    /// Test silent data push (PushMessageMode.Data). Resolves push tokens for user_id.
    /// </summary>
    [HttpPost("send-data-push/{user_id:guid}")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<bool>>> SendDataPushNotification(Guid user_id, [FromBody] SendDataPushTestInputModel model)
    {
        return FromResult(await testService.SendDataPushNotificationAsync(user_id, model));
    }

    /// <summary>
    /// Test visible push with navigation data (referenceType + referenceId).
    /// Persists to notification inbox when the notification service accepts the request.
    /// </summary>
    [HttpPost("send-navigable-notification")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<bool>>> SendNavigableNotification([FromBody] SendNavigableNotificationTestInputModel model)
    {
        return FromResult(await testService.SendNavigableNotificationAsync(model));
    }
    
    [HttpPost("test-localization")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TestLocalization()
    {
        return Ok(await testService.GetAccountTypesLocalized("fr"));
    }
    
    [HttpGet("{id:guid}/html-preview")]
    public async Task<ActionResult<ApiResponse<string>>> GetHtmlPreview(Guid id, CancellationToken cancellationToken)
    {
        var result = await testService.TestHtmlPreview(id, cancellationToken);

        //if (!result.IsSuccess)
        //    return StatusCode(result.StatusCode, new ApiResponse<string>(false, default, result.Error));

        //return Content(result.Value!, "text/html");
        return FromResult(result);
    }

    //TODO: Hasan change result to base64 instead of byte array 
    [HttpGet("{id:guid}/invoice-pdf")]
    public async Task<IActionResult> GetInvoicePdf(Guid id, CancellationToken cancellationToken)
    {
        var result = await testService.TestHtmlToPDF(id, cancellationToken);

        if (!result.IsSuccess)
            return StatusCode((int)result.StatusCode, new ApiResponse<byte[]>(false, default, result.Message));

        return File(result.Value!, "application/pdf", "invoice.pdf");
    }


    [HttpPost("test-export-excel")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<FileContentResult>>> TestExportExcel()
    {
        var response = await testService.ExportExcelFileAsync();

        //if (!response.IsSuccess)
        //    return  Problem(response.Error, response.Error, response.StatusCode);

        //return response.Value;
        return FromResult(response);

    }

    //[HttpPost("apply-payment/{paymentId:guid}")]
    //public async Task<ActionResult<ApiResponse<bool>>> ApplyPayment(Guid paymentId)
    //{
    //    await billingServiceService.ApplyPayment(paymentId: paymentId);
    //    return FromResult(Result<bool>.Success(true));
    //}
    
    // [HttpPost("journal/topup")]
    // public async Task<ActionResult<ApiResponse<bool>>> JournalTopup(CreateJournalForTopupInputModel model)
    // {
    //     var response = await accountingService.CreateJournalsForTopupAsync(model);
    //     return FromResult(response);
    // }
    
    [HttpPost("oracle/test-connection")]
    public async Task<ActionResult<ApiResponse<bool>>> OracleTestConnection()
    {
        var response = await invoiceRetrieval.TestConnectionAsync("06-03");
        return Ok(response);
    }
    
    [HttpGet("billing-services")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<BillingServiceOutputModel>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<BillingServiceOutputModel>>>> GetBillingServices(
        CancellationToken cancellationToken = default)
    {
        var acceptLanguage = Request.Headers.AcceptLanguage.ToString();
        var result = await billingServiceService.GetBillingServicesAsync(acceptLanguage, cancellationToken: cancellationToken);
        return FromResult(result);
    }

    [HttpPost("lock")]
    public async Task<ActionResult<ApiResponse<bool>>> TestLock([FromBody] TestLockRecord record)
    {
        var response = await paymentService.TestLock(record.name, TimeSpan.FromSeconds(record.seconds));
        return FromResult(response);
    }

    [HttpPost("topupRate")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<TopupRateOutputModel>>>> TestTopupRate([FromBody] TopupRateInputModel model)
    {
        var response = await paymentService.GetTopupRate(model);
        return FromResult(response);
    }

    [HttpPost("paymentRate")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PaymentRateOutputModel>>>> TestPaymentRate([FromBody] PaymentRateInputModel model)
    {
        var response = await paymentService.GetPaymentRate(model);
        return FromResult(response);
    }

    [HttpPost("test-transaction")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TestTransaction()
    {
        var response = await testService.TestTransaction();
        //if (!response.IsSuccess)
        //    return  Problem(response.Error, response.Error, response.StatusCode);

        //return response.Value;
        return Ok(response);

    }

    public record TestLockRecord(string name, int seconds);


}
