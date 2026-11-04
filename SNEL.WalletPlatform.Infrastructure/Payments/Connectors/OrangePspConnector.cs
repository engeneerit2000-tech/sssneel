using System.Collections.Concurrent;
using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SNEL.WalletPlatform.Application;
using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Interfaces.UnitOfWork;
using SNEL.WalletPlatform.Application.Payments;
using SNEL.WalletPlatform.Application.Payments.Configurations;
using SNEL.WalletPlatform.Application.Payments.Interfaces;
using SNEL.WalletPlatform.Application.Payments.Models.External.Orange;
using SNEL.WalletPlatform.Application.Payments.Models.Internal;
using SNEL.WalletPlatform.Application.Payments.Models.Output;
using SNEL.WalletPlatform.Domain.Payments;
using SNEL.WalletPlatform.Infrastructure.Payments.Clients;

namespace SNEL.WalletPlatform.Infrastructure.Payments.Connectors;

public sealed class OrangePspConnector(
    IUnitOfWork unitOfWork,
    OrangeHttpClient httpClient,
    IOrangePaymentInitContext initContext,
    IOptions<OrangeOptions> options,
    IStringLocalizer<SharedResource> localizer,
    ILogger<OrangePspConnector> logger) : PspConnector(unitOfWork, logger, localizer)
{
    private static readonly TimeSpan TokenExpirySkew = TimeSpan.FromSeconds(30);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };
    private static readonly ConcurrentDictionary<string, TokenEntry> TokenCache = new();
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> TokenLocks = new();
    public override string PaymentServiceProviderCode => "ORANGE";
    public override bool CreateTransactions => true;

    protected override async Task<Result<PaymentInitializationInternalModel>> HandleInitiatePayment(
        PaymentEntity payment,
        CancellationToken cancellationToken = default)
    {
        var paymentContext = await unitOfWork.PaymentRepository.GetByIdWithPspContextAsync(payment.Id);
        if (paymentContext?.PaymentMethod is null)
            return Result<PaymentInitializationInternalModel>.Failure("Orange payment method was not found.", AppHttpStatusCodes.NotFound);

        var credentials = PaymentMethodConfigReader.ResolveOrangeCredentials(paymentContext.PaymentMethod.Config, options.Value);
        var validation = ValidateCredentials(credentials);
        if (validation is not null)
            return Result<PaymentInitializationInternalModel>.Failure(validation, AppHttpStatusCodes.BadRequest);

        var isSandbox = credentials.Environment.Equals("sandbox", StringComparison.OrdinalIgnoreCase);
        var peerId = NormalizeMsisdn(isSandbox ? credentials.SandboxPeerId : initContext.FromNumber);
        if (string.IsNullOrWhiteSpace(peerId))
            return Result<PaymentInitializationInternalModel>.Failure(
                isSandbox
                    ? "Orange sandbox_peer_id is required for sandbox cashout."
                    : "FromNumber is required for Orange cashout.",
                AppHttpStatusCodes.BadRequest);
        if (string.IsNullOrWhiteSpace(paymentContext.PaymentRef))
            return Result<PaymentInitializationInternalModel>.Failure("Payment reference is required for Orange.", AppHttpStatusCodes.BadRequest);

        var country = ResolveCountry(credentials);
        var currency = ResolveCurrency(credentials, paymentContext.Currency?.Abbreviation);
        if (string.IsNullOrWhiteSpace(country) || string.IsNullOrWhiteSpace(currency))
            return Result<PaymentInitializationInternalModel>.Failure("Orange country and currency are required.", AppHttpStatusCodes.BadRequest);

        var request = new OrangeWithdrawRequestModel
        {
            PeerId = peerId,
            Amount = paymentContext.Amount,
            Currency = currency,
            TransactionId = paymentContext.PaymentRef
        };
        logger.LogInformation("Orange withdraw request. PaymentId={PaymentId}, Body={Body}",
            payment.Id, JsonSerializer.Serialize(request, JsonOptions));

        var token = await GetTokenAsync(credentials, cancellationToken);
        if (!token.IsSuccess)
            return token.ToFailure<PaymentInitializationInternalModel>();

        var response = await httpClient.WithdrawAsync(credentials, country, token.Value!, request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        logger.LogInformation("Orange withdraw response. PaymentId={PaymentId}, HttpStatus={Status}, Body={Body}",
            payment.Id, (int)response.StatusCode, body);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            InvalidateToken(credentials);
        if (response.StatusCode != HttpStatusCode.Accepted)
            return Result<PaymentInitializationInternalModel>.Failure(
                BuildOrangeError("Orange withdrawal was not accepted", response.StatusCode, body),
                response.StatusCode == HttpStatusCode.Unauthorized ? AppHttpStatusCodes.Unauthorized : AppHttpStatusCodes.Failed);

        var model = JsonSerializer.Deserialize<OrangeTransactionResponseModel>(body, JsonOptions);
        if (model is null || !string.Equals(model.Status, "PENDING", StringComparison.OrdinalIgnoreCase))
            return Result<PaymentInitializationInternalModel>.Failure(
                $"Orange HTTP 202 response did not contain PENDING status: {model?.Message ?? body}", AppHttpStatusCodes.Failed);

        if (!string.IsNullOrWhiteSpace(model.TransactionData?.TxnId))
        {
            paymentContext.SetExternalRef(model.TransactionData.TxnId.Trim());
            unitOfWork.PaymentRepository.Update(paymentContext);
            await unitOfWork.CompleteAsync();
        }

        return Result<PaymentInitializationInternalModel>.Success(new(
            PaymentNextActionEnum.None,
            new NoActionNextActionModel("Orange withdrawal request submitted.")));
    }

    protected override async Task<Result<PaymentCheckStatusInternalModel>> HandleGetTransactionStatus(
        PaymentTransactionStatusContextInternalModel context)
    {
        var result = new PaymentCheckStatusInternalModel();
        try
        {
            var credentials = PaymentMethodConfigReader.ResolveOrangeCredentials(context.PaymentMethod.Config, options.Value);
            var validation = ValidateCredentials(credentials);
            if (validation is not null)
                return Result<PaymentCheckStatusInternalModel>.Failure(validation, AppHttpStatusCodes.BadRequest, result);

            var transactionId = context.PaymentTransactionReference?.Trim();
            if (string.IsNullOrWhiteSpace(transactionId))
                return Result<PaymentCheckStatusInternalModel>.Failure("Orange transactionId is required.", AppHttpStatusCodes.BadRequest, result);

            var country = ResolveCountry(credentials);
            result.RequestBody = JsonSerializer.Serialize(new { country, transactionId });
            LogTransactionStatusRequest(context.PaymentId, result.RequestBody);

            var token = await GetTokenAsync(credentials);
            if (!token.IsSuccess)
                return Result<PaymentCheckStatusInternalModel>.Failure(token.Message!, token.StatusCode, result);

            var response = await httpClient.GetStatusAsync(credentials, country, transactionId, token.Value!);
            result.ResponseBody = await response.Content.ReadAsStringAsync();
            result.ResponseCode = ((int)response.StatusCode).ToString(CultureInfo.InvariantCulture);
            LogTransactionStatusResponse(context.PaymentId, result.ResponseCode, result.ResponseBody);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                InvalidateToken(credentials);
                return Result<PaymentCheckStatusInternalModel>.Failure(
                    "Orange OAuth token is invalid or expired; it was invalidated for the next request.",
                    AppHttpStatusCodes.Unauthorized,
                    result);
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                result.Status = PaymentStatus.Failed;
                result.ResponseMessage = "Orange transaction does not exist. Do not retry this transactionId.";
                return Result<PaymentCheckStatusInternalModel>.Success(result);
            }

            if (response.StatusCode != HttpStatusCode.OK)
                return Result<PaymentCheckStatusInternalModel>.Failure(
                    BuildOrangeError("Orange status request failed", response.StatusCode, result.ResponseBody),
                    AppHttpStatusCodes.Failed,
                    result);

            var model = JsonSerializer.Deserialize<OrangeTransactionResponseModel>(result.ResponseBody, JsonOptions);
            if (model is null)
                return Result<PaymentCheckStatusInternalModel>.Failure("Invalid Orange status response.", AppHttpStatusCodes.Failed, result);

            result.ResponseMessage = model.Message ?? string.Empty;
            result.PaymentExternalRef = model.TransactionData?.TxnId;
            result.Status = MapStatus(model.Status);
            return Result<PaymentCheckStatusInternalModel>.Success(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Orange status enquiry failed for payment {PaymentId}.", context.PaymentId);
            return Result<PaymentCheckStatusInternalModel>.Failure("Orange status enquiry failed.", AppHttpStatusCodes.Failed, result);
        }
    }

    internal static PaymentStatus MapStatus(string? status) => NormalizeStatus(status) switch
    {
        "SUCCESS" or "SUCCESSFUL" or "COMPLETED" => PaymentStatus.Completed,
        "FAILED" or "FAILURE" or "REJECTED" or "CANCELLED" or "EXPIRED" => PaymentStatus.Failed,
        _ => PaymentStatus.Processing
    };

    private static string NormalizeStatus(string? status) => (status ?? string.Empty).Trim().ToUpperInvariant();
    private static string NormalizeMsisdn(string? value) => new((value ?? string.Empty).Where(char.IsDigit).ToArray());
    private static string ResolveCountry(PaymentMethodConfigReader.OrangeCredentials credentials) =>
        credentials.Environment.Equals("sandbox", StringComparison.OrdinalIgnoreCase)
            ? "sx"
            : credentials.Country.Trim().ToLowerInvariant();
    private static string ResolveCurrency(PaymentMethodConfigReader.OrangeCredentials credentials, string? paymentCurrency) =>
        credentials.Environment.Equals("sandbox", StringComparison.OrdinalIgnoreCase)
            ? "OUV"
            : (string.IsNullOrWhiteSpace(credentials.Currency) ? paymentCurrency : credentials.Currency)?.Trim().ToUpperInvariant() ?? string.Empty;

    private static string? ValidateCredentials(PaymentMethodConfigReader.OrangeCredentials credentials)
    {
        if (!Uri.TryCreate(credentials.BaseUrl, UriKind.Absolute, out _)) return "Orange base URL is invalid.";
        if (string.IsNullOrWhiteSpace(credentials.TokenPath)) return "Orange token_path is not configured.";
        if (string.IsNullOrWhiteSpace(credentials.WithdrawPath)) return "Orange withdraw_path is not configured.";
        if (string.IsNullOrWhiteSpace(credentials.StatusPath)) return "Orange status_path is not configured.";
        if (string.IsNullOrWhiteSpace(credentials.BasicAuth)) return "Orange basic_auth is not configured.";
        return null;
    }

    private async Task<Result<string>> GetTokenAsync(
        PaymentMethodConfigReader.OrangeCredentials credentials,
        CancellationToken cancellationToken = default)
    {
        var key = GetTokenKey(credentials);
        if (TokenCache.TryGetValue(key, out var cached) && cached.ExpiresAt > DateTimeOffset.UtcNow)
            return Result<string>.Success(cached.Token);

        var gate = TokenLocks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(cancellationToken);
        try
        {
            if (TokenCache.TryGetValue(key, out cached) && cached.ExpiresAt > DateTimeOffset.UtcNow)
                return Result<string>.Success(cached.Token);

            var response = await httpClient.RequestTokenAsync(credentials, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            if (response.StatusCode != HttpStatusCode.OK)
                return Result<string>.Failure(
                    BuildOrangeError("Orange token request failed", response.StatusCode, body),
                    AppHttpStatusCodes.Failed);

            var token = JsonSerializer.Deserialize<OrangeTokenResponseModel>(body, JsonOptions);
            if (string.IsNullOrWhiteSpace(token?.AccessToken))
                return Result<string>.Failure("Orange token response has no access_token.", AppHttpStatusCodes.Failed);

            var lifetime = TimeSpan.FromSeconds(token.ExpiresIn > 0 ? token.ExpiresIn : 3600);
            cached = new TokenEntry(token.AccessToken, DateTimeOffset.UtcNow.Add(lifetime).Subtract(TokenExpirySkew));
            TokenCache[key] = cached;
            logger.LogInformation("Orange OAuth token cached until {ExpiresAt}.", cached.ExpiresAt);
            return Result<string>.Success(cached.Token);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Orange token acquisition failed.");
            return Result<string>.Failure("Orange token acquisition failed.", AppHttpStatusCodes.Failed);
        }
        finally
        {
            gate.Release();
        }
    }

    private static void InvalidateToken(PaymentMethodConfigReader.OrangeCredentials credentials) =>
        TokenCache.TryRemove(GetTokenKey(credentials), out _);

    private static string GetTokenKey(PaymentMethodConfigReader.OrangeCredentials credentials) =>
        $"{credentials.BaseUrl}|{credentials.TokenPath}|{credentials.BasicAuth}";

    private static string BuildOrangeError(string prefix, HttpStatusCode statusCode, string body)
    {
        try
        {
            var error = JsonSerializer.Deserialize<OrangeErrorResponseModel>(body, JsonOptions);
            var normalizedCode = error?.Code?.ToString().Trim('"');
            var code = string.IsNullOrWhiteSpace(normalizedCode) ? string.Empty : $" Code={normalizedCode}.";
            var message = string.IsNullOrWhiteSpace(error?.Message) ? body : error.Message;
            var guidance = GetOrangeErrorGuidance(statusCode, normalizedCode);
            return $"{prefix} ({(int)statusCode}).{code} {message} {guidance}".Trim();
        }
        catch (JsonException)
        {
            return $"{prefix} ({(int)statusCode}): {body}";
        }
    }

    private static string GetOrangeErrorGuidance(HttpStatusCode statusCode, string? code)
    {
        if ((int)statusCode == 429)
            return "The transaction was not processed. Back off, then submit a new request with a new transactionId; do not poll this transaction.";

        return code switch
        {
            "10" => "The subscription is pending activation. Wait for Orange confirmation and do not retry.",
            "23" => "A required request field is missing or invalid. Validate the request against the Orange API contract.",
            "30" => "Verify the amount unit and that the currency matches the configured country.",
            "51" => "Verify that the Orange contract is correctly associated with the API credentials.",
            "70" => "This transaction service is not enabled in the Orange contract; contact Orange to update the contract.",
            _ => string.Empty
        };
    }

    private sealed record TokenEntry(string Token, DateTimeOffset ExpiresAt);
}
