using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SNEL.WalletPlatform.Application;
using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Interfaces.UnitOfWork;
using SNEL.WalletPlatform.Application.Payments;
using SNEL.WalletPlatform.Application.Payments.Configurations;
using SNEL.WalletPlatform.Application.Payments.Interfaces;
using SNEL.WalletPlatform.Application.Payments.Models.External.Mpesa;
using SNEL.WalletPlatform.Application.Payments.Models.Internal;
using SNEL.WalletPlatform.Application.Payments.Models.Output;
using SNEL.WalletPlatform.Domain.Payments;
using SNEL.WalletPlatform.Infrastructure.Payments.Clients;
using System.Globalization;
using System.Net;
using System.Text.Json;
using ZXing;

namespace SNEL.WalletPlatform.Infrastructure.Payments.Connectors;

public sealed class MpesaPspConnector(
    IUnitOfWork unitOfWork,
    MpesaHttpClient httpClient,
    IMpesaSessionProvider sessionProvider,
    IMpesaPaymentInitContext initContext,
    IOptions<MpesaOptions> options,
    IStringLocalizer<SharedResource> localizer,
    ILogger<MpesaPspConnector> logger) : PspConnector(unitOfWork, logger, localizer)
{
    private Dictionary<string, string> SandboxTransactionStatuses = new Dictionary<string, string>()
    {
        { "SUCCESSFUL", "000000000001" },
        { "ERROR_1", "000000000002" },
        { "ERROR_2", "000000000003" },
        { "ERROR_3", "000000000003" }
    };

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    public override string PaymentServiceProviderCode => "MPESA";
    public override bool CreateTransactions => true;

    protected override async Task<Result<PaymentInitializationInternalModel>> HandleInitiatePayment(
        PaymentEntity payment,
        CancellationToken cancellationToken = default)
    {
        var paymentContext = await unitOfWork.PaymentRepository.GetByIdWithPspContextAsync(payment.Id);
        if (paymentContext?.PaymentMethod is null)
            return Result<PaymentInitializationInternalModel>.Failure("MPesa payment method was not found.", AppHttpStatusCodes.NotFound);

        var credentials = PaymentMethodConfigReader.ResolveMpesaCredentials(paymentContext.PaymentMethod.Config, options.Value);
        var validation = ValidateCredentials(credentials);
        if (validation is not null)
            return Result<PaymentInitializationInternalModel>.Failure(validation, AppHttpStatusCodes.BadRequest);

        var msisdn = NormalizeMsisdn(initContext.FromNumber);
        if (string.IsNullOrWhiteSpace(msisdn))
            return Result<PaymentInitializationInternalModel>.Failure("FromNumber is required for MPesa C2B payment.", AppHttpStatusCodes.BadRequest);

        if (string.IsNullOrWhiteSpace(paymentContext.PaymentRef))
            return Result<PaymentInitializationInternalModel>.Failure("Payment reference is required for MPesa.", AppHttpStatusCodes.BadRequest);

        var currency = paymentContext.Currency?.Abbreviation;
        if (string.IsNullOrWhiteSpace(currency))
            return Result<PaymentInitializationInternalModel>.Failure("Payment currency was not found.", AppHttpStatusCodes.BadRequest);

        var request = new MpesaC2bPaymentRequestModel
        {
            Amount = paymentContext.Amount.ToString("0.##", CultureInfo.InvariantCulture),
            Country = credentials.Country,
            Currency = currency.ToUpperInvariant(),
            CustomerMsisdn = msisdn,
            ServiceProviderCode = credentials.ServiceProviderCode,
            ThirdPartyConversationId = paymentContext.PaymentRef,
            TransactionReference = paymentContext.PaymentRef,
            PurchasedItemsDescription = credentials.PurchasedItemsDescription
        };
        var requestBody = JsonSerializer.Serialize(request, JsonOptions);
        logger.LogInformation("MPesa C2B request. PaymentId={PaymentId}, Body={Body}", payment.Id, requestBody);

        var session = await sessionProvider.GetSessionAsync(credentials, cancellationToken);
        if (!session.IsSuccess)
            return session.ToFailure<PaymentInitializationInternalModel>();

        var response = await httpClient.PostC2bPaymentAsync(credentials, session.Value!, request, cancellationToken);
        var ResponseCode = ((int)response.StatusCode).ToString(CultureInfo.InvariantCulture);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        logger.LogInformation("MPesa C2B response. PaymentId={PaymentId}, HttpStatus={Status}, Body={Body}", payment.Id, (int)response.StatusCode, body);
        var providerResponseCode = NormalizeResponseCode(ResponseCode);
        if (response.StatusCode == HttpStatusCode.Unauthorized && providerResponseCode != "INS-6")
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            sessionProvider.Invalidate(credentials);
        if (!response.IsSuccessStatusCode)
            return Result<PaymentInitializationInternalModel>.Failure($"MPesa payment initiation failed ({(int)response.StatusCode}): {body}", AppHttpStatusCodes.Failed);

        var model = JsonSerializer.Deserialize<MpesaC2bPaymentResponseModel>(body, JsonOptions);
        if (!string.IsNullOrWhiteSpace(model?.ResponseCode) &&
            !model.ResponseCode.Equals("INS-0", StringComparison.OrdinalIgnoreCase))
        {
            return Result<PaymentInitializationInternalModel>.Failure(
                $"MPesa rejected the payment: {model.ResponseCode} - {model.ResponseDescription}",
                AppHttpStatusCodes.Failed);
        }
        if (!string.IsNullOrWhiteSpace(model?.TransactionId))
        {
            paymentContext.SetExternalRef(model.TransactionId.Trim());
            unitOfWork.PaymentRepository.Update(paymentContext);
            await unitOfWork.CompleteAsync();
        }

        return Result<PaymentInitializationInternalModel>.Success(new(
            PaymentNextActionEnum.None,
            new NoActionNextActionModel("MPesa payment request submitted.")));
    }

    protected override async Task<Result<PaymentCheckStatusInternalModel>> HandleGetTransactionStatus(
        PaymentTransactionStatusContextInternalModel context)
    {         
        var result = new PaymentCheckStatusInternalModel();
        try
        {            
            var credentials = PaymentMethodConfigReader.ResolveMpesaCredentials(context.PaymentMethod.Config, options.Value);
            var validation = ValidateCredentials(credentials);
            if (validation is not null)
                return Result<PaymentCheckStatusInternalModel>.Failure(validation, AppHttpStatusCodes.BadRequest, result);            
            var conversationId = context.PaymentTransactionReference?.Trim();
            var queryReference = context.PaymentExternalReference?.Trim();
            if (credentials.Environment == "sandbox")
            {
                queryReference = SandboxTransactionStatuses.FirstOrDefault(x => x.Key.Equals("SUCCESSFUL", StringComparison.OrdinalIgnoreCase)).Value;
            }
            if (string.IsNullOrWhiteSpace(conversationId) || string.IsNullOrWhiteSpace(queryReference))
                return Result<PaymentCheckStatusInternalModel>.Failure("MPesa transaction and conversation references are required.", AppHttpStatusCodes.BadRequest, result);

            var query = new Dictionary<string, string>
            {
                ["input_QueryReference"] = queryReference,
                ["input_ServiceProviderCode"] = credentials.ServiceProviderCode,
                ["input_ThirdPartyConversationID"] = conversationId,
                ["input_Country"] = credentials.Country
            };
            result.RequestBody = JsonSerializer.Serialize(query, JsonOptions);
            LogTransactionStatusRequest(context.PaymentId, result.RequestBody);

            var session = await sessionProvider.GetSessionAsync(credentials);
            if (!session.IsSuccess)
                return Result<PaymentCheckStatusInternalModel>.Failure(session.Message!, session.StatusCode, result);

            var response = await httpClient.QueryTransactionStatusAsync(credentials, session.Value!, query);
            result.ResponseBody = await response.Content.ReadAsStringAsync();
            result.ResponseCode = ((int)response.StatusCode).ToString(CultureInfo.InvariantCulture);
            LogTransactionStatusResponse(context.PaymentId, result.ResponseCode, result.ResponseBody);
            var model = JsonSerializer.Deserialize<MpesaStatusResponseModel>(result.ResponseBody, JsonOptions);
            if (model is null)
                return Result<PaymentCheckStatusInternalModel>.Failure("Invalid MPesa status response.", AppHttpStatusCodes.Failed, result);

            var providerResponseCode = NormalizeResponseCode(model.ResponseCode);
            if (response.StatusCode == HttpStatusCode.Unauthorized && providerResponseCode != "INS-6")
                sessionProvider.Invalidate(credentials);
            result.ResponseCode = string.IsNullOrWhiteSpace(providerResponseCode)
                ? result.ResponseCode
                : providerResponseCode;
            result.PaymentExternalRef = model.TransactionId;
            result.ResponseMessage = model.ResponseDescription ?? string.Empty;

            if (providerResponseCode == "INS-23")
            {
                result.Status = PspTransactionStatusNotFoundPolicy.ShouldMarkFailed(context)
                    ? PaymentStatus.Failed
                    : PaymentStatus.Processing;
                return Result<PaymentCheckStatusInternalModel>.Success(result);
            }

            if (KnownFailureResponseCodes.Contains(providerResponseCode))
            {
                result.Status = PaymentStatus.Failed;
                return Result<PaymentCheckStatusInternalModel>.Success(result);
            }

            if (providerResponseCode == "INS-9")
            {
                result.Status = PaymentStatus.Processing;
                return Result<PaymentCheckStatusInternalModel>.Success(result);
            }

            if (!response.IsSuccessStatusCode || providerResponseCode != "INS-0")
            {
                return Result<PaymentCheckStatusInternalModel>.Failure(
                    $"MPesa status request failed: {result.ResponseCode} - {result.ResponseMessage}",
                    AppHttpStatusCodes.Failed,
                    result);
            }

            result.Status = MapStatus(model.TransactionStatus, providerResponseCode, model.ResponseDescription);
            return Result<PaymentCheckStatusInternalModel>.Success(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "MPesa status enquiry failed for payment {PaymentId}.", context.PaymentId);
            return Result<PaymentCheckStatusInternalModel>.Failure("MPesa status enquiry failed.", AppHttpStatusCodes.Failed, result);
        }
    }

    internal static PaymentStatus MapStatus(string? status, string? responseCode, string? description)
    {
        var value = $"{status} {responseCode} {description}".ToUpperInvariant();
        if (value.Contains("FAILED") || value.Contains("CANCEL") || value.Contains("REJECT") || value.Contains("EXPIRED"))
            return PaymentStatus.Failed;
        if (value.Contains("COMPLETED") || value.Contains("SUCCESS"))
            return PaymentStatus.Completed;
        if (string.IsNullOrWhiteSpace(status) &&
            responseCode?.Equals("INS-0", StringComparison.OrdinalIgnoreCase) == true)
            return PaymentStatus.Completed;
        return PaymentStatus.Processing;
    }

    private static readonly HashSet<string> KnownFailureResponseCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        "INS-1",
        "INS-6",
        "INS-13",
        "INS-20",
        "INS-21",
        "INS-28",
        "INS-996",
        "INS-997",
        "INS-998"
    };

    private static string NormalizeResponseCode(string? responseCode) =>
        (responseCode ?? string.Empty).Trim().ToUpperInvariant();

    private static string NormalizeMsisdn(string? value) =>
        new((value ?? string.Empty).Where(char.IsDigit).ToArray());

    private static string? ValidateCredentials(PaymentMethodConfigReader.MpesaCredentials credentials)
    {
        if (!Uri.TryCreate(credentials.BaseUrl, UriKind.Absolute, out _)) return "MPesa base URL is invalid.";
        if (string.IsNullOrWhiteSpace(credentials.PublicKey)) return "MPesa public_key is not configured.";
        if (string.IsNullOrWhiteSpace(credentials.ApiKey)) return "MPesa api_key is not configured.";
        if (string.IsNullOrWhiteSpace(credentials.ServiceProviderCode)) return "MPesa service_provider_code is not configured.";
        return null;
    }
}
