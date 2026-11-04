namespace SNEL.WalletPlatform.Application.Payments.Models.Output;

public sealed record PaymentInitializationOutputModel(
    //Guid PaymentOrderId,
    Guid PaymentId,
    string PaymentInternalReference,
    ICollection<PaymentOrderItemSummaryOutputModel> Items,
    //string PaymentServiceProviderCode,
    //string PaymentMethodCode,
    PaymentNextActionOutputModel? NextStepInfo);

public sealed record PaymentConfirmationOutputModel(
    Guid PaymentId,
    string PaymentInternalReference,
    string PaymentExternalReference,
    ICollection<PaymentOrderItemSummaryOutputModel> Items);
