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
using SNEL.WalletPlatform.Application.Payments.Models.External.AkibaPay;
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

public sealed class AkibaPayPspConnector(
    IUnitOfWork unitOfWork,
    AkibaPayHttpClient akibaPayHttpClient,
    IAkibaPayManagementTokenProvider akibaPayManagementTokenProvider,
    IAkibaPayPaymentInitContext akibaPayPaymentInitContext,
    IOptions<AkibaPayOptions> akibaPayOptions,
    IStringLocalizer<SharedResource> localizer,
    ILogger<AkibaPayPspConnector> logger) : PspConnector(unitOfWork, logger, localizer)
{
    private static readonly string[] RequiredConfiguredFields =
    [
        "akibapay_base_url",
        "merchant_id",
        "merchant_pass",
        "callback_url"
    ];

    private static readonly JsonSerializerOptions LogJsonOptions = new()
    {
        WriteIndented = false,
        PropertyNameCaseInsensitive = true
    };

    public override string PaymentServiceProviderCode => "AKIPA-PAY";
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
                case "CUSTOMER_DEPOSIT":
                    return await InitiateCustomerDepositAsync(paymentContext, cancellationToken);
                default:
                    return Result<PaymentInitializationInternalModel>.Failure(
                        localizer[LocalizationKeys.PaymentAkibaPayInitiateMethodNotImplemented, paymentContext.PaymentMethod.Code],
                        AppHttpStatusCodes.BadRequest);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "AkibaPay payment initiation failed for payment {PaymentId}.", payment.Id);
            return Result<PaymentInitializationInternalModel>.Failure(ex.Message, AppHttpStatusCodes.Failed);
        }
    }

    private async Task<Result<PaymentInitializationInternalModel>> InitiateCustomerDepositAsync(
        PaymentEntity payment,
        CancellationToken cancellationToken)
    {
        var configuredFields = PaymentMethodConfigReader.GetFields(payment.PaymentMethod!.Config);
        if (configuredFields.Count == 0)
        {
            throw new InvalidOperationException(
                localizer[LocalizationKeys.PaymentAkibaPayConfigMissingOnMethod, payment.PaymentMethod!.Code].Value);
        }
        ValidateRequiredConfiguredFields(configuredFields);

        var baseUrl = configuredFields["akibapay_base_url"];
        var merchantId = configuredFields["merchant_id"];
        var merchantPass = configuredFields["merchant_pass"];
        var callbackUrl = configuredFields["callback_url"];

        var rawFromNumber = akibaPayPaymentInitContext.FromNumber;
        if (string.IsNullOrWhiteSpace(rawFromNumber))
        {
            return Result<PaymentInitializationInternalModel>.Failure(
                localizer[LocalizationKeys.PaymentAkibaPayFromNumberRequired],
                AppHttpStatusCodes.BadRequest);
        }

        var fromNumber = AkibaPaySignatureHelper.NormalizeFromNumber(rawFromNumber);
        if (fromNumber.Length == 0)
        {
            return Result<PaymentInitializationInternalModel>.Failure(
                localizer[LocalizationKeys.PaymentAkibaPayFromNumberInvalid],
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
                localizer[LocalizationKeys.PaymentAkibaPayPaymentRefRequired],
                AppHttpStatusCodes.BadRequest);
        }

        // Same as other top-ups (e.g. Interswitch txn_ref): our internal PaymentRef is the PSP merchant reference.
        var merchantReference = payment.PaymentRef.Trim();

        var amountForAkibaPay = AkibaPaySignatureHelper.NormalizeAmount(payment.Amount);

        var (hash, signQueryString) = AkibaPaySignatureHelper.ComputeCustomerDepositHash(
            merchantId,
            merchantPass,
            fromNumber,
            amountForAkibaPay,
            currencyCode,
            callbackUrl,
            merchantReference);

        logger.LogInformation(
            "AkibaPay CustomerDeposit hash computed. PaymentId={PaymentId}, AmountForSign={AmountForSign}, SignQueryString={SignQueryString}, Hash={Hash}",
            payment.Id,
            amountForAkibaPay,
            RedactMerchantPassInSignQueryString(signQueryString),
            hash);

        var request = new AkibaPayCustomerDepositRequestModel
        {
            MerchantId = merchantId,
            MerchantPass = merchantPass,
            FromNumber = fromNumber,
            ToNumber = string.Empty,
            Amount = amountForAkibaPay,
            Currency = currencyCode,
            CallbackUrl = callbackUrl,
            MerchantReference = merchantReference,
            Hash = hash
        };

        var requestBodyForLog = SerializeRequestForLog(request);
        logger.LogInformation(
            "AkibaPay CustomerDeposit initiate request. PaymentId={PaymentId}, Url={Url}, Body={RequestBody}",
            payment.Id,
            baseUrl,
            requestBodyForLog);

        var response = await akibaPayHttpClient.PostCustomerDepositAsync(
            baseUrl,
            request,
            merchantId,
            cancellationToken).ConfigureAwait(false);

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var statusCode = ((int)response.StatusCode).ToString(CultureInfo.InvariantCulture);

        logger.LogInformation(
            "AkibaPay CustomerDeposit initiate response. PaymentId={PaymentId}, StatusCode={StatusCode}, Body={ResponseBody}",
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
                "AkibaPay customer deposit initiate failed. PaymentId={PaymentId}, StatusCode={StatusCode}, Body={ResponseBody}",
                payment.Id,
                statusCode,
                responseBody);

            if (responseBody.Contains("INVALID_PHONE_NUMBER", StringComparison.OrdinalIgnoreCase) ||
                responseBody.Contains("invalid-phone-number", StringComparison.OrdinalIgnoreCase))
            {
                return Result<PaymentInitializationInternalModel>.Failure(
                    localizer[LocalizationKeys.PaymentAkibaPayFromNumberInvalid],
                    AppHttpStatusCodes.BadRequest);
            }

            return Result<PaymentInitializationInternalModel>.Failure(
                localizer[LocalizationKeys.PaymentAkibaPayInitiateHttpFailed],
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
                    localizer[LocalizationKeys.PaymentAkibaPayStatusLookupPaymentRefRequired],
                    AppHttpStatusCodes.BadRequest,
                    result);
            }

            var managementCredentials = PaymentMethodConfigReader.ResolveAkibaPayManagementCredentials(
                context.PaymentMethod.Config,
                akibaPayOptions.Value);

            var merchantReference = context.PaymentTransactionReference.Trim();
            Result<PaymentCheckStatusInternalModel>? lastLookupResult = null;
            for (var attempt = 0; attempt < 2; attempt++)
            {
                if (attempt > 0)
                {
                    akibaPayManagementTokenProvider.InvalidateToken(
                        managementCredentials.ManagementBaseUrl,
                        managementCredentials.Login);
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
                "An error occurred while checking AkibaPay transaction status for payment {PaymentId}.",
                context.PaymentId);
            return Result<PaymentCheckStatusInternalModel>.Failure(
                localizer[LocalizationKeys.PaymentAkibaPayStatusCheckUnexpectedError],
                AppHttpStatusCodes.Failed,
                result);
        }
    }

    private async Task<Result<PaymentCheckStatusInternalModel>> LookupManagementTransactionStatusAsync(
        PaymentMethodConfigReader.AkibaPayManagementCredentials managementCredentials,
        string merchantReference,
        PaymentTransactionStatusContextInternalModel context,
        PaymentCheckStatusInternalModel result)
    {
        var tokenResult = await akibaPayManagementTokenProvider
            .GetTokenAsync(
                managementCredentials.ManagementBaseUrl,
                managementCredentials.Login,
                managementCredentials.Password)
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
            method = "GET",
            url = $"{managementCredentials.ManagementBaseUrl.TrimEnd('/')}/api/transaction/status/{merchantReference}",
            merchantReference
        });
        result.RequestBody = statusRequestBody;
        logger.LogInformation(
            "AkibaPay management transaction status request. PaymentId={PaymentId}, MerchantReference={MerchantReference}",
            context.PaymentId,
            merchantReference);
        LogTransactionStatusRequest(context.PaymentId, statusRequestBody);

        var statusResponse = await akibaPayHttpClient
            .GetTransactionStatusAsync(managementCredentials.ManagementBaseUrl, tokenResult.Value, merchantReference)
            .ConfigureAwait(false);
        var statusResponseBody = await statusResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
        result.ResponseBody = statusResponseBody;
        result.ResponseCode = ((int)statusResponse.StatusCode).ToString(CultureInfo.InvariantCulture);
        LogTransactionStatusResponse(context.PaymentId, result.ResponseCode, statusResponseBody);

        if (statusResponse.StatusCode == HttpStatusCode.Unauthorized)
        {
            logger.LogWarning(
                "AkibaPay management transaction status returned 401. PaymentId={PaymentId}, MerchantReference={MerchantReference}",
                context.PaymentId,
                merchantReference);

            return Result<PaymentCheckStatusInternalModel>.Failure(
                localizer[LocalizationKeys.PaymentAkibaPayStatusHttpFailed, result.ResponseCode],
                AppHttpStatusCodes.Unauthorized,
                result);
        }

        if (statusResponse.StatusCode == HttpStatusCode.NotFound)
        {
            result.PaymentExternalRef = context.PaymentExternalReference ?? string.Empty;

            if (string.IsNullOrWhiteSpace(statusResponseBody))
            {
                logger.LogWarning(
                    "AkibaPay management transaction status returned 404 with empty response body; treating payment as failed. PaymentId={PaymentId}, MerchantReference={MerchantReference}",
                    context.PaymentId,
                    merchantReference);

                result.Status = PaymentStatus.Failed;
                result.UserMessage = localizer[LocalizationKeys.PaymentAkibaPayStatusNotFoundFailed].Value;
                return Result<PaymentCheckStatusInternalModel>.Success(result);
            }

            result.ResponseMessage = statusResponseBody.Trim();

            if (PspTransactionStatusNotFoundPolicy.ShouldMarkFailed(context))
            {
                logger.LogWarning(
                    "AkibaPay management transaction status returned 404 with response body; treating payment as failed. PaymentId={PaymentId}, MerchantReference={MerchantReference}, Attempt={Attempt}/{TotalAttempts}, Body={ResponseBody}",
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
                    "AkibaPay management transaction status returned 404 with response body; deferring failure (transaction may not be indexed yet). PaymentId={PaymentId}, MerchantReference={MerchantReference}, Attempt={Attempt}/{TotalAttempts}, FailAfterAttempts={FailAfterAttempts}, Body={ResponseBody}",
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

        if (statusResponse.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.InternalServerError)
        {
            logger.LogWarning(
                "AkibaPay management transaction status returned {StatusCode}; treating payment as failed. PaymentId={PaymentId}, MerchantReference={MerchantReference}, Body={ResponseBody}",
                (int)statusResponse.StatusCode,
                context.PaymentId,
                merchantReference,
                statusResponseBody);

            result.PaymentExternalRef = context.PaymentExternalReference ?? string.Empty;
            result.ResponseMessage = string.IsNullOrWhiteSpace(statusResponseBody)
                ? statusResponse.ReasonPhrase
                : statusResponseBody.Trim();
            result.Status = PaymentStatus.Failed;
            result.UserMessage = localizer[LocalizationKeys.PaymentAkibaPayStatusNotFoundFailed].Value;
            return Result<PaymentCheckStatusInternalModel>.Success(result);
        }

        if (statusResponse.StatusCode != HttpStatusCode.OK)
        {
            logger.LogWarning(
                "AkibaPay management transaction status request failed. PaymentId={PaymentId}, StatusCode={StatusCode}, Body={ResponseBody}",
                context.PaymentId,
                result.ResponseCode,
                statusResponseBody);

            return Result<PaymentCheckStatusInternalModel>.Failure(
                localizer[LocalizationKeys.PaymentAkibaPayStatusHttpFailed, result.ResponseCode],
                AppHttpStatusCodes.Failed,
                result);
        }

        var transactionStatus = JsonSerializer.Deserialize<AkibaPayTransactionStatusResponseModel>(
            statusResponseBody,
            LogJsonOptions);
        if (transactionStatus is null)
        {
            return Result<PaymentCheckStatusInternalModel>.Failure(
                localizer[LocalizationKeys.PaymentAkibaPayStatusParseFailed],
                AppHttpStatusCodes.Failed,
                result);
        }

        result.ResponseMessage = transactionStatus.TransactionStatus;
        result.Status = MapManagementTransactionStatus(transactionStatus.TransactionStatus);
        result.PaymentExternalRef = transactionStatus.AkibaReference ?? context.PaymentExternalReference ?? string.Empty;

        logger.LogInformation(
            "AkibaPay management transaction status response. PaymentId={PaymentId}, MerchantReference={MerchantReference}, TransactionStatus={TransactionStatus}, AkibaReference={AkibaReference}",
            context.PaymentId,
            merchantReference,
            transactionStatus.TransactionStatus,
            transactionStatus.AkibaReference);

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
                    "AkibaPay initiate audit skipped: no initiate PaymentTxn found for payment {PaymentId}.",
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

    private static string SerializeRequestForLog(AkibaPayCustomerDepositRequestModel request) =>
        JsonSerializer.Serialize(
            new
            {
                request.MerchantId,
                MerchantPass = "***",
                request.FromNumber,
                request.ToNumber,
                request.Amount,
                request.Currency,
                request.CallbackUrl,
                request.MerchantReference,
                Hash = "***"
            },
            LogJsonOptions);

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
                    localizer[LocalizationKeys.PaymentAkibaPayConfigFieldEmpty, key].Value);
            }
        }

        var baseUrl = fields["akibapay_base_url"].Trim();
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out _))
        {
            throw new InvalidOperationException(
                localizer[LocalizationKeys.PaymentAkibaPayConfigBaseUrlInvalid].Value);
        }
    }
}
