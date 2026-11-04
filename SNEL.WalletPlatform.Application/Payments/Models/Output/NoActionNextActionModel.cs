namespace SNEL.WalletPlatform.Application.Payments.Models.Output;

public sealed record NoActionNextActionModel(
    string Message) : IPaymentNextActionData;

