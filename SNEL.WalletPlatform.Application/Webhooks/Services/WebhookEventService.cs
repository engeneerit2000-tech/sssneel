using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SNEL.WalletPlatform.Application.Interfaces.UnitOfWork;
using SNEL.WalletPlatform.Application.Payments.Interfaces;
using SNEL.WalletPlatform.Application.Webhooks.Interfaces;
using SNEL.WalletPlatform.Application.Webhooks.Models;
using SNEL.WalletPlatform.Application.Webhooks.Models.Input;
using SNEL.WalletPlatform.Domain.Webhooks;
using SNEL.WalletPlatform.Domain.Payments;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Globalization;

namespace SNEL.WalletPlatform.Application.Webhooks.Services;

public sealed class WebhookEventService(
    IUnitOfWork unitOfWork,
    IPaymentService paymentService,
    IConfiguration configuration,
    ILogger<WebhookEventService> logger) : IWebhookEventService
{
    private static readonly JsonSerializerOptions AkibaPayWebhookJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly JsonSerializerOptions AfrimonyWebhookJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly JsonSerializerOptions MpesaWebhookJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task CreateInterswitchWebhookEventAsync(string rawBody, string? signature)
    {
        logger.LogInformation("WEBHOOK RECEIVED: Event: {Payload}, Signature: {Signature}", rawBody, signature);
        try
        {
            var inputModel = JsonSerializer.Deserialize<InterswitchWebhookEventInputModel>(
                rawBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Create the entity using our factory method
            var transactionEvent = InterswitchWebhookEventEntity.Create(
                @event: inputModel.Event,
                uuid: inputModel.Uuid,
                timestamp: inputModel.Timestamp,
                remittanceAmount: inputModel.Data.RemittanceAmount,
                bankCode: inputModel.Data.BankCode,
                amount: inputModel.Data.Amount,
                paymentReference: inputModel.Data.PaymentReference,
                channel: inputModel.Data.Channel,
                splitAccounts: inputModel.Data.SplitAccounts,
                retrievalReferenceNumber: inputModel.Data.RetrievalReferenceNumber,
                transactionDate: inputModel.Data.TransactionDate,
                accountNumber: inputModel.Data.AccountNumber,
                responseCode: inputModel.Data.ResponseCode,
                token: inputModel.Data.Token,
                responseDescription: inputModel.Data.ResponseDescription,
                paymentId: inputModel.Data.PaymentId,
                merchantCustomerId: inputModel.Data.MerchantCustomerId,
                escrow: inputModel.Data.Escrow,
                merchantReference: inputModel.Data.MerchantReference,
                currencyCode: inputModel.Data.CurrencyCode,
                merchantCustomerName: inputModel.Data.MerchantCustomerName,
                cardNumber: inputModel.Data.CardNumber,
                signature: signature);

            // Check for duplicates
            var exists = await unitOfWork.InterswitchWebhookEventRepository.AnyByUuidAsync(transactionEvent.Uuid);
            if (exists)
            {
                logger.LogInformation("WEBHOOK DUPLICATE: Event {Uuid}, Status: {Status}, Signature: {Signature}",
                    transactionEvent.Uuid, transactionEvent.Status, signature);
                transactionEvent.ChangeStatus(WebhookEventStatus.Duplicate);
            }
            else
            {
                // Verify signature existence
                if (string.IsNullOrEmpty(signature))
                {
                    logger.LogWarning("WEBHOOK NO-SIGN: Event {Uuid}, Status: {Status}, Signature: {Signature}",
                        transactionEvent.Uuid, transactionEvent.Status, signature);

                    transactionEvent.ChangeStatus(WebhookEventStatus.SignatureMissing);
                }
                else
                {
                    // Verify signature validity
                    var isValidSignature = VerifyInterswitchSignature(rawBody, signature);
                    if (!isValidSignature)
                    {
                        logger.LogWarning("WEBHOOK INVALID_SIGN:  Event {Uuid}, Status: {Status}, Signature: {Signature}",
                            transactionEvent.Uuid, transactionEvent.Status, signature);

                        transactionEvent.ChangeStatus(WebhookEventStatus.InvalidSignature);
                    }
                    else
                    {
                        logger.LogWarning("WEBHOOK VALID_SIGN:  Event {Uuid}, Status: {Status}, Signature: {Signature}",
                            transactionEvent.Uuid, transactionEvent.Status, signature);
                        transactionEvent.ChangeStatus(WebhookEventStatus.Pending);
                    }
                }
            }

            // Save to database
            await unitOfWork.InterswitchWebhookEventRepository.AddAsync(transactionEvent);
            await unitOfWork.CompleteAsync();

            logger.LogInformation("WEBHOOK SAVED: Event {Uuid}, Status: {Status}, Signature: {Signature}",
                transactionEvent.Uuid, transactionEvent.Status, signature);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "WEBHOOK ERROR: Critical error processing webhook - {Message}", ex.Message);
        }
    }

    public async Task<AkibaPayWebhookHandleResult> CreateAkibaPayWebhookEventAsync(string rawBody)
    {
        try
        {
            var inputModel = JsonSerializer.Deserialize<AkibaPayWebhookEventInputModel>(
                rawBody,
                AkibaPayWebhookJsonOptions);

            if (inputModel is null ||
                string.IsNullOrWhiteSpace(inputModel.AkibaReference) ||
                string.IsNullOrWhiteSpace(inputModel.MerchantReference) ||
                string.IsNullOrWhiteSpace(inputModel.TransactionStatus))
            {
                logger.LogWarning("AKIBAPAY WEBHOOK PARSE_FAILED: invalid payload. Body: {Body}", rawBody);
                return AkibaPayWebhookHandleResult.ParseFailed("Invalid or incomplete AkibaPay webhook payload.");
            }

            var webhookEvent = AkibaPayWebhookEventEntity.Create(
                akibaReference: inputModel.AkibaReference,
                merchantReference: inputModel.MerchantReference,
                transactionStatus: inputModel.TransactionStatus,
                toNumberTelecomReference: inputModel.ToNumberTelecomReference,
                fromNumberTelecomReference: inputModel.FromNumberTelecomReference,
                amount: inputModel.Amount,
                currency: inputModel.Currency,
                toNumber: inputModel.ToNumber,
                fromNumber: inputModel.FromNumber,
                createdAt: inputModel.CreatedAt,
                updatedAt: inputModel.UpdatedAt,
                callbackGeneratedAt: inputModel.CallbackGeneratedAt);

            //var isDuplicate = await unitOfWork.AkibaPayWebhookEventRepository.AnyByAkibaReferenceAsync(
            //    webhookEvent.AkibaReference);
            //if (isDuplicate)
            //{
            //    webhookEvent.ChangeStatus(WebhookEventStatus.Duplicate);
            //    await unitOfWork.AkibaPayWebhookEventRepository.AddAsync(webhookEvent);
            //    await unitOfWork.CompleteAsync();
            //    logger.LogInformation(
            //        "AKIBAPAY WEBHOOK DUPLICATE: AkibaReference={AkibaReference}, MerchantReference={MerchantReference}",
            //        webhookEvent.AkibaReference,
            //        webhookEvent.MerchantReference);
            //    return AkibaPayWebhookHandleResult.Duplicate(
            //        webhookEvent.AkibaReference,
            //        webhookEvent.MerchantReference);
            //}

            var payment = await unitOfWork.PaymentRepository.GetByPaymentRefWithApplyContextAsync(
                webhookEvent.MerchantReference);
            if (payment is null)
            {
                webhookEvent.ChangeStatus(WebhookEventStatus.Failed);
                await unitOfWork.AkibaPayWebhookEventRepository.AddAsync(webhookEvent);
                await unitOfWork.CompleteAsync();
                logger.LogWarning(
                    "AKIBAPAY WEBHOOK PAYMENT_NOT_FOUND: MerchantReference={MerchantReference}, AkibaReference={AkibaReference}",
                    webhookEvent.MerchantReference,
                    webhookEvent.AkibaReference);
                return AkibaPayWebhookHandleResult.PaymentNotFound(
                    webhookEvent.AkibaReference,
                    webhookEvent.MerchantReference);
            }

            if (payment.Status == PaymentStatus.Completed &&
                string.Equals(webhookEvent.TransactionStatus, "SUCCESS", StringComparison.OrdinalIgnoreCase))
            {
                webhookEvent.ChangeStatus(WebhookEventStatus.Processed);
                await unitOfWork.AkibaPayWebhookEventRepository.AddAsync(webhookEvent);
                await unitOfWork.CompleteAsync();
                logger.LogInformation(
                    "AKIBAPAY WEBHOOK NOOP_ALREADY_COMPLETED: PaymentId={PaymentId}, AkibaReference={AkibaReference}",
                    payment.Id,
                    webhookEvent.AkibaReference);
                return AkibaPayWebhookHandleResult.Processed(
                    payment.Id,
                    webhookEvent.AkibaReference,
                    webhookEvent.MerchantReference,
                    webhookEvent.TransactionStatus,
                    "Payment already completed.");
            }

            var outcomeResult = await paymentService.ApplyAkibaPayWebhookOutcomeAsync(
                payment.Id,
                webhookEvent.TransactionStatus,
                webhookEvent.AkibaReference);

            if (!outcomeResult.IsSuccess)
            {
                webhookEvent.ChangeStatus(WebhookEventStatus.Failed);
                await unitOfWork.AkibaPayWebhookEventRepository.AddAsync(webhookEvent);
                await unitOfWork.CompleteAsync();
                logger.LogWarning(
                    "AKIBAPAY WEBHOOK APPLY_FAILED: PaymentId={PaymentId}, Error={Error}, AkibaReference={AkibaReference}",
                    payment.Id,
                    outcomeResult.Message,
                    webhookEvent.AkibaReference);
                return AkibaPayWebhookHandleResult.Failed(
                    outcomeResult.Message ?? "Failed to apply webhook outcome to payment.",
                    webhookEvent.MerchantReference,
                    webhookEvent.AkibaReference);
            }

            webhookEvent.ChangeStatus(WebhookEventStatus.Processed);
            await unitOfWork.AkibaPayWebhookEventRepository.AddAsync(webhookEvent);
            await unitOfWork.CompleteAsync();

            logger.LogInformation(
                "AKIBAPAY WEBHOOK PROCESSED: PaymentId={PaymentId}, PaymentStatus={Status}, AkibaReference={AkibaReference}, MerchantReference={MerchantReference}",
                payment.Id,
                outcomeResult.Value,
                webhookEvent.AkibaReference,
                webhookEvent.MerchantReference);

            return AkibaPayWebhookHandleResult.Processed(
                payment.Id,
                webhookEvent.AkibaReference,
                webhookEvent.MerchantReference,
                webhookEvent.TransactionStatus,
                $"Payment updated to {outcomeResult.Value}.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "AKIBAPAY WEBHOOK ERROR: {Message}", ex.Message);
            return AkibaPayWebhookHandleResult.Error(ex.Message);
        }
    }

    public async Task<EasypayWebhookHandleResult> CreateEasypayWebhookEventAsync(EasypayWebhookEventInputModel inputModel)
    {
        try
        {
            if (inputModel is null ||
                string.IsNullOrWhiteSpace(inputModel.ReferenceId) ||
                string.IsNullOrWhiteSpace(inputModel.TransactionId) ||
                string.IsNullOrWhiteSpace(inputModel.Status) ||
                string.IsNullOrWhiteSpace(inputModel.Currency) ||
                string.IsNullOrWhiteSpace(inputModel.Provider))
            {
                logger.LogWarning("EASYPAY WEBHOOK PARSE_FAILED: invalid payload. Body: {Body}", JsonSerializer.Serialize(inputModel, AkibaPayWebhookJsonOptions));
                return EasypayWebhookHandleResult.ParseFailed("Invalid or incomplete Easypay webhook payload.");
            }

            var webhookEvent = EasypayWebhookEventEntity.Create(
                referenceId: inputModel.ReferenceId,
                transactionId: inputModel.TransactionId,
                status: inputModel.Status,
                amount: inputModel.Amount,
                currency: inputModel.Currency,
                date: ParseEasypayDate(inputModel.Date),
                provider: inputModel.Provider,
                phone: inputModel.Phone,
                telecomId: inputModel.TelecomId,
                type: inputModel.Type,
                reason: inputModel.Reason ?? inputModel.ErrorMessage);

            //var isDuplicate = await unitOfWork.EasypayWebhookEventRepository.AnyByEasypayReferenceAsync(
            //    webhookEvent.ReferenceId);
            //if (isDuplicate)
            //{
            //    webhookEvent.ChangeStatus(WebhookEventStatus.Duplicate);
            //    await unitOfWork.EasypayWebhookEventRepository.AddAsync(webhookEvent);
            //    await unitOfWork.CompleteAsync();
            //    logger.LogInformation(
            //        "EASYPAY WEBHOOK DUPLICATE: Reference={ReferenceId}",
            //        webhookEvent.ReferenceId);
            //    return EasypayWebhookHandleResult.Duplicate(
            //        webhookEvent.ReferenceId);
            //}

            var payment = await unitOfWork.PaymentRepository.GetByPaymentRefWithApplyContextAsync(
                webhookEvent.ReferenceId);
            if (payment is null)
            {
                webhookEvent.ChangeStatus(WebhookEventStatus.Failed);
                await unitOfWork.EasypayWebhookEventRepository.AddAsync(webhookEvent);
                await unitOfWork.CompleteAsync();
                logger.LogWarning(
                    "EASYPAY WEBHOOK PAYMENT_NOT_FOUND: Reference={ReferenceId}",
                    webhookEvent.ReferenceId);
                return EasypayWebhookHandleResult.PaymentNotFound(
                    webhookEvent.ReferenceId);
            }

            if (!IsEasypayCollectionEvent(webhookEvent.Type))
            {
                webhookEvent.ChangeStatus(WebhookEventStatus.Processed);
                await unitOfWork.EasypayWebhookEventRepository.AddAsync(webhookEvent);
                await unitOfWork.CompleteAsync();
                logger.LogInformation(
                    "EASYPAY WEBHOOK NON_COLLECTION_RECORDED: PaymentId={PaymentId}, Reference={ReferenceId}, Type={Type}; original payment status was not changed.",
                    payment.Id,
                    webhookEvent.ReferenceId,
                    webhookEvent.Type);
                return EasypayWebhookHandleResult.Processed(
                    payment.Id,
                    webhookEvent.ReferenceId,
                    webhookEvent.Status,
                    $"EasyPay {webhookEvent.Type} event recorded; original payment unchanged.");
            }

            if (payment.Status == PaymentStatus.Completed &&
                string.Equals(webhookEvent.Status, "SUCCESS", StringComparison.OrdinalIgnoreCase))
            {
                webhookEvent.ChangeStatus(WebhookEventStatus.Processed);
                await unitOfWork.EasypayWebhookEventRepository.AddAsync(webhookEvent);
                await unitOfWork.CompleteAsync();
                logger.LogInformation(
                    "EASYPAY WEBHOOK NOOP_ALREADY_COMPLETED: PaymentId={PaymentId}, Reference={ReferenceId}",
                    payment.Id,
                    webhookEvent.ReferenceId);
                return EasypayWebhookHandleResult.Processed(
                    payment.Id,
                    webhookEvent.ReferenceId,
                    webhookEvent.Status,
                    "Payment already completed.");
            }

            var outcomeResult = await paymentService.ApplyEasypayWebhookOutcomeAsync(
                payment.Id,
                webhookEvent.Status,
                webhookEvent.TransactionId);

            if (!outcomeResult.IsSuccess)
            {
                webhookEvent.ChangeStatus(WebhookEventStatus.Failed);
                await unitOfWork.EasypayWebhookEventRepository.AddAsync(webhookEvent);
                await unitOfWork.CompleteAsync();
                logger.LogWarning(
                    "EASYPAY WEBHOOK APPLY_FAILED: PaymentId={PaymentId}, Error={Error}, Reference={ReferenceId}",
                    payment.Id,
                    outcomeResult.Message,
                    webhookEvent.ReferenceId);
                return EasypayWebhookHandleResult.Failed(
                    outcomeResult.Message ?? "Failed to apply webhook outcome to payment.",
                    webhookEvent.ReferenceId);
            }

            webhookEvent.ChangeStatus(WebhookEventStatus.Processed);
            await unitOfWork.EasypayWebhookEventRepository.AddAsync(webhookEvent);
            await unitOfWork.CompleteAsync();

            logger.LogInformation(
                "EASYPAY WEBHOOK PROCESSED: PaymentId={PaymentId}, PaymentStatus={Status}, Reference={ReferenceId}",
                payment.Id,
                outcomeResult.Value,
                webhookEvent.ReferenceId);

            return EasypayWebhookHandleResult.Processed(
                payment.Id,
                webhookEvent.ReferenceId,                
                webhookEvent.Status,
                $"Payment updated to {outcomeResult.Value}.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "EASYPAY WEBHOOK ERROR: {Message}", ex.Message);
            return EasypayWebhookHandleResult.Error(ex.Message);
        }
    }

    private static bool IsEasypayCollectionEvent(string? eventType) =>
        string.IsNullOrWhiteSpace(eventType) ||
        string.Equals(eventType.Trim(), "Collections", StringComparison.OrdinalIgnoreCase);

    private static DateTimeOffset? ParseEasypayDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return DateTimeOffset.TryParse(
            value,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
            out var parsed)
            ? parsed
            : null;
    }

    public async Task<AfrimonyWebhookHandleResult> CreateAfrimonyWebhookEventAsync(string rawBody)
    {
        try
        {
            var inputModel = JsonSerializer.Deserialize<AfrimonyWebhookEventInputModel>(
                rawBody,
                AfrimonyWebhookJsonOptions);

            if (inputModel is null ||
                string.IsNullOrWhiteSpace(inputModel.MmTransactionId) ||
                string.IsNullOrWhiteSpace(inputModel.ExternalId) ||
                string.IsNullOrWhiteSpace(inputModel.TxnStatus))
            {
                logger.LogWarning("AFRIMONY WEBHOOK PARSE_FAILED: invalid payload. Body: {Body}", rawBody);
                return AfrimonyWebhookHandleResult.ParseFailed("Invalid or incomplete Afrimony webhook payload.");
            }

            var webhookEvent = AfrimonyWebhookEventEntity.Create(
                mmTransactionId: inputModel.MmTransactionId,
                externalId: inputModel.ExternalId,
                txnStatus: inputModel.TxnStatus,
                amount: inputModel.Amount,
                senderMsisdn: inputModel.SenderMsisdn);

            var isDuplicate = await unitOfWork.AfrimonyWebhookEventRepository.AnyByMmTransactionIdAsync(
                webhookEvent.MmTransactionId);
            if (isDuplicate)
            {
                webhookEvent.ChangeStatus(WebhookEventStatus.Duplicate);
                await unitOfWork.AfrimonyWebhookEventRepository.AddAsync(webhookEvent);
                await unitOfWork.CompleteAsync();
                logger.LogInformation(
                    "AFRIMONY WEBHOOK DUPLICATE: MMTransactionID={MmTransactionId}, ExternalId={ExternalId}",
                    webhookEvent.MmTransactionId,
                    webhookEvent.ExternalId);
                return AfrimonyWebhookHandleResult.Duplicate(
                    webhookEvent.MmTransactionId,
                    webhookEvent.ExternalId);
            }

            var payment = await unitOfWork.PaymentRepository.GetByPaymentRefWithApplyContextAsync(
                webhookEvent.ExternalId);
            if (payment is null)
            {
                webhookEvent.ChangeStatus(WebhookEventStatus.Failed);
                await unitOfWork.AfrimonyWebhookEventRepository.AddAsync(webhookEvent);
                await unitOfWork.CompleteAsync();
                logger.LogWarning(
                    "AFRIMONY WEBHOOK PAYMENT_NOT_FOUND: ExternalId={ExternalId}, MMTransactionID={MmTransactionId}",
                    webhookEvent.ExternalId,
                    webhookEvent.MmTransactionId);
                return AfrimonyWebhookHandleResult.PaymentNotFound(
                    webhookEvent.MmTransactionId,
                    webhookEvent.ExternalId);
            }

            if (payment.Status == PaymentStatus.Completed &&
                IsAfrimonySuccessStatus(webhookEvent.TxnStatus))
            {
                webhookEvent.ChangeStatus(WebhookEventStatus.Processed);
                await unitOfWork.AfrimonyWebhookEventRepository.AddAsync(webhookEvent);
                await unitOfWork.CompleteAsync();
                logger.LogInformation(
                    "AFRIMONY WEBHOOK NOOP_ALREADY_COMPLETED: PaymentId={PaymentId}, MMTransactionID={MmTransactionId}",
                    payment.Id,
                    webhookEvent.MmTransactionId);
                return AfrimonyWebhookHandleResult.Processed(
                    payment.Id,
                    webhookEvent.MmTransactionId,
                    webhookEvent.ExternalId,
                    webhookEvent.TxnStatus,
                    "Payment already completed.");
            }

            var outcomeResult = await paymentService.ApplyAfrimonyWebhookOutcomeAsync(
                payment.Id,
                webhookEvent.TxnStatus,
                webhookEvent.MmTransactionId);

            if (!outcomeResult.IsSuccess)
            {
                webhookEvent.ChangeStatus(WebhookEventStatus.Failed);
                await unitOfWork.AfrimonyWebhookEventRepository.AddAsync(webhookEvent);
                await unitOfWork.CompleteAsync();
                logger.LogWarning(
                    "AFRIMONY WEBHOOK APPLY_FAILED: PaymentId={PaymentId}, Error={Error}, MMTransactionID={MmTransactionId}",
                    payment.Id,
                    outcomeResult.Message,
                    webhookEvent.MmTransactionId);
                return AfrimonyWebhookHandleResult.Failed(
                    outcomeResult.Message ?? "Failed to apply webhook outcome to payment.",
                    webhookEvent.ExternalId,
                    webhookEvent.MmTransactionId);
            }

            webhookEvent.ChangeStatus(WebhookEventStatus.Processed);
            await unitOfWork.AfrimonyWebhookEventRepository.AddAsync(webhookEvent);
            await unitOfWork.CompleteAsync();

            logger.LogInformation(
                "AFRIMONY WEBHOOK PROCESSED: PaymentId={PaymentId}, PaymentStatus={Status}, MMTransactionID={MmTransactionId}, ExternalId={ExternalId}",
                payment.Id,
                outcomeResult.Value,
                webhookEvent.MmTransactionId,
                webhookEvent.ExternalId);

            return AfrimonyWebhookHandleResult.Processed(
                payment.Id,
                webhookEvent.MmTransactionId,
                webhookEvent.ExternalId,
                webhookEvent.TxnStatus,
                $"Payment updated to {outcomeResult.Value}.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "AFRIMONY WEBHOOK ERROR: {Message}", ex.Message);
            return AfrimonyWebhookHandleResult.Error(ex.Message);
        }
    }

    private static bool IsAfrimonySuccessStatus(string txnStatus)
    {
        var normalized = (txnStatus ?? string.Empty).Trim().ToUpperInvariant();
        return normalized is "TS" or "SUCCESS" or "SUCCESSFUL" or "COMPLETED";
    }

    public async Task<MpesaWebhookHandleResult> CreateMpesaWebhookEventAsync(string rawBody)
    {
        try
        {
            var input = JsonSerializer.Deserialize<MpesaWebhookEventInputModel>(rawBody, MpesaWebhookJsonOptions);
            if (input is null ||
                string.IsNullOrWhiteSpace(input.ThirdPartyConversationId) ||
                string.IsNullOrWhiteSpace(input.ResultCode))
            {
                logger.LogWarning("MPESA WEBHOOK PARSE_FAILED: invalid payload. Body: {Body}", rawBody);
                return new MpesaWebhookHandleResult("ParseFailed", Message: "Invalid or incomplete MPesa callback payload.");
            }

            var webhookEvent = MpesaWebhookEventEntity.Create(
                input.OriginalConversationId,
                input.ThirdPartyConversationId,
                input.TransactionId,
                input.ResultCode,
                input.ResultDescription);

            var payment = await unitOfWork.PaymentRepository.GetByPaymentRefWithApplyContextAsync(
                webhookEvent.ThirdPartyConversationId);
            if (payment is null)
            {
                webhookEvent.ChangeStatus(WebhookEventStatus.Failed);
                await unitOfWork.MpesaWebhookEventRepository.AddAsync(webhookEvent);
                await unitOfWork.CompleteAsync();
                logger.LogWarning(
                    "MPESA WEBHOOK PAYMENT_NOT_FOUND: ThirdPartyConversationId={Reference}",
                    webhookEvent.ThirdPartyConversationId);
                return new MpesaWebhookHandleResult(
                    "PaymentNotFound",
                    TransactionId: webhookEvent.TransactionId,
                    ThirdPartyConversationId: webhookEvent.ThirdPartyConversationId);
            }

            if (payment.Status == PaymentStatus.Completed &&
                string.Equals(webhookEvent.ResultCode, "INS-0", StringComparison.OrdinalIgnoreCase))
            {
                webhookEvent.ChangeStatus(WebhookEventStatus.Processed);
                await unitOfWork.MpesaWebhookEventRepository.AddAsync(webhookEvent);
                await unitOfWork.CompleteAsync();
                logger.LogInformation(
                    "MPESA WEBHOOK NOOP_ALREADY_COMPLETED: PaymentId={PaymentId}, TransactionId={TransactionId}",
                    payment.Id,
                    webhookEvent.TransactionId);
                return new MpesaWebhookHandleResult(
                    "Processed", payment.Id, webhookEvent.TransactionId,
                    webhookEvent.ThirdPartyConversationId, webhookEvent.ResultCode,
                    "Payment already completed.");
            }

            var applied = await paymentService.ApplyMpesaWebhookOutcomeAsync(
                payment.Id,
                webhookEvent.ResultCode,
                webhookEvent.TransactionId);
            if (!applied.IsSuccess)
            {
                webhookEvent.ChangeStatus(WebhookEventStatus.Failed);
                await unitOfWork.MpesaWebhookEventRepository.AddAsync(webhookEvent);
                await unitOfWork.CompleteAsync();
                logger.LogWarning(
                    "MPESA WEBHOOK APPLY_FAILED: PaymentId={PaymentId}, Error={Error}, TransactionId={TransactionId}",
                    payment.Id,
                    applied.Message,
                    webhookEvent.TransactionId);
                return new MpesaWebhookHandleResult(
                    "Failed",
                    payment.Id,
                    webhookEvent.TransactionId,
                    webhookEvent.ThirdPartyConversationId,
                    webhookEvent.ResultCode,
                    applied.Message);
            }

            webhookEvent.ChangeStatus(WebhookEventStatus.Processed);
            await unitOfWork.MpesaWebhookEventRepository.AddAsync(webhookEvent);
            await unitOfWork.CompleteAsync();

            logger.LogInformation(
                "MPESA WEBHOOK PROCESSED: PaymentId={PaymentId}, PaymentStatus={Status}, TransactionId={TransactionId}, ThirdPartyConversationId={Reference}",
                payment.Id,
                applied.Value,
                webhookEvent.TransactionId,
                webhookEvent.ThirdPartyConversationId);

            return new MpesaWebhookHandleResult(
                "Processed",
                payment.Id,
                webhookEvent.TransactionId,
                webhookEvent.ThirdPartyConversationId,
                webhookEvent.ResultCode,
                $"Payment updated to {applied.Value}.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "MPESA WEBHOOK ERROR");
            return new MpesaWebhookHandleResult("Error", Message: ex.Message);
        }
    }

    public async Task<OrangeWebhookHandleResult> CreateOrangeWebhookEventAsync(string rawBody)
    {
        try
        {
            var input = JsonSerializer.Deserialize<OrangeWebhookEventInputModel>(rawBody, MpesaWebhookJsonOptions);
            if (input is null || string.IsNullOrWhiteSpace(input.Status) ||
                string.IsNullOrWhiteSpace(input.TransactionData?.TransactionId) ||
                string.IsNullOrWhiteSpace(input.TransactionData.Currency))
            {
                logger.LogWarning("ORANGE WEBHOOK PARSE_FAILED: invalid payload. Body: {Body}", rawBody);
                return new OrangeWebhookHandleResult("ParseFailed", Message: "Invalid or incomplete Orange callback payload.");
            }

            var data = input.TransactionData;
            var webhookEvent = OrangeWebhookEventEntity.Create(
                input.Status, input.Message, data.TransactionId!, data.TxnId, data.Type,
                data.PeerId, data.PeerIdType, data.Amount, data.Currency!, data.PosId,
                data.ExecutionDate, data.Country);

            var payment = await unitOfWork.PaymentRepository.GetByPaymentRefWithApplyContextAsync(webhookEvent.TransactionId);
            if (payment is null)
            {
                webhookEvent.ChangeStatus(WebhookEventStatus.Failed);
                await unitOfWork.OrangeWebhookEventRepository.AddAsync(webhookEvent);
                await unitOfWork.CompleteAsync();
                logger.LogWarning("ORANGE WEBHOOK PAYMENT_NOT_FOUND: TransactionId={TransactionId}", webhookEvent.TransactionId);
                return new OrangeWebhookHandleResult("PaymentNotFound",
                    TransactionId: webhookEvent.TransactionId,
                    OrangeTransactionId: webhookEvent.OrangeTransactionId);
            }

            if (payment.Status == PaymentStatus.Completed &&
                string.Equals(webhookEvent.StatusCode, "SUCCESS", StringComparison.OrdinalIgnoreCase))
            {
                webhookEvent.ChangeStatus(WebhookEventStatus.Processed);
                await unitOfWork.OrangeWebhookEventRepository.AddAsync(webhookEvent);
                await unitOfWork.CompleteAsync();
                return new OrangeWebhookHandleResult("Processed", payment.Id, webhookEvent.TransactionId,
                    webhookEvent.OrangeTransactionId, webhookEvent.StatusCode, "Payment already completed.");
            }

            var applied = await paymentService.ApplyOrangeWebhookOutcomeAsync(
                payment.Id, webhookEvent.StatusCode, webhookEvent.OrangeTransactionId);
            if (!applied.IsSuccess)
            {
                webhookEvent.ChangeStatus(WebhookEventStatus.Failed);
                await unitOfWork.OrangeWebhookEventRepository.AddAsync(webhookEvent);
                await unitOfWork.CompleteAsync();
                logger.LogWarning("ORANGE WEBHOOK APPLY_FAILED: PaymentId={PaymentId}, Error={Error}",
                    payment.Id, applied.Message);
                return new OrangeWebhookHandleResult("Failed", payment.Id, webhookEvent.TransactionId,
                    webhookEvent.OrangeTransactionId, webhookEvent.StatusCode, applied.Message);
            }

            webhookEvent.ChangeStatus(WebhookEventStatus.Processed);
            await unitOfWork.OrangeWebhookEventRepository.AddAsync(webhookEvent);
            await unitOfWork.CompleteAsync();
            logger.LogInformation(
                "ORANGE WEBHOOK PROCESSED: PaymentId={PaymentId}, Status={Status}, TransactionId={TransactionId}, OrangeTransactionId={OrangeTransactionId}",
                payment.Id, applied.Value, webhookEvent.TransactionId, webhookEvent.OrangeTransactionId);
            return new OrangeWebhookHandleResult("Processed", payment.Id, webhookEvent.TransactionId,
                webhookEvent.OrangeTransactionId, webhookEvent.StatusCode, $"Payment updated to {applied.Value}.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "ORANGE WEBHOOK ERROR");
            return new OrangeWebhookHandleResult("Error", Message: ex.Message);
        }
    }

    private bool VerifyInterswitchSignature(string rawBody, string signature)
    {
        try
        {
            var secretKey = configuration["Webhooks:Interswitch:Secret"];

            if (string.IsNullOrWhiteSpace(secretKey))
            {
                logger.LogError("Webhook secret not configured");
                return false;
            }

            if (string.IsNullOrWhiteSpace(signature))
                return false;

            using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(secretKey));
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawBody));

            var computedSignature = Convert.ToHexString(hashBytes).ToLowerInvariant().Replace("-", "");
            var providedSignature = signature.Trim();

            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(computedSignature),
                Encoding.UTF8.GetBytes(providedSignature));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error verifying webhook signature");
            return false;
        }
    }
}
