using Elasticsearch.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SNEL.WalletPlatform.Application.Payments;
using SNEL.WalletPlatform.Application.Payments.Configurations;
using SNEL.WalletPlatform.Application.Payments.Interfaces;
using SNEL.WalletPlatform.Application.Webhooks.Interfaces;
using SNEL.WalletPlatform.Application.Webhooks.Models.Input;
using SNEL.WalletPlatform.Presentation.Common;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SNEL.WalletPlatform.Presentation.Controllers.V1;

[AllowAnonymous]
[Route("api/v1/webhooks")]
public sealed class WebhookController(
    IWebhookEventService transactionEventService,
    IOptions<EasypayOptions> easypayOptions,
    IOptions<OrangeOptions> orangeOptions,
    IPaymentMethodService paymentMethodService,
    ILogger<WebhookController> logger) : ApiControllerBase
{
    private static readonly JsonSerializerOptions WebhookLogJsonOptions = new() { WriteIndented = false };

    [HttpPost("interswitch")]
    public async Task<IActionResult> CreateInterswitchWebhookEvent(CancellationToken token)
    {
        var signature = Request.Headers["X-Interswitch-Signature"].ToString();

        string rawBody;
        using (var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: false))
        {
            rawBody = await reader.ReadToEndAsync(token);
        }

        logger.LogInformation("INTERSWITCH WEBHOOK REQUEST: {Body}", rawBody);

        await transactionEventService.CreateInterswitchWebhookEventAsync(rawBody, signature);

        logger.LogInformation("INTERSWITCH WEBHOOK RESPONSE: HTTP 200");
        return Ok();
    }

    [HttpPost("akibapay")]
    public async Task<IActionResult> CreateAkibaPayWebhookEvent(CancellationToken token)
    {
        string rawBody;
        using (var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: false))
        {
            rawBody = await reader.ReadToEndAsync(token);
        }

        var requestLog = new
        {
            Method = Request.Method,
            Path = Request.Path.Value,
            Query = Request.QueryString.Value,
            ContentType = Request.ContentType,
            ContentLength = Request.ContentLength,
            RemoteIp = HttpContext.Connection.RemoteIpAddress?.ToString(),
            Body = rawBody
        };

        logger.LogInformation(
            "AKIBAPAY WEBHOOK REQUEST: {Request}",
            JsonSerializer.Serialize(requestLog, WebhookLogJsonOptions));

        var handleResult = await transactionEventService.CreateAkibaPayWebhookEventAsync(rawBody);

        var responseLog = new
        {
            HttpStatus = 200,
            HandleResult = handleResult
        };

        logger.LogInformation(
            "AKIBAPAY WEBHOOK RESPONSE: {Response}",
            JsonSerializer.Serialize(responseLog, WebhookLogJsonOptions));

        return Ok();
    }

    [HttpPost("easypay")]
    [Consumes("application/json")]
    public async Task<IActionResult> CreateEasypayWebhookEvent(
        [FromBody] JsonElement input,
        CancellationToken cancellationToken)
    {
        var rawBody = input.GetRawText();
        var requestLog = new
        {
            Method = Request.Method,
            Path = Request.Path.Value,
            Query = Request.QueryString.Value,
            ContentType = Request.ContentType,
            ContentLength = Request.ContentLength,
            RemoteIp = HttpContext.Connection.RemoteIpAddress?.ToString(),
            Body = rawBody
        };

        logger.LogInformation(
            "EASYPAY WEBHOOK REQUEST: {Request}",
            JsonSerializer.Serialize(requestLog, WebhookLogJsonOptions));

        EasypayWebhookEventInputModel? webhookInput;
        try
        {
            webhookInput = JsonSerializer.Deserialize<EasypayWebhookEventInputModel>(
                rawBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (JsonException exception)
        {
            logger.LogWarning(exception, "EASYPAY WEBHOOK PARSE_FAILED: {Body}", rawBody);
            return BadRequest(new { Success = 0, Errormsg = "Invalid JSON payload." });
        }

        if (webhookInput is null)
        {
            return BadRequest(new { Success = 0, Errormsg = "Invalid JSON payload." });
        }

        var isCardEvent = string.Equals(webhookInput.Provider?.Trim(), "Card", StringComparison.OrdinalIgnoreCase);
        var paymentMethodCode = isCardEvent ? "CARD_COLLECTIONS" : "COLLECTIONS";
        var paymentMethodResult = await paymentMethodService.GetByCodeAsync(paymentMethodCode, cancellationToken);
        if (!paymentMethodResult.IsSuccess || paymentMethodResult.Value?.Config is null)
        {
            logger.LogError(
                "EASYPAY WEBHOOK CONFIG_NOT_FOUND: PaymentMethodCode={PaymentMethodCode}.",
                paymentMethodCode);
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new { Success = 0, Errormsg = "EasyPay callback authentication is not configured." });
        }

        var credentials = PaymentMethodConfigReader.ResolveEasypayManagementCredentials(
            paymentMethodResult.Value.Config.Value.GetRawText(),
            isCardEvent ? null : easypayOptions.Value);
        if (!IsValidEasypayBasicAuth(
                Request.Headers.Authorization.ToString(),
                credentials.webhookAuth))
        {
            logger.LogWarning(
                "EASYPAY WEBHOOK UNAUTHORIZED: PaymentMethodCode={PaymentMethodCode}, Provider={Provider}.",
                paymentMethodCode,
                webhookInput.Provider);
            return Unauthorized(new { Success = 0, Errormsg = "Unauthorized" });
        }

        var handleResult = await transactionEventService.CreateEasypayWebhookEventAsync(webhookInput);

        var responseLog = new
        {
            HttpStatus = 200,
            HandleResult = handleResult
        };

        logger.LogInformation(
            "EASYPAY WEBHOOK RESPONSE: {Response}",
            JsonSerializer.Serialize(responseLog, WebhookLogJsonOptions));

        return Ok(new { Success = 1, Errormsg = "" });
    }

    private static bool IsValidEasypayBasicAuth(string providedAuthorization, string configuredAuthorization)
    {
        var provided = NormalizeBasic(providedAuthorization);
        var configured = NormalizeBasic(configuredAuthorization);
        if (provided.Length == 0 || configured.Length == 0)
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(provided),
            Encoding.UTF8.GetBytes(configured));
    }

    [HttpPost("afrimony")]
    [Consumes("application/json")]
    public async Task<IActionResult> CreateAfrimonyWebhookEvent(
        [FromBody] AfrimonyWebhookEventInputModel input)
    {
        var rawBody = JsonSerializer.Serialize(input, WebhookLogJsonOptions);

        var requestLog = new
        {
            Method = Request.Method,
            Path = Request.Path.Value,
            Query = Request.QueryString.Value,
            ContentType = Request.ContentType,
            ContentLength = Request.ContentLength,
            RemoteIp = HttpContext.Connection.RemoteIpAddress?.ToString(),
            Body = rawBody
        };

        logger.LogInformation(
            "AFRIMONY WEBHOOK REQUEST: {Request}",
            JsonSerializer.Serialize(requestLog, WebhookLogJsonOptions));

        var handleResult = await transactionEventService.CreateAfrimonyWebhookEventAsync(rawBody);

        var responseBody = new { status = "success" };
        var responseLog = new
        {
            HttpStatus = 200,
            Body = responseBody,
            HandleResult = handleResult
        };

        logger.LogInformation(
            "AFRIMONY WEBHOOK RESPONSE: {Response}",
            JsonSerializer.Serialize(responseLog, WebhookLogJsonOptions));

        return Ok(responseBody);
    }

    class OrangeError
    {
        public string error { get; set; } = "Unauthorized";
    }
    

    [HttpPost("mpesa")]
    [Consumes("application/json")]
    public async Task<IActionResult> CreateMpesaWebhookEvent([FromBody] JsonElement input)
    {
        var rawBody = input.GetRawText();
        logger.LogInformation("MPESA_WEBHOOK_REQUEST: {Body}", rawBody);
        var result = await transactionEventService.CreateMpesaWebhookEventAsync(rawBody);
        logger.LogInformation(
            "MPESA_WEBHOOK_RESPONSE: {Response}",
            JsonSerializer.Serialize(result, WebhookLogJsonOptions));
        return Ok();
    }
    
    
    [HttpPost("orange/notifications")]
    [Consumes("application/json")]
    public async Task<IActionResult> CreateOrangeWebhookEvent([FromBody] JsonElement input)
    {
        var rawBody = input.GetRawText();
        logger.LogInformation(
            "ORANGE_WEBHOOK_REQUEST: {Body}",
            rawBody);

        var configuredAuth = orangeOptions.Value.WebhookAuth?.Trim();
        var paymentMethodResult = await paymentMethodService.GetByCodeAsync("CASHOUT");
        if (paymentMethodResult.IsSuccess && paymentMethodResult.Value is not null)
        {
            configuredAuth = PaymentMethodConfigReader.ResolveOrangeCredentials(
                paymentMethodResult.Value.Config?.ToString(),
                orangeOptions.Value).WebhookAuth;
        }
        if (!string.IsNullOrWhiteSpace(configuredAuth) &&
            !string.Equals(NormalizeBasic(Request.Headers.Authorization.ToString()), NormalizeBasic(configuredAuth), StringComparison.Ordinal))
        {
            return Unauthorized(new OrangeError());
        }

        var result = await transactionEventService.CreateOrangeWebhookEventAsync(rawBody);
        logger.LogInformation("ORANGE_WEBHOOK_RESPONSE: {Response}", JsonSerializer.Serialize(result, WebhookLogJsonOptions));
        return Ok(new { status = "OK", errorMsg = "" });
    }

    private static string NormalizeBasic(string value) =>
        value.Trim().StartsWith("Basic ", StringComparison.OrdinalIgnoreCase)
            ? value.Trim()["Basic ".Length..].Trim()
            : value.Trim();
    [HttpPost("orange/orangeMoneyProvTest")]
    public async Task<IActionResult> CreateOrangeTestWebhookEvent([FromBody] JsonElement? input)
    {
        logger.LogInformation(
            "ORANGE_TEST_WEBHOOK_REQUEST: {Body}",
            JsonSerializer.Serialize(input, WebhookLogJsonOptions));

        //var paymentMethodResult = await paymentMethodService.GetByCodeAsync("COLLECTIONS");
        //if (!paymentMethodResult.IsSuccess)
        //{
        //    throw new Exception("Server Error (Not Configured)");
        //}
        //var managementCredentials = PaymentMethodConfigReader.ResolveEasypayManagementCredentials(
        //        paymentMethodResult.Value.Config.ToString(),
        //        easypayOptions.Value);
        //var authHeader = Request.Headers.Authorization.ToString();

        ////if (!IsValidBasicAuth(authHeader))
        ////    return Unauthorized();
        //if (string.IsNullOrWhiteSpace(authHeader) || authHeader.Split(' ').Length != 2 || authHeader.Split(' ')[1] != managementCredentials.webhookAuth)
        //{
        //    return Unauthorized();
        //}
        if (string.IsNullOrEmpty(Request.Headers.Authorization.ToString()))
        {
            return Unauthorized(new OrangeError());
        }

        var auth_base64 = Request.Headers.Authorization.ToString().TrimStart("Basic ".ToCharArray());
        if (auth_base64 != "b3JhbmdlOlBAc3N3MHJk")
        {
            return Unauthorized(new OrangeError());
        }
        return Ok(new { status = "OK", Errormsg = "" });
    }

    [HttpPost("orange/mandates/activation/notifications")]
    public async Task<IActionResult> CreateOrangeTest2WebhookEvent([FromBody] JsonElement? input)
    {
        logger.LogInformation(
            "ORANGE_TEST2_WEBHOOK_REQUEST: {Body}",
            JsonSerializer.Serialize(input, WebhookLogJsonOptions));

        //var paymentMethodResult = await paymentMethodService.GetByCodeAsync("COLLECTIONS");
        //if (!paymentMethodResult.IsSuccess)
        //{
        //    throw new Exception("Server Error (Not Configured)");
        //}
        //var managementCredentials = PaymentMethodConfigReader.ResolveEasypayManagementCredentials(
        //        paymentMethodResult.Value.Config.ToString(),
        //        easypayOptions.Value);
        //var authHeader = Request.Headers.Authorization.ToString();

        ////if (!IsValidBasicAuth(authHeader))
        ////    return Unauthorized();
        //if (string.IsNullOrWhiteSpace(authHeader) || authHeader.Split(' ').Length != 2 || authHeader.Split(' ')[1] != managementCredentials.webhookAuth)
        //{
        //    return Unauthorized();
        //}
        if (string.IsNullOrEmpty(Request.Headers.Authorization.ToString()))
        {
            return Unauthorized(new OrangeError());
        }

        var auth_base64 = Request.Headers.Authorization.ToString().TrimStart("Basic ".ToCharArray());
        if (auth_base64 != "b3JhbmdlOlBAc3N3MHJk")
        {
            return Unauthorized(new OrangeError());
        }
        return Ok(new { status = "OK", Errormsg = "" });
    }
}
