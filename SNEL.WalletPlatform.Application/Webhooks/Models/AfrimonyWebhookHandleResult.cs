namespace SNEL.WalletPlatform.Application.Webhooks.Models;

public sealed class AfrimonyWebhookHandleResult
{
    public string Outcome { get; init; } = string.Empty;
    public string? ExternalId { get; init; }
    public string? MmTransactionId { get; init; }
    public string? TxnStatus { get; init; }
    public Guid? PaymentId { get; init; }
    public string? Message { get; init; }

    public static AfrimonyWebhookHandleResult ParseFailed(string message) =>
        new() { Outcome = "ParseFailed", Message = message };

    public static AfrimonyWebhookHandleResult Duplicate(string mmTransactionId, string externalId) =>
        new()
        {
            Outcome = "Duplicate",
            MmTransactionId = mmTransactionId,
            ExternalId = externalId
        };

    public static AfrimonyWebhookHandleResult PaymentNotFound(string mmTransactionId, string externalId) =>
        new()
        {
            Outcome = "PaymentNotFound",
            MmTransactionId = mmTransactionId,
            ExternalId = externalId
        };

    public static AfrimonyWebhookHandleResult Processed(
        Guid paymentId,
        string mmTransactionId,
        string externalId,
        string txnStatus,
        string? message = null) =>
        new()
        {
            Outcome = "Processed",
            PaymentId = paymentId,
            MmTransactionId = mmTransactionId,
            ExternalId = externalId,
            TxnStatus = txnStatus,
            Message = message
        };

    public static AfrimonyWebhookHandleResult Failed(
        string message,
        string? externalId = null,
        string? mmTransactionId = null) =>
        new()
        {
            Outcome = "Failed",
            Message = message,
            ExternalId = externalId,
            MmTransactionId = mmTransactionId
        };

    public static AfrimonyWebhookHandleResult Error(string message) =>
        new() { Outcome = "Error", Message = message };
}
