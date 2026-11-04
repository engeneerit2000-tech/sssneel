using Microsoft.Extensions.Localization;
using SNEL.WalletPlatform.Application;
using SNEL.WalletPlatform.Application.Common.Localization;
using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Interfaces.Auth;
using SNEL.WalletPlatform.Application.Interfaces.UnitOfWork;
using SNEL.WalletPlatform.Application.Payments.Interfaces;
using SNEL.WalletPlatform.Application.Payments.Models.Internal;
using SNEL.WalletPlatform.Application.Payments.Models.Output;
using SNEL.WalletPlatform.Domain.Payments;
using SNEL.WalletPlatform.Infrastructure.BillingServices.ConnectAfrica;
using SNEL.WalletPlatform.Infrastructure.Payments.Services;
using System.Globalization;
using System.Threading;
using Microsoft.Extensions.Logging;
using SNEL.WalletPlatform.Infrastructure.BillingServices.ConnectAfrica.Models;

namespace SNEL.WalletPlatform.Infrastructure.Payments.Connectors;

public sealed class SnelWalletPspConnector(
    IUnitOfWork unitOfWork,
    SnelWalletConfigService snelWalletConfigService,
    ConnectAfricaHttpClient connectAfricaHttpClient,
    ICurrentUserContext currentUserContext,
    IStringLocalizer<SharedResource> localizer,
    ILogger<SnelWalletPspConnector> logger,
    IPaymentRefGenerator paymentRefGenerator) : PspConnector(unitOfWork, logger, localizer)
{
    public override string PaymentServiceProviderCode => "SNEL_WALLET";
    public override bool CreateTransactions => false;
    public override bool SupportsTransactionStatusPolling => false;
    protected override async Task<Result<PaymentInitializationInternalModel>> HandleInitiatePayment(
        PaymentEntity payment,
        CancellationToken cancellationToken = default)
    {
        //var payment = await unitOfWork.PaymentRepository.GetByIdWithPspContextAsync(payment.Id);
        if (payment?.PaymentOrder is null)
        {
            return Result<PaymentInitializationInternalModel>.Failure(
                "Payment order not found for payment.",
                AppHttpStatusCodes.NotFound);
        }
        
        var paymentOrderItems = payment.PaymentOrder.Items;
        if (paymentOrderItems is null || paymentOrderItems.Count == 0)
        {
            return Result<PaymentInitializationInternalModel>.Failure(
                "Payment order has no items.",
                AppHttpStatusCodes.Failed);
        }
        
        var walletAccountIds = paymentOrderItems
            .Select(x => x.WalletAccountId)
            .Distinct()
            .ToArray();
        if (walletAccountIds.Length != 1 || walletAccountIds[0] == Guid.Empty)
        {
            return Result<PaymentInitializationInternalModel>.Failure(
                "Payment order items must reference a single wallet account.",
                AppHttpStatusCodes.Failed);
        }
        
        var walletAccountId = walletAccountIds[0];
        try
        {
            
        
            // lock is needed here?
            var account = await unitOfWork.AccountRepository.GetByIdAsync(walletAccountId);
            if (account is null)
            {
                return Result<PaymentInitializationInternalModel>.Failure(
                    $"Wallet account '{walletAccountId}' was not found.",
                    AppHttpStatusCodes.NotFound);
            }
        
            if (account.Balance < payment.Amount)
            {
                return Result<PaymentInitializationInternalModel>.Failure(
                    localizer[LocalizationKeys.AccountInsufficientBalance, walletAccountId],
                    AppHttpStatusCodes.BadRequest);
            }                        
            return Result<PaymentInitializationInternalModel>.Success(new PaymentInitializationInternalModel(
                PaymentNextAction: PaymentNextActionEnum.Confirm,
                Data: new PromptConfirmationNextActionModel(Message: localizer[LocalizationKeys.PaymentConfirmationNeeded, payment.Amount, payment.Currency?.Abbreviation!])));
        }
        catch
        {
            throw;
        }
    }

    protected override async Task<Result<PaymentNextActionOutputModel?>> HandleGetNextStepInfo(
        PaymentEntity payment)
    {
        //var payment = await unitOfWork.PaymentRepository.GetByIdWithPspContextAsync(payment.Id);
        if (payment?.PaymentMethod is null)
        {
            return Result<PaymentNextActionOutputModel?>.Failure(
                "Payment method not found for payment.",
                AppHttpStatusCodes.NotFound);
        }
        
        if (string.IsNullOrWhiteSpace(payment.PaymentMethod.Config))
        {
            return Result<PaymentNextActionOutputModel?>.Failure(
                "Payment method config is required.",
                AppHttpStatusCodes.Failed);
        }
        
        var messageTemplate = snelWalletConfigService.GetConfirmationMessageTemplate(
            payment.PaymentMethod.Config,
            currentUserContext.LanguageCode);
        var formattedAmount = payment.Amount.ToString("0.00", CultureInfo.InvariantCulture);
        var message = string.Format(CultureInfo.InvariantCulture, messageTemplate, formattedAmount);
        
        var nextStepInfo = new PaymentNextActionOutputModel(
            PaymentNextAction: PaymentNextActionEnum.Confirm,
            Data: new PromptConfirmationNextActionModel(Message: message));
        
        return Result<PaymentNextActionOutputModel?>.Success(nextStepInfo);
    }

    protected override async Task<Result<PaymentConfirmationInternalModel>> HandleConfirmPayment(Guid paymentId)
    {
        var payment = await unitOfWork.PaymentRepository.GetByIdWithPspContextAsync(paymentId);
        if (payment?.PaymentOrder is null)
        {
            return Result<PaymentConfirmationInternalModel>.Failure(
                "Payment order not found for payment.",
                AppHttpStatusCodes.NotFound);
        }
        
        var paymentOrderItems = payment.PaymentOrder.Items;
        if (paymentOrderItems is null || paymentOrderItems.Count == 0)
        {
            return Result<PaymentConfirmationInternalModel>.Failure(
                "Payment order has no items.",
                AppHttpStatusCodes.Failed);
        }
        
        var walletAccountIds = paymentOrderItems
            .Select(x => x.WalletAccountId)
            .Distinct()
            .ToArray();
        if (walletAccountIds.Length != 1 || walletAccountIds[0] == Guid.Empty)
        {
            return Result<PaymentConfirmationInternalModel>.Failure(
                "Payment order items must reference a single wallet account.",
                AppHttpStatusCodes.Failed);
        }
        
        var walletAccountId = walletAccountIds[0];
        var account = await unitOfWork.AccountRepository.GetByIdAsync(walletAccountId);
        if (account is null)
        {
            return Result<PaymentConfirmationInternalModel>.Failure(
                $"Wallet account '{walletAccountId}' was not found.",
                AppHttpStatusCodes.NotFound);
        }
        
        if (account.Balance < payment.Amount)
        {
            return Result<PaymentConfirmationInternalModel>.Failure(
                localizer[LocalizationKeys.AccountInsufficientBalance, walletAccountId],
                AppHttpStatusCodes.BadRequest);
        }
        var reference = paymentRefGenerator.GenerateRandomString();
        return Result<PaymentConfirmationInternalModel>.Success(new PaymentConfirmationInternalModel(
            PaymentExternalRef: reference,
            RequestBody: $"Confirm payment of {payment.Amount} from account {walletAccountId}",
            ResponseBody: $"Payment confirmed with reference {reference}",
            ResponseCode: "00",
            ResponseMessage: "Payment confirmed successfully."
            ));
    }    
}
