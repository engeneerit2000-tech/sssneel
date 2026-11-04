namespace SNEL.WalletPlatform.Application.Payments.Models.Output;

public sealed record PromptConfirmationNextActionModel(
    string Message) : IPaymentNextActionData;
