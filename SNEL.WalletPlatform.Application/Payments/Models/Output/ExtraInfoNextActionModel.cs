namespace SNEL.WalletPlatform.Application.Payments.Models.Output;

public sealed record ExtraInfoNextActionModel(
    string Message,
    string[] PreviousNumbers) : IPaymentNextActionData;
