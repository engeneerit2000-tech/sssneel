using SNEL.WalletPlatform.Application;
using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Common.Localization;
using SNEL.WalletPlatform.Application.Payments.Models.Internal;
using SNEL.WalletPlatform.Application.Payments.Models.Output;
using SNEL.WalletPlatform.Domain.Payments;
using System.Threading;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using SNEL.WalletPlatform.Application.Interfaces.UnitOfWork;

namespace SNEL.WalletPlatform.Application.Payments.Interfaces;

public abstract class PspConnector(
    IUnitOfWork unitOfWork,
    ILogger<PspConnector> logger,
    IStringLocalizer<SharedResource> localizer)
{
    public abstract string PaymentServiceProviderCode { get; }
    public abstract bool CreateTransactions{ get;}

    /// <summary>
    /// When false, stale-expire and bulk sync skip external PSP status checks (e.g. internal wallet confirm flow).
    /// </summary>
    public virtual bool SupportsTransactionStatusPolling => true;


    public virtual Task<Result<bool>> IsEligible(PaymentEntity payment)
    {
        return Task.FromResult(Result<bool>.Success(true));
    }

    public Task<Result<PaymentNextActionOutputModel>> GetDataForPaymentInitiation(PaymentEntity payment)
    {
        return HandleGetDataForPaymentInitiation(payment);
    }
    protected virtual Task<Result<PaymentNextActionOutputModel>> HandleGetDataForPaymentInitiation(PaymentEntity payment)=>
        throw new NotImplementedException($"HandleGetDataForPaymentInitiation is not used for this PSP connector{this.GetType().Name}.");

    public async Task<Result<PaymentInitializationInternalModel>> InitiatePayment(
        PaymentEntity payment,
        CancellationToken cancellationToken = default)
    {
        var eligibilityResult = await IsEligible(payment);
        if (!eligibilityResult.IsSuccess)
        {
            return Result<PaymentInitializationInternalModel>.Failure(
                eligibilityResult.Message!,
                eligibilityResult.StatusCode);
        }

        if (!eligibilityResult.Value)
        {
            return Result<PaymentInitializationInternalModel>.Failure(
                localizer[LocalizationKeys.PaymentPspPaymentNotEligible],
                AppHttpStatusCodes.BadRequest);
        }

        PaymentTxnEntity? paymentTxnInit = null;
        if (CreateTransactions)
        {
            paymentTxnInit = await unitOfWork.ExecuteInTransactionAsync(async ct =>
            {

                var paymentTxn = PaymentTxnEntity.Create(
                    paymentId: payment.Id,
                    type: TransactionType.Initiate,
                    status: TransactionStatus.Completed,
                    currencyId: payment.CurrencyId,
                    amount: payment.Amount,
                    externalRef: payment.ExternalRef,
                    reqBody: null,
                    resBody: null,
                    resCode: null,
                    resMessage: null);
                await unitOfWork.PaymentTxnRepository.AddAsync(paymentTxn);
                await unitOfWork.CompleteAsync();
                return paymentTxn;
            }, cancellationToken);
            if (paymentTxnInit == null)
            {
                logger.LogError("Failed to create payment transaction for initiation for PaymentId: {PaymentId}.", payment.Id);
                return Result<PaymentInitializationInternalModel>.Failure(
                    localizer[LocalizationKeys.PaymentPspInitTransactionCreateFailed, payment.Id],
                    AppHttpStatusCodes.Failed);
            }
        }
        var initiateResult = await HandleInitiatePayment(payment,cancellationToken);
        if (!initiateResult.IsSuccess)
        {
            if (paymentTxnInit != null)
            {
                logger.LogError("Payment initiation failed for PaymentId: {PaymentId}. Error: {ErrorMessage}", payment.Id, initiateResult.Message);
                await unitOfWork.ExecuteInTransactionAsync(async ct =>
                {
                    paymentTxnInit!.SetStatus(TransactionStatus.Failed);
                    unitOfWork.PaymentTxnRepository.Update(paymentTxnInit);
                    await unitOfWork.CompleteAsync();
                });
            }            
        }
        return initiateResult;

    }

    protected virtual Task<Result<PaymentInitializationInternalModel>> HandleInitiatePayment(
        PaymentEntity payment,
        CancellationToken cancellationToken = default)=> throw new NotImplementedException($"HandleInitiatePayment is not used for this PSP connector{this.GetType().Name}.");

    public Task<Result<PaymentNextActionOutputModel?>> GetNextStepInfo(
        PaymentEntity payment)
    {
        return HandleGetNextStepInfo(payment);
    }

    protected virtual Task<Result<PaymentNextActionOutputModel?>> HandleGetNextStepInfo(
        PaymentEntity payment)=>
        throw new NotImplementedException($"HandleGetNextStepInfo is not used for this PSP connector{this.GetType().Name}.");


    public async Task<Result<PaymentConfirmationInternalModel>> ConfirmPayment(Guid paymentId)
    {
        PaymentTxnEntity? paymentTxnConfirm = null;
        if (CreateTransactions)
        {
            paymentTxnConfirm = await unitOfWork.ExecuteInTransactionAsync<PaymentTxnEntity>(async ct =>
            {

                var payment = await unitOfWork.PaymentRepository.GetByIdAsync(paymentId);

                var paymentTxnConfirm = PaymentTxnEntity.Create(
                    paymentId: paymentId,
                    type: TransactionType.Confirm,
                    status: TransactionStatus.Completed,
                    currencyId: payment.CurrencyId,
                    amount: payment.Amount,
                    externalRef: payment.ExternalRef,
                    reqBody: null,
                    resBody: null,
                    resCode: null,
                    resMessage: null);
                await unitOfWork.PaymentTxnRepository.AddAsync(paymentTxnConfirm);
                await unitOfWork.CompleteAsync();
                return paymentTxnConfirm;
            });
        }
         
        

        var confirmResult = await HandleConfirmPayment(paymentId);
        if (confirmResult.IsSuccess)
        {
            if (paymentTxnConfirm != null)
            {
                
                await unitOfWork.ExecuteInTransactionAsync(async ct =>
                {
                    paymentTxnConfirm!.SetStatus(TransactionStatus.Completed);
                    paymentTxnConfirm.SetRequestBody(confirmResult.Value.RequestBody);
                    paymentTxnConfirm.SetResponseBody(confirmResult.Value.ResponseBody);
                    paymentTxnConfirm.SetResponseCode(confirmResult.Value.ResponseCode);
                    paymentTxnConfirm.SetResponseMessage(confirmResult.Value.ResponseMessage);
                    unitOfWork.PaymentTxnRepository.Update(paymentTxnConfirm);
                    await unitOfWork.CompleteAsync();
                });
            }
        }
        else {
            if (paymentTxnConfirm != null)
            {
                await unitOfWork.ExecuteInTransactionAsync(async ct =>
                {
                    paymentTxnConfirm!.SetStatus(TransactionStatus.Failed);
                    unitOfWork.PaymentTxnRepository.Update(paymentTxnConfirm);
                    await unitOfWork.CompleteAsync();
                });
            }
        }
        return confirmResult;
    }

    protected virtual Task<Result<PaymentConfirmationInternalModel>> HandleConfirmPayment(Guid paymentId)=> throw new NotImplementedException($"HandleConfirmPayment is not used for this PSP connector{this.GetType().Name}.");


    //Nedal craete paymentTxnEntity here befor HandleGetTransactionStatus, and if HandleGetTransactionStatus fails, update that paymentTxnEntity with failed status and error details. This way we will have a record of transaction attempt even if it fails at status check step.
    public async Task<Result<PaymentCheckStatusInternalModel>> GetTransactionStatus(PaymentTransactionStatusContextInternalModel context)
    {
        PaymentTxnEntity? paymentTxnCheckStatus = null;
        if (CreateTransactions)
        {
            paymentTxnCheckStatus = await unitOfWork.ExecuteInTransactionAsync<PaymentTxnEntity>(async ct =>
            {
                var paymentTxn = PaymentTxnEntity.Create(
                    paymentId: context.PaymentId,
                    type: TransactionType.CheckStatus,
                    status: TransactionStatus.Completed,
                    currencyId: context.CurrencyId,
                    amount: context.Amount,
                    externalRef: context.PaymentExternalReference,
                    reqBody: null,
                    resBody: null,
                    resCode: null,
                    resMessage: null);
                await unitOfWork.PaymentTxnRepository.AddAsync(paymentTxn);
                await unitOfWork.CompleteAsync();
                return paymentTxn;
            });
        }
        var resultCheckStatus = await HandleGetTransactionStatus(context);

        if (paymentTxnCheckStatus != null && resultCheckStatus.Value is not null)
        {
            await unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                paymentTxnCheckStatus!.SetStatus(resultCheckStatus.IsSuccess ? TransactionStatus.Completed : TransactionStatus.Failed);
                paymentTxnCheckStatus.SetRequestBody(resultCheckStatus.Value.RequestBody);
                paymentTxnCheckStatus.SetResponseBody(resultCheckStatus.Value.ResponseBody);
                paymentTxnCheckStatus.SetResponseCode(resultCheckStatus.Value.ResponseCode);
                paymentTxnCheckStatus.SetResponseMessage(resultCheckStatus.Value.ResponseMessage);
                unitOfWork.PaymentTxnRepository.Update(paymentTxnCheckStatus);
                await unitOfWork.CompleteAsync();
            });
        }
        
        return resultCheckStatus;
    }

    protected void LogTransactionStatusRequest(Guid paymentId, string? requestBody) =>
        logger.LogInformation(
            "{PspCode} GetTransactionStatus request. PaymentId={PaymentId}, Body={RequestBody}",
            PaymentServiceProviderCode,
            paymentId,
            requestBody ?? string.Empty);

    protected void LogTransactionStatusResponse(Guid paymentId, string? responseCode, string? responseBody) =>
        logger.LogInformation(
            "{PspCode} GetTransactionStatus response. PaymentId={PaymentId}, StatusCode={StatusCode}, Body={ResponseBody}",
            PaymentServiceProviderCode,
            paymentId,
            responseCode ?? string.Empty,
            responseBody ?? string.Empty);

    protected virtual Task<Result<PaymentCheckStatusInternalModel>> HandleGetTransactionStatus(
        PaymentTransactionStatusContextInternalModel context)=> throw new NotImplementedException($"HandleGetTransactionStatus is not used for this PSP connector{this.GetType().Name}.");

}