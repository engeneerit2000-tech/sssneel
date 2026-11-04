namespace SNEL.WalletPlatform.Application.Payments.Models.Output;

public sealed record OtpConfirmationNextActionModel(
    string Message,
    int OtpLength) : IPaymentNextActionData;
