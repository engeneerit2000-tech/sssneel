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
using SNEL.WalletPlatform.Application.Payments.Models.External.Afrimony;
using SNEL.WalletPlatform.Application.Payments.Models.Internal;
using SNEL.WalletPlatform.Application.Payments.Models.Output;
using SNEL.WalletPlatform.Domain.Lookups;
using SNEL.WalletPlatform.Domain.Payments;
using SNEL.WalletPlatform.Infrastructure.Payments.Clients;
using System.Globalization;
using System.Net;
using System.Text.Json;

namespace SNEL.WalletPlatform.Infrastructure.Payments.Connectors;

public sealed class AfrimonyPspConnector(
    IUnitOfWork unitOfWork,
    AfrimonyHttpClient afrimonyHttpClient,
    IAfrimonyTokenProvider afrimonyTokenProvider,
    IAfrimonyPaymentInitContext afrimonyPaymentInitContext,
    IOptions<AfrimonyOptions> afrimonyOptions,
    IStringLocalizer<SharedResource> localizer,
    ILogger<AfrimonyPspConnector> logger) : PspConnector(unitOfWork, logger, localizer)
{
    private static readonly string[] RequiredConfiguredFields =
    [
        "afrimony_base_url",
        "oauth_url",
        "oauth_basic",
        "id_value",
        "mpin",
        "product_id",
        "initiator",
        "bearer_code"
    ];

    private static readonly JsonSerializerOptions LogJsonOptions = new()
    {
        WriteIndented = false,
        PropertyNameCaseInsensitive = true
    };

    public override string PaymentServiceProviderCode => "AFRIMONEY";
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
                case "MERCHANT_PAY":
                    return await InitiateMerchantPayAsync(paymentContext, cancellationToken);
                default:
                    return Result<PaymentInitializationInternalModel>.Failure(
                        localizer[LocalizationKeys.PaymentAfrimonyInitiateMethodNotImplemented, paymentContext.PaymentMethod.Code],
                        AppHttpStatusCodes.BadRequest);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Afrimony payment initiation failed for payment {PaymentId}.", payment.Id);
            return Result<PaymentInitializationInternalModel>.Failure(ex.Message, AppHttpStatusCodes.Failed);
        }
    }

    private async Task<Result<PaymentInitializationInternalModel>> InitiateMerchantPayAsync(
        PaymentEntity payment,
        CancellationToken cancellationToken)
    {
        var configuredFields = PaymentMethodConfigReader.GetFields(payment.PaymentMethod!.Config);
        if (configuredFields.Count == 0)
        {
            throw new InvalidOperationException(
                localizer[LocalizationKeys.PaymentAfrimonyConfigMissingOnMethod, payment.PaymentMethod!.Code].Value);
        }

        ValidateRequiredConfiguredFields(configuredFields);

        var credentials = PaymentMethodConfigReader.ResolveAfrimonyCredentials(
            payment.PaymentMethod.Config,
            afrimonyOptions.Value);

        var rawFromNumber = afrimonyPaymentInitContext.FromNumber;
        if (string.IsNullOrWhiteSpace(rawFromNumber))
        {
            return Result<PaymentInitializationInternalModel>.Failure(
                localizer[LocalizationKeys.PaymentAfrimonyFromNumberRequired],
                AppHttpStatusCodes.BadRequest);
        }

        var fromNumber = AfrimonyCurrencyHelper.NormalizeFromNumber(rawFromNumber);
        if (fromNumber.Length == 0)
        {
            return Result<PaymentInitializationInternalModel>.Failure(
                localizer[LocalizationKeys.PaymentAfrimonyFromNumberInvalid],
                AppHttpStatusCodes.BadRequest);
        }

        if (fromNumber.Length >= 3)
        {
            var phonePrefix = fromNumber[1..3];
            var isValid = await unitOfWork.PaymentProviderRouterRepository
                .IsPhoneMatchingProvider("243", phonePrefix, PaymentServiceProviderCode, cancellationToken);
            if (!isValid)
            {
                logger.LogWarning(
                    "Afrimony phone prefix not supported. PaymentId={PaymentId}, FromNumber={FromNumber}, Prefix={Prefix}",
                    payment.Id, fromNumber, phonePrefix);
                return Result<PaymentInitializationInternalModel>.Failure(
                    localizer[LocalizationKeys.PaymentAfrimonyFromNumberPrefixNotSupported],
                    AppHttpStatusCodes.BadRequest);
            }
        }

        var allCurrencies = CachingFactory.GetValue<CurrencyEntity>();
        var currency = allCurrencies.FirstOrDefault(x => x.Id == payment.CurrencyId);
        if (currency is null)
        {
            return Result<PaymentInitializationInternalModel>.Failure(
                localizer[LocalizationKeys.CommonKeys.CurrencyNotFound, payment.CurrencyId],
                AppHttpStatusCodes.NotFound);
        }

        if (!AfrimonyCurrencyHelper.TryMapCurrencyCode(currency.Abbreviation, out var afrimonyCurrencyCode))
        {
            return Result<PaymentInitializationInternalModel>.Failure(
                localizer[LocalizationKeys.PaymentAfrimonyCurrencyUnsupported, currency.Abbreviation],
                AppHttpStatusCodes.BadRequest);
        }

        if (string.IsNullOrWhiteSpace(payment.PaymentRef))
        {
            return Result<PaymentInitializationInternalModel>.Failure(
                localizer[LocalizationKeys.PaymentAfrimonyPaymentRefRequired],
                AppHttpStatusCodes.BadRequest);
        }

        var externalReferenceId = payment.PaymentRef.Trim();
        var productId = configuredFields["product_id"].Trim();

        var request = new AfrimonyMerchantPayRequestModel
        {
            BearerCode = configuredFields["bearer_code"].Trim(),
            TransactionAmount = payment.Amount,
            Currency = afrimonyCurrencyCode,
            ExternalReferenceId = externalReferenceId,
            Initiator = configuredFields["initiator"].Trim(),
            Transactor = new AfrimonyPartyModel
            {
                IdType = "mobileNumber",
                IdValue = configuredFields["id_value"].Trim(),
                ProductId = productId,
                Mpin = configuredFields["mpin"].Trim()
            },
            Sender = new AfrimonyPartyModel
            {
                IdType = "mobileNumber",
                IdValue = fromNumber,
                ProductId = productId
            }
        };

        var requestBodyForLog = SerializeRequestForLog(request);
        logger.LogInformation(
            "Afrimony MerchantPay initiate request. PaymentId={PaymentId}, Url={Url}, Body={RequestBody}",
            payment.Id,
            credentials.BaseUrl,
            requestBodyForLog);

        Result<PaymentInitializationInternalModel>? lastResult = null;
        for (var attempt = 0; attempt < 2; attempt++)
        {
            if (attempt > 0)
            {
                afrimonyTokenProvider.InvalidateToken(credentials.OAuthUrl, credentials.OAuthBasic);
            }

            var tokenResult = await afrimonyTokenProvider
                .GetTokenAsync(credentials.OAuthUrl, credentials.OAuthBasic, cancellationToken)
                .ConfigureAwait(false);
            if (!tokenResult.IsSuccess)
            {
                return Result<PaymentInitializationInternalModel>.Failure(
                    tokenResult.Message!,
                    tokenResult.StatusCode);
            }

            var response = await afrimonyHttpClient
                .PostMerchantPayAsync(credentials.BaseUrl, request, tokenResult.Value, cancellationToken)
                .ConfigureAwait(false);

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var statusCode = ((int)response.StatusCode).ToString(CultureInfo.InvariantCulture);

            logger.LogInformation(
                "Afrimony MerchantPay initiate response. PaymentId={PaymentId}, StatusCode={StatusCode}, Body={ResponseBody}",
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

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                lastResult = Result<PaymentInitializationInternalModel>.Failure(
                    localizer[LocalizationKeys.PaymentAfrimonyInitiateHttpFailed, statusCode, responseBody],
                    AppHttpStatusCodes.Unauthorized);
                continue;
            }

            if (response.StatusCode is not (HttpStatusCode.OK or HttpStatusCode.Created or HttpStatusCode.Accepted))
            {
                logger.LogWarning(
                    "Afrimony merchant pay initiate failed. PaymentId={PaymentId}, StatusCode={StatusCode}, Body={ResponseBody}",
                    payment.Id,
                    statusCode,
                    responseBody);

                return Result<PaymentInitializationInternalModel>.Failure(
                    localizer[LocalizationKeys.PaymentAfrimonyInitiateHttpFailed, statusCode, responseBody],
                    AppHttpStatusCodes.Failed);
            }

            var merchantPayResponse = JsonSerializer.Deserialize<AfrimonyMerchantPayResponseModel>(
                responseBody,
                LogJsonOptions);

            if (merchantPayResponse?.Status?.Equals("FAILED", StringComparison.OrdinalIgnoreCase) == true)
            {
                var errorMessage = merchantPayResponse.Errors?.FirstOrDefault()?.Message
                    ?? "Afrimony merchant pay request failed.";

                logger.LogWarning(
                    "Afrimony MerchantPay returned FAILED status. PaymentId={PaymentId}, TxnStatus={TxnStatus}, Body={ResponseBody}",
                    payment.Id,
                    merchantPayResponse.TxnStatus,
                    responseBody);

                return Result<PaymentInitializationInternalModel>.Failure(
                    errorMessage,
                    AppHttpStatusCodes.Failed);
            }

            if (merchantPayResponse?.Status?.Equals("PAUSED", StringComparison.OrdinalIgnoreCase) == true)
            {
                var txnStatus = (merchantPayResponse.TxnStatus ?? string.Empty).Trim().ToUpperInvariant();

                logger.LogInformation(
                    "Afrimony MerchantPay returned PAUSED status. PaymentId={PaymentId}, TxnStatus={TxnStatus}",
                    payment.Id,
                    txnStatus);

                if (txnStatus is "TF")
                {
                    var errorMessage = merchantPayResponse.Errors?.FirstOrDefault()?.Message
                        ?? "Afrimony merchant pay transaction failed.";

                    return Result<PaymentInitializationInternalModel>.Failure(
                        errorMessage,
                        AppHttpStatusCodes.Failed);
                }
            }

            if (!string.IsNullOrWhiteSpace(merchantPayResponse?.TransactionId))
            {
                payment.SetExternalRef(merchantPayResponse.TransactionId.Trim());
                unitOfWork.PaymentRepository.Update(payment);
                await unitOfWork.CompleteAsync().ConfigureAwait(false);
            }

            return Result<PaymentInitializationInternalModel>.Success(
                new PaymentInitializationInternalModel(
                    PaymentNextActionEnum.None,
                    new NoActionNextActionModel(localizer[LocalizationKeys.PaymentConfirmationExternal])));
        }

        return lastResult!;
    }

    protected override async Task<Result<PaymentCheckStatusInternalModel>> HandleGetTransactionStatus(
        PaymentTransactionStatusContextInternalModel context)
    {
        var result = new PaymentCheckStatusInternalModel();

        try
        {
            var referenceId = !string.IsNullOrWhiteSpace(context.PaymentExternalReference)
                ? context.PaymentExternalReference.Trim()
                : context.PaymentTransactionReference?.Trim();

            if (string.IsNullOrWhiteSpace(referenceId))
            {
                return Result<PaymentCheckStatusInternalModel>.Failure(
                    localizer[LocalizationKeys.PaymentAfrimonyStatusLookupPaymentRefRequired],
                    AppHttpStatusCodes.BadRequest,
                    result);
            }

            var credentials = PaymentMethodConfigReader.ResolveAfrimonyCredentials(
                context.PaymentMethod.Config,
                afrimonyOptions.Value);

            Result<PaymentCheckStatusInternalModel>? lastLookupResult = null;
            for (var attempt = 0; attempt < 2; attempt++)
            {
                if (attempt > 0)
                {
                    afrimonyTokenProvider.InvalidateToken(credentials.OAuthUrl, credentials.OAuthBasic);
                }

                lastLookupResult = await LookupManagementTransactionStatusAsync(
                    credentials,
                    referenceId,
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
                "An error occurred while checking Afrimony transaction status for payment {PaymentId}.",
                context.PaymentId);
            return Result<PaymentCheckStatusInternalModel>.Failure(
                localizer[LocalizationKeys.PaymentAfrimonyStatusCheckUnexpectedError],
                AppHttpStatusCodes.Failed,
                result);
        }
    }

    private async Task<Result<PaymentCheckStatusInternalModel>> LookupManagementTransactionStatusAsync(
        PaymentMethodConfigReader.AfrimonyCredentials managementCredentials,
        string merchantReference,
        PaymentTransactionStatusContextInternalModel context,
        PaymentCheckStatusInternalModel result)
    {
        var tokenResult = await afrimonyTokenProvider
            .GetTokenAsync(managementCredentials.OAuthUrl, managementCredentials.OAuthBasic)
            .ConfigureAwait(false);
        if (!tokenResult.IsSuccess)
        {
            return Result<PaymentCheckStatusInternalModel>.Failure(
                tokenResult.Message!,
                tokenResult.StatusCode,
                result);
        }

        var enquiryRequest = new AfrimonyTransactionEnquiryRequestModel
        {
            Command = new AfrimonyTransactionEnquiryCommandRequest
            {
                Type = "TRANSREQ",
                ReferenceId = merchantReference
            }
        };

        var statusRequestBody = JsonSerializer.Serialize(enquiryRequest, LogJsonOptions);
        result.RequestBody = statusRequestBody;
        logger.LogInformation(
            "Afrimony TransactionEnquiry request. PaymentId={PaymentId}, REFERENCEID={ReferenceId}",
            context.PaymentId,
            merchantReference);
        LogTransactionStatusRequest(context.PaymentId, statusRequestBody);

        var statusResponse = await afrimonyHttpClient
            .PostTransactionEnquiryAsync(managementCredentials.BaseUrl, enquiryRequest, tokenResult.Value)
            .ConfigureAwait(false);
        var statusResponseBody = await statusResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
        result.ResponseBody = statusResponseBody;
        result.ResponseCode = ((int)statusResponse.StatusCode).ToString(CultureInfo.InvariantCulture);
        LogTransactionStatusResponse(context.PaymentId, result.ResponseCode, statusResponseBody);

        if (statusResponse.StatusCode == HttpStatusCode.Unauthorized)
        {
            logger.LogWarning(
                "Afrimony TransactionEnquiry returned 401. PaymentId={PaymentId}, REFERENCEID={ReferenceId}",
                context.PaymentId,
                merchantReference);

            return Result<PaymentCheckStatusInternalModel>.Failure(
                localizer[LocalizationKeys.PaymentAfrimonyStatusHttpFailed, result.ResponseCode],
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
                    "Afrimony TransactionEnquiry returned 404 with response body; treating payment as failed. PaymentId={PaymentId}, REFERENCEID={ReferenceId}, Attempt={Attempt}/{TotalAttempts}, Body={ResponseBody}",
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
                    "Afrimony TransactionEnquiry returned 404 with response body; deferring failure (transaction may not be indexed yet). PaymentId={PaymentId}, REFERENCEID={ReferenceId}, Attempt={Attempt}/{TotalAttempts}, FailAfterAttempts={FailAfterAttempts}, Body={ResponseBody}",
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
                "Afrimony TransactionEnquiry request failed. PaymentId={PaymentId}, StatusCode={StatusCode}, Body={ResponseBody}",
                context.PaymentId,
                result.ResponseCode,
                statusResponseBody);

            return Result<PaymentCheckStatusInternalModel>.Failure(
                localizer[LocalizationKeys.PaymentAfrimonyStatusHttpFailed, result.ResponseCode],
                AppHttpStatusCodes.Failed,
                result);
        }

        var enquiryResponse = JsonSerializer.Deserialize<AfrimonyTransactionEnquiryResponseModel>(
            statusResponseBody,
            LogJsonOptions);
        if (enquiryResponse?.Command is null)
        {
            return Result<PaymentCheckStatusInternalModel>.Failure(
                localizer[LocalizationKeys.PaymentAfrimonyStatusParseFailed],
                AppHttpStatusCodes.Failed,
                result);
        }

        var command = enquiryResponse.Command;
        var statusToken = FirstNonEmpty(command.RefTxnState, command.StatusCode, command.StatusValue);

        result.ResponseMessage = FirstNonEmpty(command.RefMessage, command.Message, statusToken) ?? string.Empty;
        result.PaymentExternalRef =
            FirstNonEmpty(merchantReference, context.PaymentExternalReference) ?? string.Empty;

        if (IsDefinitiveInvalidTransaction(command))
        {
            logger.LogWarning(
                "Afrimony TransactionEnquiry returned invalid reference (TB002 / Transaction is invalid); treating payment as failed. PaymentId={PaymentId}, REFERENCEID={ReferenceId}, TXNSTATUS={TxnStatus}, MESSAGE={Message}",
                context.PaymentId,
                merchantReference,
                command.TxnStatus,
                command.Message);

            result.Status = PaymentStatus.Failed;
            result.UserMessage = localizer[LocalizationKeys.PaymentAfrimonyStatusNotFoundFailed].Value;
            return Result<PaymentCheckStatusInternalModel>.Success(result);
        }

        if (IsEnquiryNotFound(command))
        {
            if (PspTransactionStatusNotFoundPolicy.ShouldMarkFailed(context))
            {
                logger.LogWarning(
                    "Afrimony TransactionEnquiry returned not-found equivalent (REFTXNSTATUS 8000 / timed out); treating payment as failed. PaymentId={PaymentId}, REFERENCEID={ReferenceId}, Attempt={Attempt}/{TotalAttempts}, REFTXNSTATE={RefTxnState}, REFTXNSTATUS={RefTxnStatus}, REFMESSAGE={RefMessage}",
                    context.PaymentId,
                    merchantReference,
                    context.StatusPollingAttempt,
                    context.StatusPollingTotalAttempts,
                    command.RefTxnState,
                    command.RefTxnStatus,
                    command.RefMessage);

                result.Status = PaymentStatus.Failed;
                result.UserMessage = localizer[LocalizationKeys.PaymentAfrimonyStatusNotFoundFailed].Value;
            }
            else
            {
                logger.LogWarning(
                    "Afrimony TransactionEnquiry returned not-found equivalent (REFTXNSTATUS 8000 / timed out); deferring failure (transaction may not be indexed yet). PaymentId={PaymentId}, REFERENCEID={ReferenceId}, Attempt={Attempt}/{TotalAttempts}, FailAfterAttempts={FailAfterAttempts}, REFTXNSTATE={RefTxnState}, REFTXNSTATUS={RefTxnStatus}, REFMESSAGE={RefMessage}",
                    context.PaymentId,
                    merchantReference,
                    context.StatusPollingAttempt,
                    context.StatusPollingTotalAttempts,
                    context.NotFoundFailAfterAttempts,
                    command.RefTxnState,
                    command.RefTxnStatus,
                    command.RefMessage);

                result.Status = PaymentStatus.Processing;
            }

            return Result<PaymentCheckStatusInternalModel>.Success(result);
        }

        result.Status = MapEnquiryStatus(statusToken, command.StatusValue);

        logger.LogInformation(
            "Afrimony TransactionEnquiry response. PaymentId={PaymentId}, REFERENCEID={ReferenceId}, REFTXNSTATE={RefTxnState}, StatusCode={StatusCode}, StatusValue={StatusValue}, TransId={TransId}",
            context.PaymentId,
            merchantReference,
            command.RefTxnState,
            command.StatusCode,
            command.StatusValue,
            command.TransId);

        return Result<PaymentCheckStatusInternalModel>.Success(result);
    }

    private static PaymentStatus MapEnquiryStatus(string? statusCode, string? statusValue)
    {
        var code = (statusCode ?? string.Empty).Trim().ToUpperInvariant();
        var value = (statusValue ?? string.Empty).Trim().ToUpperInvariant();

        if (code is "TS" || value is "SUCCESS" or "SUCCESSFUL" or "COMPLETED")
        {
            return PaymentStatus.Completed;
        }

        if (code is "TF" || value is "FAILED" or "FAILURE")
        {
            return PaymentStatus.Failed;
        }

        return PaymentStatus.Processing;
    }

    private static bool IsDefinitiveInvalidTransaction(AfrimonyTransactionEnquiryCommandResponse command)
    {
        var message = (command.Message ?? string.Empty).Trim();
        if (message.Equals("Transaction is invalid", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var txnStatus = (command.TxnStatus ?? string.Empty).Trim().ToUpperInvariant();
        return txnStatus == "TB002";
    }

    private static bool IsEnquiryNotFound(AfrimonyTransactionEnquiryCommandResponse command)
    {
        var state = (command.RefTxnState ?? string.Empty).Trim().ToUpperInvariant();
        if (state is not "TF")
        {
            return false;
        }

        var refStatus = (command.RefTxnStatus ?? string.Empty).Trim();
        if (refStatus == "8000")
        {
            return true;
        }

        var refMessage = (command.RefMessage ?? string.Empty).Trim();
        return refMessage.Equals("Transaction Timed Out", StringComparison.OrdinalIgnoreCase);
    }

    private static string? FirstNonEmpty(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }
        }

        return null;
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
                    "Afrimony initiate audit skipped: no initiate PaymentTxn found for payment {PaymentId}.",
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

    private static string SerializeRequestForLog(AfrimonyMerchantPayRequestModel request) =>
        JsonSerializer.Serialize(
            new
            {
                request.BearerCode,
                request.TransactionAmount,
                request.Currency,
                request.ExternalReferenceId,
                request.Initiator,
                Transactor = new
                {
                    request.Transactor.IdType,
                    request.Transactor.IdValue,
                    request.Transactor.ProductId,
                    Mpin = "***"
                },
                Sender = new
                {
                    request.Sender.IdType,
                    request.Sender.IdValue,
                    request.Sender.ProductId
                }
            },
            LogJsonOptions);

    private void ValidateRequiredConfiguredFields(IReadOnlyDictionary<string, string> fields)
    {
        foreach (var key in RequiredConfiguredFields)
        {
            if (!fields.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException(
                    localizer[LocalizationKeys.PaymentAfrimonyConfigFieldEmpty, key].Value);
            }
        }

        var baseUrl = fields["afrimony_base_url"].Trim();
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out _))
        {
            throw new InvalidOperationException(
                localizer[LocalizationKeys.PaymentAfrimonyConfigBaseUrlInvalid].Value);
        }

        var oauthUrl = fields["oauth_url"].Trim();
        if (!Uri.TryCreate(oauthUrl, UriKind.Absolute, out _))
        {
            throw new InvalidOperationException(
                localizer[LocalizationKeys.PaymentAfrimonyConfigOAuthUrlInvalid].Value);
        }
    }
}
