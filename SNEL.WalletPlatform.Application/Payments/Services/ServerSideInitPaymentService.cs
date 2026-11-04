//using Quartz;
//using SNEL.WalletPlatform.Application.Common;
//using SNEL.WalletPlatform.Application.Payments.Models.Output;
//using SNEL.WalletPlatform.Domain.Payments;

//namespace SNEL.WalletPlatform.Application.Payments.Services;

//public sealed class ServerSideInitPaymentService(ISchedulerFactory schedulerFactory)
//    : InitPaymentServiceBase(schedulerFactory)
//{
//    public override bool CanHandle(PaymentInitModeEnum paymentInitMode) =>
//        paymentInitMode == PaymentInitModeEnum.SERVER_SIDE;

//    protected override Task<Result<PaymentNextActionOutputModel>> HandleInitPayment()
//    {
//        return PaymentMethod.ConfirmationMode switch
//        {
//            ConfirmationModeEnum.EXTERNAL => HandleExternalConfirmation(),
//            ConfirmationModeEnum.PLATFORM => HandlePlatformConfirmation(),
//            _ => HandleUnsupportedPath()
//        };
//    }

//    private Task<Result<PaymentNextActionOutputModel>> HandleExternalConfirmation()
//    {
//        return PaymentMethod.CompletionMode switch
//        {
//            CompletionModeEnum.POLLING => HandleExternalPollingCompletion(),
//            CompletionModeEnum.WEBHOOK => HandleExternalWebhookCompletion(),
//            _ => HandleUnsupportedPath()
//        };
//    }

//    private Task<Result<PaymentNextActionOutputModel>> HandlePlatformConfirmation()
//    {
//        return PaymentMethod.ConfirmationTypeEnum switch
//        {
//            ConfirmationTypeEnum.NONE => HandlePlatformNoConfirmationType(),
//            ConfirmationTypeEnum.OTP => HandlePlatformNextStepConfirmationType(),
//            ConfirmationTypeEnum.PROMPT => HandlePlatformNextStepConfirmationType(),
//            _ => HandleUnsupportedPath()
//        };
//    }

//    private async Task<Result<PaymentNextActionOutputModel>> HandleExternalPollingCompletion()
//    {
//        var outputResult = await Connector.InitiatePayment(Payment);
//        if (!outputResult.IsSuccess)
//        {
//            return Result<PaymentNextActionOutputModel>.Failure(
//                outputResult.Error!,
//                outputResult.StatusCode);
//        }

//        await TriggerPollingJob(Payment.Id);
//        return Result<PaymentNextActionOutputModel>.Success(outputResult.Value);
//    }

//    private async Task<Result<PaymentNextActionOutputModel>> HandleExternalWebhookCompletion()
//    {
//        var outputResult = await Connector.InitiatePayment(Payment);
//        if (!outputResult.IsSuccess)
//        {
//            return Result<PaymentNextActionOutputModel>.Failure(
//                outputResult.Error!,
//                outputResult.StatusCode);
//        }

//        // We do nothing here, just wait for the webhook to be called
//        return Result<PaymentNextActionOutputModel>.Success(outputResult.Value!);
//    }

//    private async Task<Result<PaymentNextActionOutputModel>> HandlePlatformNoConfirmationType()
//    {
//        var outputResult = await Connector.InitiatePayment(Payment);
//        if (!outputResult.IsSuccess)
//        {
//            return Result<PaymentNextActionOutputModel>.Failure(
//                outputResult.Error!,
//                outputResult.StatusCode);
//        }

//        var confirmResult = await Connector.ConfirmPayment(Payment.Id);
//        if (!confirmResult.IsSuccess)
//        {
//            return Result<PaymentNextActionOutputModel>.Failure(
//                confirmResult.Error!,
//                confirmResult.StatusCode);
//        }

//        return Result<PaymentNextActionOutputModel>.Success(outputResult.Value);
//    }

//    private async Task<Result<PaymentNextActionOutputModel>> HandlePlatformNextStepConfirmationType()
//    {
//        var outputResult = await Connector.InitiatePayment(Payment);
//        if (!outputResult.IsSuccess)
//        {
//            return Result<PaymentNextActionOutputModel>.Failure(
//                outputResult.Error!,
//                outputResult.StatusCode);
//        }

//        var nextStepInfoResult = await Connector.GetNextStepInfo(Payment);
//        if (!nextStepInfoResult.IsSuccess)
//        {
//            return Result<PaymentNextActionOutputModel>.Failure(
//                nextStepInfoResult.Error!,
//                nextStepInfoResult.StatusCode);
//        }
//        return Result<PaymentNextActionOutputModel>.Success(nextStepInfoResult.Value);
//    }
//}
