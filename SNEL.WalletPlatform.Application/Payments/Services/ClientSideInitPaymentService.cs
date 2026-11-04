//using Quartz;
//using SNEL.WalletPlatform.Application.Common;
//using SNEL.WalletPlatform.Application.Payments.Models.Output;
//using SNEL.WalletPlatform.Domain.Payments;

//namespace SNEL.WalletPlatform.Application.Payments.Services;

//public sealed class ClientSideInitPaymentService(ISchedulerFactory schedulerFactory)
//    : InitPaymentServiceBase(schedulerFactory)
//{
//    public override bool CanHandle(PaymentInitModeEnum paymentInitMode) =>
//        paymentInitMode == PaymentInitModeEnum.CLIENT_SIDE;

//    protected override async Task<Result<PaymentNextActionOutputModel>> HandleInitPayment()
//    {
//        var outputResult = await Connector.GetDataForPaymentInitiation(Payment);
//        if (!outputResult.IsSuccess)
//        {
//            return Result<PaymentNextActionOutputModel>.Failure(
//                outputResult.Error!,
//                outputResult.StatusCode);
//        }

//        switch (PaymentMethod.CompletionMode)
//        {
//            case CompletionModeEnum.POLLING:
//                await HandlePollingCompletion(Payment.Id);
//                break;
//            case CompletionModeEnum.WEBHOOK:
//                await HandleWebhookCompletion();
//                break;
//            default:
//                await HandleUnsupportedPath();
//                break;
//        }
//        return Result<PaymentNextActionOutputModel>.Success(outputResult.Value!);

//    }

//    private async Task HandlePollingCompletion(Guid paymentId)
//    {
       

//        await TriggerPollingJob(paymentId);
//    }

//    private async Task HandleWebhookCompletion()
//    {
//        // We do nothing here, just wait for the webhook to be called
        
//    }
//}
