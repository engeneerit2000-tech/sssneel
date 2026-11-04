namespace SNEL.WalletPlatform.Application.Payments.Models.Output;

public sealed record InitializeClientSideFromWebNextActionModel(
    IReadOnlyDictionary<string, string>? NextActionFields, string Message) : IPaymentNextActionData;
