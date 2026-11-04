using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SNEL.WalletPlatform.Application;
using SNEL.WalletPlatform.Application.Caching;
using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Common.Localization;
using SNEL.WalletPlatform.Application.Interfaces.UnitOfWork;
using SNEL.WalletPlatform.Application.Payments;
using SNEL.WalletPlatform.Application.Payments.Configurations;
using SNEL.WalletPlatform.Application.Payments.Interfaces;
using SNEL.WalletPlatform.Application.Payments.Models.External.Easypay;
using SNEL.WalletPlatform.Application.Payments.Models.Internal;
using SNEL.WalletPlatform.Application.Payments.Models.Output;
using SNEL.WalletPlatform.Domain.Lookups;
using SNEL.WalletPlatform.Domain.Payments;
using SNEL.WalletPlatform.Infrastructure.Payments;
using SNEL.WalletPlatform.Infrastructure.Payments.Clients;
using System.Globalization;
using System.Net;
using System.Text.Json;

namespace SNEL.WalletPlatform.Infrastructure.Payments.Connectors;

public sealed class EasypayPspConnector(
    IUnitOfWork unitOfWork,
    EasypayHttpClient easypayHttpClient,
    IEasypayManagementTokenProvider easypayManagementTokenProvider,
    IEasypayPaymentInitContext easypayPaymentInitContext,
    IOptions<EasypayOptions> easypayOptions,
    IStringLocalizer<SharedResource> localizer,
    ILogger<EasypayPspConnector> logger) : PspConnector(unitOfWork, logger, localizer)
{
    private const string MobileMoneyCollectionsMethodCode = "COLLECTIONS";
    private const string CardCollectionsMethodCode = "CARD_COLLECTIONS";

    private static readonly HashSet<string> SupportedCardCurrencyCodes =
        new(StringComparer.Ordinal) { "CDF", "USD" };

    private static readonly string[] RequiredConfiguredFields =
    [
        "easypay_base_url",
        "api_key"
    ];

    private static readonly JsonSerializerOptions LogJsonOptions = new()
    {
        WriteIndented = false,
        PropertyNameCaseInsensitive = true
    };

    public override string PaymentServiceProviderCode => "EASYPAY";
    public override bool CreateTransactions => true;

    protected override async Task<Result<PaymentInitializationInternalModel>> HandleInitiatePayment(
        PaymentEntity payment,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var paymentContext = await unitOfWork.PaymentRepository.GetByIdWithPspContextAsync(payment.Id);
            if (paymentContext?.PaymentMethod is null)
            {
                return Result<PaymentInitializationInternalModel>.Failure(
                    localizer[LocalizationKeys.PaymentValidationPspSyncNoMethod, payment.Id],
                    AppHttpStatusCodes.NotFound);
            }

            switch (paymentContext.PaymentMethod.Code.ToUpperInvariant())
            {
                case MobileMoneyCollectionsMethodCode:
                    return await InitiateCustomerDepositAsync(paymentContext, cancellationToken);
                case CardCollectionsMethodCode:
                    return await InitiateCardCollectionAsync(paymentContext, cancellationToken);
                default:
                    return Result<PaymentInitializationInternalModel>.Failure(
                        localizer[LocalizationKeys.PaymentEasypayInitiateMethodNotImplemented, paymentContext.PaymentMethod.Code],
                        AppHttpStatusCodes.BadRequest);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Easypay payment initiation failed for payment {PaymentId}.", payment.Id);
            return Result<PaymentInitializationInternalModel>.Failure(ex.Message, AppHttpStatusCodes.Failed);
        }
    }

    private async Task<Result<PaymentInitializationInternalModel>> InitiateCardCollectionAsync(
        PaymentEntity payment,
        CancellationToken cancellationToken)
    {
        var configuredFields = PaymentMethodConfigReader.GetFields(payment.PaymentMethod!.Config);
        if (configuredFields.Count == 0)
        {
            throw new InvalidOperationException(
                localizer[LocalizationKeys.PaymentEasypayConfigMissingOnMethod, payment.PaymentMethod.Code].Value);
        }

        ValidateRequiredConfiguredFields(configuredFields);
        var baseUrl = configuredFields["easypay_base_url"];
        var apiKey = configuredFields["api_key"];

        var currency = CachingFactory.GetValue<CurrencyEntity>()
            .FirstOrDefault(x => x.Id == payment.CurrencyId);
        if (currency is null)
        {
            return Result<PaymentInitializationInternalModel>.Failure(
                localizer[LocalizationKeys.CommonKeys.CurrencyNotFound, payment.CurrencyId],
                AppHttpStatusCodes.NotFound);
        }

        var currencyCode = currency.Abbreviation.Trim().ToUpperInvariant();
        if (!SupportedCardCurrencyCodes.Contains(currencyCode))
        {
            return Result<PaymentInitializationInternalModel>.Failure(
                localizer[LocalizationKeys.PaymentEasypayCardCurrencyUnsupported],
                AppHttpStatusCodes.BadRequest);
        }

        var methodCurrency = payment.PaymentMethod.PaymentMethodCurrencies
            .FirstOrDefault(x => x.CurrencyId == payment.CurrencyId && x.Active);
        if (methodCurrency is null)
        {
            return Result<PaymentInitializationInternalModel>.Failure(
                localizer[LocalizationKeys.PaymentMethodCurrencyNotConfigured, payment.PaymentMethod.Name],
                AppHttpStatusCodes.BadRequest);
        }

        if (!methodCurrency.CheckLimits(payment.Amount))
        {
            return Result<PaymentInitializationInternalModel>.Failure(
                localizer[
                    LocalizationKeys.PaymentAmountOutsideMethodLimits,
                    payment.Amount,
                    currencyCode,
                    payment.PaymentMethod.Name],
                AppHttpStatusCodes.BadRequest);
        }

        if (decimal.Round(payment.Amount, methodCurrency.Scale) != payment.Amount)
        {
            return Result<PaymentInitializationInternalModel>.Failure(
                localizer[LocalizationKeys.PaymentEasypayCardAmountInvalid, methodCurrency.Scale],
                AppHttpStatusCodes.BadRequest);
        }

        if (string.IsNullOrWhiteSpace(payment.PaymentRef))
        {
            return Result<PaymentInitializationInternalModel>.Failure(
                localizer[LocalizationKeys.PaymentEasypayPaymentRefRequired],
                AppHttpStatusCodes.BadRequest);
        }

        var email = NullIfWhiteSpace(easypayPaymentInitContext.Email);
        if (easypayPaymentInitContext.SendPaymentLinkByEmail && email is null)
        {
            return Result<PaymentInitializationInternalModel>.Failure(
                localizer[LocalizationKeys.PaymentEasypayCardEmailRequired],
                AppHttpStatusCodes.BadRequest);
        }

        var returnUrl = NullIfWhiteSpace(easypayPaymentInitContext.ReturnUrl);
        if (returnUrl is not null && !IsPublicHttpsUrl(returnUrl))
        {
            return Result<PaymentInitializationInternalModel>.Failure(
                localizer[LocalizationKeys.PaymentEasypayCardReturnUrlInvalid],
                AppHttpStatusCodes.BadRequest);
        }

        var request = new EasypayCardCollectionRequestModel
        {
            ReferenceId = payment.PaymentRef.Trim(),
            Amount = payment.Amount,
            Currency = currencyCode,
            Email = email,
            Description = NullIfWhiteSpace(easypayPaymentInitContext.Description),
            Notify = easypayPaymentInitContext.SendPaymentLinkByEmail ? "email" : null,
            ReturnUrl = returnUrl
        };

        var requestBodyForLog = SerializeRequestForLog(request);
        logger.LogInformation(
            "Easypay card collection initiate request. PaymentId={PaymentId}, Url={Url}, Body={RequestBody}",
            payment.Id,
            baseUrl,
            requestBodyForLog);

        var responseStatus = HttpStatusCode.InternalServerError;
        string? responseReason = null;
        var responseBody = string.Empty;
        EasypayCollectionResponseModel? responseModel = null;

        for (var attempt = 0; attempt < 2; attempt++)
        {
            var tokenResult = await easypayManagementTokenProvider
                .GetTokenAsync(baseUrl, apiKey, cancellationToken)
                .ConfigureAwait(false);
            if (!tokenResult.IsSuccess || string.IsNullOrWhiteSpace(tokenResult.Value))
            {
                return Result<PaymentInitializationInternalModel>.Failure(
                    tokenResult.Message ?? localizer[LocalizationKeys.PaymentEasypayManagementLoginHttpFailed],
                    tokenResult.StatusCode);
            }

            using var response = await easypayHttpClient
                .PostCardCollectionAsync(baseUrl, request, tokenResult.Value, cancellationToken)
                .ConfigureAwait(false);
            responseBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            responseStatus = response.StatusCode;
            responseReason = response.ReasonPhrase;

            try
            {
                responseModel = JsonSerializer.Deserialize<EasypayCollectionResponseModel>(
                    responseBody,
                    LogJsonOptions);
            }
            catch (JsonException)
            {
                responseModel = null;
            }

            var tokenExpired = response.StatusCode == HttpStatusCode.Unauthorized ||
                               responseModel?.ErrorCode == 3009;
            if (tokenExpired && attempt == 0)
            {
                logger.LogInformation(
                    "Easypay card collection token was rejected; refreshing once. PaymentId={PaymentId}.",
                    payment.Id);
                easypayManagementTokenProvider.InvalidateToken(baseUrl, apiKey);
                continue;
            }

            break;
        }

        var statusCode = ((int)responseStatus).ToString(CultureInfo.InvariantCulture);
        logger.LogInformation(
            "Easypay card collection initiate response. PaymentId={PaymentId}, StatusCode={StatusCode}, Body={ResponseBody}",
            payment.Id,
            statusCode,
            responseBody);

        await PersistInitiateTransactionAuditAsync(
            payment.Id,
            requestBodyForLog,
            responseBody,
            statusCode,
            responseReason,
            cancellationToken).ConfigureAwait(false);

        if (responseStatus is not (HttpStatusCode.OK or HttpStatusCode.Created or HttpStatusCode.Accepted) ||
            responseModel?.Success != 1)
        {
            logger.LogWarning(
                "Easypay card collection initiate failed. PaymentId={PaymentId}, StatusCode={StatusCode}, ErrorCode={ErrorCode}, Body={ResponseBody}",
                payment.Id,
                statusCode,
                responseModel?.ErrorCode,
                responseBody);

            return Result<PaymentInitializationInternalModel>.Failure(
                localizer[LocalizationKeys.PaymentEasypayInitiateHttpFailed, statusCode, responseBody],
                AppHttpStatusCodes.Failed);
        }

        var responseData = responseModel.Data;
        if (responseData is null ||
            string.IsNullOrWhiteSpace(responseData.PaymentUrl) ||
            !Uri.TryCreate(responseData.PaymentUrl, UriKind.Absolute, out var paymentUri) ||
            paymentUri.Scheme != Uri.UriSchemeHttps)
        {
            return Result<PaymentInitializationInternalModel>.Failure(
                localizer[LocalizationKeys.PaymentEasypayStatusParseFailed],
                AppHttpStatusCodes.Failed);
        }

        var nextActionFields = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["paymentUrl"] = responseData.PaymentUrl,
            ["referenceId"] = responseData.ReferenceId ?? request.ReferenceId
        };
        if (!string.IsNullOrWhiteSpace(responseData.TransactionId))
        {
            nextActionFields["transactionId"] = responseData.TransactionId;
        }
        if (responseData.ExpiresAt.HasValue)
        {
            nextActionFields["expiresAt"] = responseData.ExpiresAt.Value.ToString("O", CultureInfo.InvariantCulture);
        }

        return Result<PaymentInitializationInternalModel>.Success(
            new PaymentInitializationInternalModel(
                PaymentNextActionEnum.InitializeClientSideFromWeb,
                new InitializeClientSideFromWebNextActionModel(
                    nextActionFields,
                    localizer[LocalizationKeys.PaymentPspInitClientPrepared])));
    }

    private async Task<Result<PaymentInitializationInternalModel>> InitiateCustomerDepositAsync(
        PaymentEntity payment,
        CancellationToken cancellationToken)
    {
        var configuredFields = PaymentMethodConfigReader.GetFields(payment.PaymentMethod!.Config);
        if (configuredFields.Count == 0)
        {
            throw new InvalidOperationException(
                localizer[LocalizationKeys.PaymentEasypayConfigMissingOnMethod, payment.PaymentMethod!.Code].Value);
        }
        ValidateRequiredConfiguredFields(configuredFields);

        var baseUrl = configuredFields["easypay_base_url"];
        var apiKey = configuredFields["api_key"];        
        //var callbackUrl = configuredFields["callback_url"];

        var rawFromNumber = easypayPaymentInitContext.FromNumber;
        if (string.IsNullOrWhiteSpace(rawFromNumber))
        {
            return Result<PaymentInitializationInternalModel>.Failure(
                localizer[LocalizationKeys.PaymentEasypayFromNumberRequired],
                AppHttpStatusCodes.BadRequest);
        }

        var fromNumber = AkibaPaySignatureHelper.NormalizeFromNumber(rawFromNumber);
        if (fromNumber.Length == 0)
        {
            return Result<PaymentInitializationInternalModel>.Failure(
                localizer[LocalizationKeys.PaymentEasypayFromNumberInvalid],
                AppHttpStatusCodes.BadRequest);
        }

        var allCurrencies = CachingFactory.GetValue<CurrencyEntity>();
        var currency = allCurrencies.FirstOrDefault(x => x.Id == payment.CurrencyId);
        if (currency is null)
        {
            return Result<PaymentInitializationInternalModel>.Failure(
                localizer[LocalizationKeys.CommonKeys.CurrencyNotFound, payment.CurrencyId],
                AppHttpStatusCodes.NotFound);
        }

        var currencyCode = currency.Abbreviation.Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(payment.PaymentRef))
        {
            return Result<PaymentInitializationInternalModel>.Failure(
                localizer[LocalizationKeys.PaymentEasypayPaymentRefRequired],
                AppHttpStatusCodes.BadRequest);
        }

        // Same as other top-ups (e.g. Interswitch txn_ref): our internal PaymentRef is the PSP merchant reference.
        var merchantReference = payment.PaymentRef.Trim();

        var amountForEasypay = AkibaPaySignatureHelper.NormalizeAmount(payment.Amount);                

        var request = new EasypayCustomerDepositRequestModel
        {
            ReferenceId = merchantReference,
            Amount = amountForEasypay,
            Currency = currencyCode,
            Phone = fromNumber,
        };

        var requestBodyForLog = SerializeRequestForLog(request);
        logger.LogInformation(
            "Easypay CustomerDeposit initiate request. PaymentId={PaymentId}, Url={Url}, Body={RequestBody}",
            payment.Id,
            baseUrl,
            requestBodyForLog);
        var tokenResult = await easypayManagementTokenProvider.GetTokenAsync(baseUrl, apiKey);
        if (tokenResult is null || !tokenResult.IsSuccess)
        {
            return Result<PaymentInitializationInternalModel>.Failure(
                localizer[LocalizationKeys.PaymentEasypayManagementLoginHttpFailed],
                AppHttpStatusCodes.BadRequest);
        }
        var response = await easypayHttpClient.PostCustomerDepositAsync(
            baseUrl,
            request,
            tokenResult.Value!,
            cancellationToken).ConfigureAwait(false);

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var statusCode = ((int)response.StatusCode).ToString(CultureInfo.InvariantCulture);

        logger.LogInformation(
            "Easypay CustomerDeposit initiate response. PaymentId={PaymentId}, StatusCode={StatusCode}, Body={ResponseBody}",
            payment.Id,
            statusCode,
            responseBody);

        await PersistInitiateTransactionAuditAsync(
            payment.Id,
            requestBodyForLog,
            responseBody,
            statusCode,
            response.ReasonPhrase,
            cancellationToken).ConfigureAwait(false);

        if (response.StatusCode is not (HttpStatusCode.OK or HttpStatusCode.Created or HttpStatusCode.Accepted))
        {
            logger.LogWarning(
                "Easypay customer deposit initiate failed. PaymentId={PaymentId}, StatusCode={StatusCode}, Body={ResponseBody}",
                payment.Id,
                statusCode,
                responseBody);

            return Result<PaymentInitializationInternalModel>.Failure(
                localizer[LocalizationKeys.PaymentEasypayInitiateHttpFailed, statusCode, responseBody],
                AppHttpStatusCodes.Failed);
        }

        return Result<PaymentInitializationInternalModel>.Success(
            new PaymentInitializationInternalModel(
                PaymentNextActionEnum.None,
                new NoActionNextActionModel(localizer[LocalizationKeys.PaymentConfirmationExternal])));
    }

    protected override async Task<Result<PaymentCheckStatusInternalModel>> HandleGetTransactionStatus(
        PaymentTransactionStatusContextInternalModel context)
    {
        var result = new PaymentCheckStatusInternalModel();

        try
        {
            if (string.IsNullOrWhiteSpace(context.PaymentTransactionReference))
            {
                return Result<PaymentCheckStatusInternalModel>.Failure(
                    localizer[LocalizationKeys.PaymentEasypayStatusLookupPaymentRefRequired],
                    AppHttpStatusCodes.BadRequest,
                    result);
            }
            var managementCredentials = PaymentMethodConfigReader.ResolveEasypayManagementCredentials(
                context.PaymentMethod.Config,
                easypayOptions.Value);

            var merchantReference = context.PaymentTransactionReference.Trim();
            Result<PaymentCheckStatusInternalModel>? lastLookupResult = null;
            for (var attempt = 0; attempt < 2; attempt++)
            {
                if (attempt > 0)
                {
                    easypayManagementTokenProvider.InvalidateToken(
                        managementCredentials.ManagementBaseUrl,
                        managementCredentials.apiKey);
                }

                lastLookupResult = await LookupManagementTransactionStatusAsync(
                    managementCredentials,
                    merchantReference,
                    context,
                    result).ConfigureAwait(false);

                if (lastLookupResult.IsSuccess ||
                    lastLookupResult.StatusCode != AppHttpStatusCodes.Unauthorized)
                {
                    return lastLookupResult;
                }
            }

            return lastLookupResult!;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "An error occurred while checking Easypay transaction status for payment {PaymentId}.",
                context.PaymentId);
            return Result<PaymentCheckStatusInternalModel>.Failure(
                localizer[LocalizationKeys.PaymentEasypayStatusCheckUnexpectedError],
                AppHttpStatusCodes.Failed,
                result);
        }
    }

    private async Task<Result<PaymentCheckStatusInternalModel>> LookupManagementTransactionStatusAsync(
        PaymentMethodConfigReader.EasypayManagementCredentials managementCredentials,
        string merchantReference,
        PaymentTransactionStatusContextInternalModel context,
        PaymentCheckStatusInternalModel result)
    {
        var tokenResult = await easypayManagementTokenProvider
            .GetTokenAsync(
                managementCredentials.ManagementBaseUrl,
                managementCredentials.apiKey)
            .ConfigureAwait(false);
        if (!tokenResult.IsSuccess)
        {
            return Result<PaymentCheckStatusInternalModel>.Failure(
                tokenResult.Message!,
                tokenResult.StatusCode,
                result);
        }

        var statusRequestBody = JsonSerializer.Serialize(new
        {
            method = "POST",
            url = $"{managementCredentials.ManagementBaseUrl.TrimEnd('/')}/status",
            referenceId = merchantReference
        });
        result.RequestBody = statusRequestBody;
        logger.LogInformation(
            "Easypay management transaction status request. PaymentId={PaymentId}, MerchantReference={MerchantReference}",
            context.PaymentId,
            merchantReference);
        LogTransactionStatusRequest(context.PaymentId, statusRequestBody);

        var statusResponse = await easypayHttpClient
            .GetTransactionStatusAsync(managementCredentials.ManagementBaseUrl, tokenResult.Value, merchantReference)
            .ConfigureAwait(false);
        var statusResponseBody = await statusResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
        result.ResponseBody = statusResponseBody;
        result.ResponseCode = ((int)statusResponse.StatusCode).ToString(CultureInfo.InvariantCulture);
        LogTransactionStatusResponse(context.PaymentId, result.ResponseCode, statusResponseBody);

        if (statusResponse.StatusCode == HttpStatusCode.Unauthorized)
        {
            logger.LogWarning(
                "Easypay management transaction status returned 401. PaymentId={PaymentId}, MerchantReference={MerchantReference}",
                context.PaymentId,
                merchantReference);

            return Result<PaymentCheckStatusInternalModel>.Failure(
                localizer[LocalizationKeys.PaymentEasypayStatusHttpFailed, result.ResponseCode],
                AppHttpStatusCodes.Unauthorized,
                result);
        }

        if (statusResponse.StatusCode == HttpStatusCode.NotFound &&
            !string.IsNullOrWhiteSpace(statusResponseBody))
        {
            result.ResponseMessage = statusResponseBody.Trim();
            result.PaymentExternalRef = context.PaymentExternalReference ?? string.Empty;

            if (PspTransactionStatusNotFoundPolicy.ShouldMarkFailed(context))
            {
                logger.LogWarning(
                    "Easypay management transaction status returned 404 with response body; treating payment as failed. PaymentId={PaymentId}, MerchantReference={MerchantReference}, Attempt={Attempt}/{TotalAttempts}, Body={ResponseBody}",
                    context.PaymentId,
                    merchantReference,
                    context.StatusPollingAttempt,
                    context.StatusPollingTotalAttempts,
                    statusResponseBody);

                result.Status = PaymentStatus.Failed;
            }
            else
            {
                logger.LogWarning(
                    "Easypay management transaction status returned 404 with response body; deferring failure (transaction may not be indexed yet). PaymentId={PaymentId}, MerchantReference={MerchantReference}, Attempt={Attempt}/{TotalAttempts}, FailAfterAttempts={FailAfterAttempts}, Body={ResponseBody}",
                    context.PaymentId,
                    merchantReference,
                    context.StatusPollingAttempt,
                    context.StatusPollingTotalAttempts,
                    context.NotFoundFailAfterAttempts,
                    statusResponseBody);

                result.Status = PaymentStatus.Processing;
            }

            return Result<PaymentCheckStatusInternalModel>.Success(result);
        }

        if (statusResponse.StatusCode != HttpStatusCode.OK)
        {
            logger.LogWarning(
                "Easypay management transaction status request failed. PaymentId={PaymentId}, StatusCode={StatusCode}, Body={ResponseBody}",
                context.PaymentId,
                result.ResponseCode,
                statusResponseBody);

            return Result<PaymentCheckStatusInternalModel>.Failure(
                localizer[LocalizationKeys.PaymentEasypayStatusHttpFailed, result.ResponseCode],
                AppHttpStatusCodes.Failed,
                result);
        }
        var transactionStatus = JsonSerializer.Deserialize<EasypayTransactionStatusResponseModel>(
                statusResponseBody,
                LogJsonOptions);                
        if (transactionStatus is null)
        {
            return Result<PaymentCheckStatusInternalModel>.Failure(
                localizer[LocalizationKeys.PaymentEasypayStatusParseFailed],
                AppHttpStatusCodes.Failed,
                result);
        }

        if (transactionStatus.Success != 1)
        {
            result.ResponseMessage = transactionStatus.ErrorMsg ?? statusResponseBody.Trim();
            result.PaymentExternalRef = context.PaymentExternalReference ?? string.Empty;

            if (transactionStatus.ErrorCode == 3020)
            {
                logger.LogWarning(
                    "Easypay transaction status returned errorcode 3020 (reference not found); treating payment as failed. PaymentId={PaymentId}, MerchantReference={MerchantReference}, ErrorMsg={ErrorMsg}",
                    context.PaymentId,
                    merchantReference,
                    transactionStatus.ErrorMsg);

                result.Status = PaymentStatus.Failed;
                result.UserMessage = localizer[LocalizationKeys.PaymentEasypayStatusReferenceNotFound].Value;
                return Result<PaymentCheckStatusInternalModel>.Success(result);
            }

            logger.LogWarning(
                "Easypay transaction status returned success=0. PaymentId={PaymentId}, MerchantReference={MerchantReference}, ErrorCode={ErrorCode}, ErrorMsg={ErrorMsg}",
                context.PaymentId,
                merchantReference,
                transactionStatus.ErrorCode,
                transactionStatus.ErrorMsg);

            return Result<PaymentCheckStatusInternalModel>.Failure(
                localizer[LocalizationKeys.PaymentEasypayStatusParseFailed],
                AppHttpStatusCodes.Failed,
                result);
        }

        if (transactionStatus.Data is null)
        {
            return Result<PaymentCheckStatusInternalModel>.Failure(
                localizer[LocalizationKeys.PaymentEasypayStatusParseFailed],
                AppHttpStatusCodes.Failed,
                result);
        }

        result.ResponseMessage = string.IsNullOrWhiteSpace(transactionStatus.Data.Reason)
            ? transactionStatus.Data.Status
            : transactionStatus.Data.Reason;
        result.Status = MapManagementTransactionStatus(transactionStatus.Data.Status);
        result.PaymentExternalRef = transactionStatus.Data.TransactionId ??
                                    context.PaymentExternalReference ??
                                    string.Empty;
        if (result.Status == PaymentStatus.Failed && !string.IsNullOrWhiteSpace(transactionStatus.Data.Reason))
        {
            result.UserMessage = transactionStatus.Data.Reason;
        }

        logger.LogInformation(
            "Easypay management transaction status response. PaymentId={PaymentId}, Reference={Reference}, Status={Status}",
            context.PaymentId,
            merchantReference,
            transactionStatus.Data.Status);

        return Result<PaymentCheckStatusInternalModel>.Success(result);
    }

    private static PaymentStatus MapManagementTransactionStatus(string? transactionStatus)
    {
        return (transactionStatus ?? string.Empty).Trim().ToUpperInvariant() switch
        {
            "SUCCESSFUL" or "SUCCESS" or "COMPLETED" => PaymentStatus.Completed,
            "FAILED" or "FAILURE" => PaymentStatus.Failed,
            "PENDING" => PaymentStatus.Processing,
            _ => PaymentStatus.Processing
        };
    }

    private async Task PersistInitiateTransactionAuditAsync(
        Guid paymentId,
        string requestBody,
        string responseBody,
        string responseCode,
        string? responseMessage,
        CancellationToken cancellationToken)
    {
        if (!CreateTransactions)
        {
            return;
        }

        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var query = await unitOfWork.PaymentTxnRepository.GetAllQueryableAsync(
                t => t.PaymentId == paymentId && t.Type == TransactionType.Initiate,
                ct);
            var txn = await query
                .OrderByDescending(t => t.CreatedAtUtc)
                .FirstOrDefaultAsync(ct);

            if (txn is null)
            {
                logger.LogWarning(
                    "Easypay initiate audit skipped: no initiate PaymentTxn found for payment {PaymentId}.",
                    paymentId);
                return;
            }

            txn.SetRequestBody(requestBody);
            txn.SetResponseBody(responseBody);
            txn.SetResponseCode(responseCode);
            if (!string.IsNullOrWhiteSpace(responseMessage))
            {
                txn.SetResponseMessage(responseMessage);
            }

            unitOfWork.PaymentTxnRepository.Update(txn);
            await unitOfWork.CompleteAsync();
        }, cancellationToken);
    }

    private static string SerializeRequestForLog(EasypayCustomerDepositRequestModel request) =>
        JsonSerializer.Serialize(
            new
            {
                request.ReferenceId,
                request.Amount,
                request.Currency,
                request.Phone
            },
            LogJsonOptions);

    private static string SerializeRequestForLog(EasypayCardCollectionRequestModel request) =>
        JsonSerializer.Serialize(request, LogJsonOptions);

    private static string? NullIfWhiteSpace(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static bool IsPublicHttpsUrl(string value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
            uri.Scheme != Uri.UriSchemeHttps ||
            uri.IsLoopback ||
            string.IsNullOrWhiteSpace(uri.Host))
        {
            return false;
        }

        if (!IPAddress.TryParse(uri.Host, out var address))
        {
            return true;
        }

        var bytes = address.GetAddressBytes();
        return address.AddressFamily switch
        {
            System.Net.Sockets.AddressFamily.InterNetwork =>
                bytes[0] != 10 &&
                bytes[0] != 127 &&
                !(bytes[0] == 169 && bytes[1] == 254) &&
                !(bytes[0] == 172 && bytes[1] is >= 16 and <= 31) &&
                !(bytes[0] == 192 && bytes[1] == 168),
            System.Net.Sockets.AddressFamily.InterNetworkV6 =>
                !address.IsIPv6LinkLocal && !address.IsIPv6SiteLocal,
            _ => false
        };
    }
    private static string RedactMerchantPassInSignQueryString(string signQueryString)
    {
        const string prefix = "merchantPass=";
        var start = signQueryString.IndexOf(prefix, StringComparison.Ordinal);
        if (start < 0)
        {
            return signQueryString;
        }

        start += prefix.Length;
        var end = signQueryString.IndexOf('&', start);
        if (end < 0)
        {
            end = signQueryString.Length;
        }

        return string.Concat(signQueryString.AsSpan(0, start), "***", signQueryString.AsSpan(end));
    }

    private void ValidateRequiredConfiguredFields(IReadOnlyDictionary<string, string> fields)
    {
        foreach (var key in RequiredConfiguredFields)
        {
            if (!fields.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException(
                    localizer[LocalizationKeys.PaymentEasypayConfigFieldEmpty, key].Value);
            }
        }

        var baseUrl = fields["easypay_base_url"].Trim();
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out _))
        {
            throw new InvalidOperationException(
                localizer[LocalizationKeys.PaymentEasypayConfigBaseUrlInvalid].Value);
        }
    }
}
