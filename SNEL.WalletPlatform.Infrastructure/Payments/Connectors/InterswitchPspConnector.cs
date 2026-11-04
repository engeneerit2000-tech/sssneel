using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using NPOI.POIFS.Properties;
using SNEL.WalletPlatform.Application;
using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Common.Localization;
using SNEL.WalletPlatform.Application.Interfaces.UnitOfWork;
using SNEL.WalletPlatform.Application.Payments.Interfaces;
using SNEL.WalletPlatform.Application.Payments;
using SNEL.WalletPlatform.Application.Payments.Models.External.Interswitch;
using SNEL.WalletPlatform.Application.Payments.Models.Input;
using SNEL.WalletPlatform.Application.Payments.Models.Internal;
using SNEL.WalletPlatform.Application.Payments.Models.Output;
using SNEL.WalletPlatform.Domain.Payments;
using SNEL.WalletPlatform.Infrastructure.Payments.Clients;
//using SNEL.WalletPlatform.Infrastructure.Payments.Interswitch;
using SNEL.WalletPlatform.Infrastructure.Payments.Models;
using System.Globalization;
using System.Net;
using System.Text.Json;

namespace SNEL.WalletPlatform.Infrastructure.Payments.Connectors;

public sealed class InterswitchPspConnector(
    IUnitOfWork unitOfWork,
    InterswitchHttpClient interswitchHttpClient,
    //InterswitchMobileMoneyPaymentInitializer mobileMoneyPaymentInitializer,
    IStringLocalizer<SharedResource> localizer,
    ILogger<InterswitchPspConnector> logger) : PspConnector(unitOfWork, logger, localizer)
{
    private static readonly string[] RequiredConfiguredFields =
    [
        "pay_item_id",
        "pay_item_name",
        "mode",
        "merchant_code",
        "site_redirect_url",
        "init_url",
        "client_side_base_url"
    ];

    public override string PaymentServiceProviderCode => "INTERSWITCH";
    public override bool CreateTransactions => true;

    protected override Task<Result<PaymentNextActionOutputModel>> HandleGetDataForPaymentInitiation(PaymentEntity payment)
    {
        if (string.IsNullOrWhiteSpace(payment.PaymentMethod?.Config))
        {
            throw new InvalidOperationException(
                $"Payment method '{payment.PaymentMethod?.Code}' has no configuration.");
        }

        using var document = JsonDocument.Parse(payment.PaymentMethod.Config);
        var root = document.RootElement;

        if (!root.TryGetProperty("fields", out var fieldsElement) || fieldsElement.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidOperationException("Inline config must include a 'fields' object.");
        }

        var configuredFields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var property in fieldsElement.EnumerateObject())
        {
            configuredFields[property.Name] = property.Value.GetString() ?? string.Empty;
        }

        ValidateRequiredConfiguredFields(configuredFields);

        var paymentInitFields = RequiredConfiguredFields.ToDictionary(
            fieldName => fieldName,
            fieldName => configuredFields[fieldName],
            StringComparer.OrdinalIgnoreCase);
        paymentInitFields["amount"] = ToMinorUnits(payment.Amount).ToString(CultureInfo.InvariantCulture);
        paymentInitFields["currency"] = payment.Currency.Code;
        paymentInitFields["txn_ref"] = payment.PaymentRef;

        var output = new PaymentNextActionOutputModel(
                PaymentNextAction: PaymentNextActionEnum.InitializeClientSideFromWeb,
                Data: new InitializeClientSideFromWebNextActionModel(
                    NextActionFields: paymentInitFields,
                    Message: localizer[LocalizationKeys.PaymentConfirmedAndWillCompletedExternally]));

        return Task.FromResult(Result<PaymentNextActionOutputModel>.Success(output));
    }

    //protected override async Task<Result<PaymentNextActionOutputModel>> HandleInitiatePayment(
    //    PaymentInitContextInternalModel context,
    //    CancellationToken cancellationToken = default)
    //{
    //    try
    //    {
    //        switch (context.PaymentMethod.Code.ToUpperInvariant())
    //        {
    //            case "MOBILE_MONEY":
    //            {
    //                var paymentMethod = context.PaymentMethod;

    //                var requestBody = new InterswitchMobileWalletInitializeRequestModel
    //                {
    //                    OrderId = context.Output.PaymentId.ToString("N"),
    //                    OrderCurrency = context.CurrencyAbbreviation.Trim().ToUpperInvariant(),
    //                    CustomerMobileNumber = context.CustomerMobileNumber,
    //                    MobileWalletProvider = DetectFromMobileNumber(context.CustomerMobileNumber),
    //                    PayItemId = GetRequiredField(paymentMethod, "pay_item_id"),
    //                    MerchantCode = GetMerchantCode(paymentMethod),
    //                    TxnRef = context.Output.TxnRef,
    //                    Currency = context.CurrencyIso4217.Trim(),
    //                    OrderAmount = ToMinorUnits(context.Amount).ToString(CultureInfo.InvariantCulture),
    //                    MerchantCountry = "CD"
    //                };
    //                var interswitchBaseUrl = GetInterswitchBaseUrl(context.PaymentMethod);
    //                var clientId = GetClientId(context.PaymentMethod);
    //                var secretKey = GetSecretKey(context.PaymentMethod);

    //                var accessToken = await interswitchHttpClient.GetAccessTokenAsync(
    //                    interswitchBaseUrl,
    //                    clientId,
    //                    secretKey,
    //                    cancellationToken).ConfigureAwait(false);
    //                interswitchHttpClient.SetBearerToken(accessToken);

    //                var output = await mobileMoneyPaymentInitializer.InitializeAsync(
    //                        context,
    //                        requestBody,
    //                        interswitchBaseUrl,
    //                        cancellationToken)
    //                    .ConfigureAwait(false);

    //                return Result<PaymentInitializeOutputModel>.Success(output);
    //            }
    //            default:
    //                return await HandleUnsupportedInitiatePaymentAsync(context).ConfigureAwait(false);
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        logger.LogError(ex, "Interswitch payment initiation failed.");
    //        return Result<PaymentInitializeOutputModel>.Failure(ex.Message, AppHttpStatusCodes.Failed);
    //    }
    //}

    private static Task<Result<PaymentInitializationOutputModel>> HandleUnsupportedInitiatePaymentAsync(         PaymentEntity payment)
    {
        return Task.FromResult(Result<PaymentInitializationOutputModel>.Failure(
            $"Interswitch server-side initiation is not implemented for payment method '{payment.PaymentMethod?.Code}'.",
            AppHttpStatusCodes.BadRequest));
    }

    protected override Task<Result<PaymentNextActionOutputModel?>> HandleGetNextStepInfo(
        PaymentEntity payment)
    {
        // TODO From biller config
        PaymentNextActionOutputModel? nextStepInfo = new PaymentNextActionOutputModel(
            PaymentNextAction: PaymentNextActionEnum.Confirm,
            Data: new OtpConfirmationNextActionModel(
                Message: "OTP confirmation is required.",
                OtpLength: 6));

        return Task.FromResult(Result<PaymentNextActionOutputModel?>.Success(nextStepInfo));
    }

    protected override Task<Result<PaymentConfirmationInternalModel>> HandleConfirmPayment(Guid paymentId)
    {
        throw new NotImplementedException();
    }

    protected override async Task<Result<PaymentCheckStatusInternalModel>> HandleGetTransactionStatus(
        PaymentTransactionStatusContextInternalModel context)
    {
        PaymentCheckStatusInternalModel result = new PaymentCheckStatusInternalModel();

        try
        {
            var merchantCode = GetMerchantCode(context.PaymentMethod);
            var interswitchBaseUrl = GetInterswitchBaseUrl(context.PaymentMethod);
            var clientId = GetClientId(context.PaymentMethod);
            var secretKey = GetSecretKey(context.PaymentMethod);
            var amount = ToMinorUnits(context.Amount);
            var requestPayload = new
            {
                merchantcode = merchantCode,
                transactionreference = context.PaymentTransactionReference,
                amount
            };
            var requestBody = JsonSerializer.Serialize(requestPayload);
            result.RequestBody = requestBody;

            logger.LogInformation(
                "Interswitch: GetAccessToken request. BaseUrl={BaseUrl}, ClientId={ClientId}",
                interswitchBaseUrl,
                clientId);
            var accessToken = await interswitchHttpClient.GetAccessTokenAsync(
                interswitchBaseUrl,
                clientId,
                secretKey);
            logger.LogInformation("Interswitch: GetAccessToken response received (token length={TokenLength}).", accessToken?.Length ?? 0);
            interswitchHttpClient.SetBearerToken(accessToken);

            logger.LogInformation(
                "Interswitch: GetTransactionStatus request. BaseUrl={BaseUrl}, MerchantCode={MerchantCode}, TransactionRef={TransactionRef}, Amount={Amount}, Body={RequestBody}",
                interswitchBaseUrl,
                merchantCode,
                context.PaymentTransactionReference,
                amount,
                requestBody);
            LogTransactionStatusRequest(context.PaymentId, requestBody);
            var response = await interswitchHttpClient.GetTransactionStatusAsync(
                interswitchBaseUrl,
                merchantCode,
                context.PaymentTransactionReference,
                amount);
            var responseBody = await response.Content.ReadAsStringAsync();
            result.ResponseBody = responseBody;
            result.ResponseCode = ((int)response.StatusCode).ToString();
            LogTransactionStatusResponse(context.PaymentId, result.ResponseCode, responseBody);

            if (response.StatusCode == HttpStatusCode.NotFound &&
                !string.IsNullOrWhiteSpace(responseBody))
            {
                result.ResponseMessage = responseBody.Trim();
                result.PaymentExternalRef = context.PaymentExternalReference ?? string.Empty;

                if (PspTransactionStatusNotFoundPolicy.ShouldMarkFailed(context))
                {
                    logger.LogWarning(
                        "Interswitch transaction status returned 404 with response body; treating payment as failed. PaymentId={PaymentId}, TransactionReference={TransactionReference}, Attempt={Attempt}/{TotalAttempts}, Body={ResponseBody}",
                        context.PaymentId,
                        context.PaymentTransactionReference,
                        context.StatusPollingAttempt,
                        context.StatusPollingTotalAttempts,
                        responseBody);

                    result.Status = PaymentStatus.Failed;
                }
                else
                {
                    logger.LogWarning(
                        "Interswitch transaction status returned 404 with response body; deferring failure (transaction may not be indexed yet). PaymentId={PaymentId}, TransactionReference={TransactionReference}, Attempt={Attempt}/{TotalAttempts}, FailAfterAttempts={FailAfterAttempts}, Body={ResponseBody}",
                        context.PaymentId,
                        context.PaymentTransactionReference,
                        context.StatusPollingAttempt,
                        context.StatusPollingTotalAttempts,
                        context.NotFoundFailAfterAttempts,
                        responseBody);

                    result.Status = PaymentStatus.Processing;
                }

                return Result<PaymentCheckStatusInternalModel>.Success(result);
            }

            if (response.StatusCode != HttpStatusCode.OK)
            {
                logger.LogWarning(
                    "Interswitch transaction status request failed with status code {StatusCode}. Body: {ResponseBody}",
                    response.StatusCode,
                    responseBody);

                return Result<PaymentCheckStatusInternalModel>.Failure(
                    $"Failed to get transaction status from Interswitch. HTTP {(int)response.StatusCode}.",
                    AppHttpStatusCodes.Failed, result);
            }

            logger.LogInformation(
                "Interswitch transaction status response received for transaction reference {TransactionReference}.",
                context.PaymentTransactionReference);
            var transactionStatusResponse = JsonSerializer.Deserialize<InterswitchTransactionStatusResponseModel>(responseBody);
            if (transactionStatusResponse is null)
            {
                return Result<PaymentCheckStatusInternalModel>.Failure(
                    "Failed to parse transaction status response from Interswitch.",
                    AppHttpStatusCodes.Failed,result);
            }

            result.ResponseMessage = transactionStatusResponse.ResponseDescription;
            var paymentStatus = MapPaymentStatus(transactionStatusResponse);
            result.Status = paymentStatus;
            result.PaymentExternalRef = JsonSerializer.Serialize(new
            {
                transactionStatusResponse.RetrievalReferenceNumber,
                transactionStatusResponse.PaymentReference,
                transactionStatusResponse.TerminalId,
                transactionStatusResponse.PaymentId
            });

            return Result<PaymentCheckStatusInternalModel>.Success(result);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "An error occurred while checking transaction status with Interswitch for transaction reference {TransactionReference}.",
                context.PaymentTransactionReference);
            return Result<PaymentCheckStatusInternalModel>.Failure(
                "An error occurred while checking transaction status with Interswitch.",
                AppHttpStatusCodes.Failed,result);

        }
    }

    private static void ValidateRequiredConfiguredFields(IReadOnlyDictionary<string, string> fields)
    {
        foreach (var key in RequiredConfiguredFields)
        {
            if (!fields.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException($"Inline config must provide a non-empty '{key}' field.");
            }
        }

        var mode = fields["mode"].Trim().ToUpperInvariant();
        if (mode is not ("TEST" or "LIVE"))
        {
            throw new InvalidOperationException("Inline field 'mode' must be either TEST or LIVE.");
        }
    }

    private static string GetMerchantCode(PaymentMethodEntity paymentMethod)
    {
        return GetRequiredField(paymentMethod, "merchant_code");
    }

    private static string GetInterswitchBaseUrl(PaymentMethodEntity paymentMethod)
    {
        var interswitchBaseUrl = GetRequiredField(paymentMethod, "interswitch_base_url");
        if (string.IsNullOrWhiteSpace(interswitchBaseUrl))
        {
            throw new InvalidOperationException("Inline config must provide a non-empty 'interswitch_base_url' field.");
        }

        if (!Uri.TryCreate(interswitchBaseUrl, UriKind.Absolute, out _))
        {
            throw new InvalidOperationException("Inline config field 'interswitch_base_url' must be a valid absolute URL.");
        }

        return interswitchBaseUrl;
    }

    private static string GetClientId(PaymentMethodEntity paymentMethod)
    {
        return GetRequiredField(paymentMethod, "client_id");
    }

    private static string GetSecretKey(PaymentMethodEntity paymentMethod)
    {
        return GetRequiredField(paymentMethod, "secret_key");
    }

    private static string GetRequiredField(PaymentMethodEntity paymentMethod, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(paymentMethod.Config))
        {
            throw new InvalidOperationException(
                $"Payment method '{paymentMethod.Code}' has no configuration.");
        }

        using var document = JsonDocument.Parse(paymentMethod.Config);
        var root = document.RootElement;

        if (!root.TryGetProperty("fields", out var fieldsElement) || fieldsElement.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidOperationException("Inline config must include a 'fields' object.");
        }

        if (!fieldsElement.TryGetProperty(fieldName, out var fieldElement))
        {
            throw new InvalidOperationException($"Inline config must include a '{fieldName}' field.");
        }

        var fieldValue = fieldElement.GetString();
        if (string.IsNullOrWhiteSpace(fieldValue))
        {
            throw new InvalidOperationException($"Inline config must provide a non-empty '{fieldName}' field.");
        }

        return fieldValue;
    }

    private static PaymentStatus MapPaymentStatus(InterswitchTransactionStatusResponseModel transactionStatusResponse)
    {
        var responseCode = transactionStatusResponse.ResponseCode.Trim().ToUpperInvariant();

        return responseCode switch
        {
            "10" or "11" or "00" => PaymentStatus.Completed,
            "09" or "Z25" => PaymentStatus.Pending,
            "Z0" or "Z61" or "Z62" or "Z67"=> PaymentStatus.Processing,
            "Z6" or "17" => PaymentStatus.Cancelled,
            _ => PaymentStatus.Processing
        };
    }

    private static int ToMinorUnits(decimal amount)
    {
        // should be config on payment method currency (depends on currency and provider documentation)
        return decimal.ToInt32(decimal.Round(amount * 100m, 0, MidpointRounding.AwayFromZero));
    }

    /// <summary>
    /// DRC (243) operator prefix → Interswitch <c>mobile_wallet_provider</c> (international 243+… or 9-digit national).
    /// </summary>
    private static string DetectFromMobileNumber(string mobileNumber)
    {
        if (string.IsNullOrWhiteSpace(mobileNumber))
        {
            throw new InvalidOperationException("Mobile number is required to determine the mobile wallet provider.");
        }

        var digits = new string(mobileNumber.Where(char.IsDigit).ToArray());
        if (digits.Length == 0)
        {
            throw new InvalidOperationException("Mobile number must contain digits.");
        }

        string prefix2;
        if (digits.Length >= 12 && digits.StartsWith("243", StringComparison.Ordinal))
        {
            prefix2 = digits.Substring(3, 2);
        }
        else if (digits.Length == 9)
        {
            prefix2 = digits.Substring(0, 2);
        }
        else
        {
            throw new InvalidOperationException(
                "Mobile number must be 9 digits (national) or 12+ digits starting with country code 243.");
        }

        return prefix2 switch
        {
            "90" or "91" => "AFRIMONEY",
            "96" or "97" or "98" or "99" => "AIRTELMONEY",
            "81" or "82" => "MPESA",
            "80" or "84" or "85" or "89" => "ORANGE",
            _ => throw new InvalidOperationException(
                $"No mobile wallet provider is configured for operator prefix '{prefix2}' (country 243).")
        };
    }
}
