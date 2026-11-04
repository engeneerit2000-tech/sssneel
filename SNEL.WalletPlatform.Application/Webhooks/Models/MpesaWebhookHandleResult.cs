namespace SNEL.WalletPlatform.Application.Webhooks.Models;

public sealed record MpesaWebhookHandleResult(
    string Outcome,
    Guid? PaymentId = null,
    string? TransactionId = null,
    string? ThirdPartyConversationId = null,
    string? TransactionStatus = null,
    string? Message = null);
