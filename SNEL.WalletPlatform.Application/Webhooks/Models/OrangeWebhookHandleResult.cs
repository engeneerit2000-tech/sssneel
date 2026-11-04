namespace SNEL.WalletPlatform.Application.Webhooks.Models;

public sealed record OrangeWebhookHandleResult(
    string Outcome,
    Guid? PaymentId = null,
    string? TransactionId = null,
    string? OrangeTransactionId = null,
    string? TransactionStatus = null,
    string? Message = null);
