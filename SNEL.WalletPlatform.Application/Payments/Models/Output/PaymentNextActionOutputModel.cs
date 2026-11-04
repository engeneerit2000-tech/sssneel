namespace SNEL.WalletPlatform.Application.Payments.Models.Output;

public sealed record PaymentNextActionOutputModel(
    PaymentNextActionEnum PaymentNextAction,
    IPaymentNextActionData? Data);
