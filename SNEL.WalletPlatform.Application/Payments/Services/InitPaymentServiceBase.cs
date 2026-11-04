//using SNEL.WalletPlatform.Application.Common;
//using SNEL.WalletPlatform.Application.Jobs;
//using SNEL.WalletPlatform.Application.Payments.Interfaces;
//using SNEL.WalletPlatform.Application.Payments.Models.Internal;
//using SNEL.WalletPlatform.Application.Payments.Models.Output;
//using SNEL.WalletPlatform.Domain.Payments;
//using Quartz;

//namespace SNEL.WalletPlatform.Application.Payments.Services;

//public abstract class InitPaymentServiceBase(ISchedulerFactory schedulerFactory) : IInitPaymentService
//{
//    protected PaymentEntity Payment { get; private set; } = null!;
//    protected PspConnector Connector { get; private set; } = null!;

//    protected PaymentMethodEntity PaymentMethod => Payment.PaymentMethod;

//    public abstract bool CanHandle(PaymentInitModeEnum paymentInitMode);

//    public Task<Result<PaymentNextActionOutputModel>> InitPayment(
//        PaymentEntity payment,
//        PspConnector connector)
//    {
//        Payment = payment;
//        Connector = connector;
//        return HandleInitPayment();
//    }

//    protected abstract Task<Result<PaymentNextActionOutputModel>> HandleInitPayment();

//    //protected virtual async Task TriggerPollingJob(Guid paymentId)
//    //{
//    //    var scheduler = await schedulerFactory.GetScheduler();
//    //    // throws if job was not registered on startup
//    //    if (!await scheduler.CheckExists(PaymentStatusPollingJobConstants.JobKey))
//    //    {
//    //        throw new InvalidOperationException(
//    //            $"Quartz job '{PaymentStatusPollingJobConstants.JobGroup}.{PaymentStatusPollingJobConstants.JobName}' is not registered.");
//    //    }

//    //    var triggerKey = PaymentStatusPollingJobConstants.CreateTriggerKey(paymentId);
//    //    // Removes old registered job if exists
//    //    if (await scheduler.CheckExists(triggerKey))
//    //    {
//    //        await scheduler.UnscheduleJob(triggerKey);
//    //    }

//    //    var trigger = TriggerBuilder.Create()
//    //        .WithIdentity(triggerKey)
//    //        .ForJob(PaymentStatusPollingJobConstants.JobKey)
//    //        .UsingJobData(PaymentStatusPollingJobConstants.PaymentIdKey, paymentId.ToString())
//    //        .StartAt(DateBuilder.FutureDate(
//    //            PaymentStatusPollingJobConstants.InitialDelaySeconds, // 60
//    //            IntervalUnit.Second))
//    //        .WithSimpleSchedule(x => x
//    //            .WithIntervalInSeconds(PaymentStatusPollingJobConstants.IntervalSeconds) // 30
//    //            .WithRepeatCount(PaymentStatusPollingJobConstants.TotalAttempts - 1)) // 10
//    //        .Build();

//    //    await scheduler.ScheduleJob(trigger);
//    //}

//    protected Task<Result<PaymentNextActionOutputModel>> HandleUnsupportedPath()
//    {
//        return Task.FromResult(Result<PaymentNextActionOutputModel>.Failure(
//            $"Payment init path is not supported: InitMode='{PaymentMethod.PaymentInitMode}', ConfirmationMode='{PaymentMethod.ConfirmationMode}', ConfirmationType='{PaymentMethod.ConfirmationTypeEnum}', CompletionMode='{PaymentMethod.CompletionMode}'.",
//            AppHttpStatusCodes.BadRequest));
//    }
    
    
//}
