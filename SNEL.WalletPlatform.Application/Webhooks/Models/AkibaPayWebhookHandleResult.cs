namespace SNEL.WalletPlatform.Application.Webhooks.Models;

public sealed class AkibaPayWebhookHandleResult
{
    public string Outcome { get; init; } = string.Empty;
    public string? MerchantReference { get; init; }
    public string? AkibaReference { get; init; }
    public string? TransactionStatus { get; init; }
    public Guid? PaymentId { get; init; }
    public string? Message { get; init; }

    public static AkibaPayWebhookHandleResult ParseFailed(string message) =>
        new() { Outcome = "ParseFailed", Message = message };

    public static AkibaPayWebhookHandleResult Duplicate(string akibaReference, string merchantReference) =>
        new()
        {
            Outcome = "Duplicate",
            AkibaReference = akibaReference,
            MerchantReference = merchantReference
        };

    public static AkibaPayWebhookHandleResult PaymentNotFound(string akibaReference, string merchantReference) =>
        new()
        {
            Outcome = "PaymentNotFound",
            AkibaReference = akibaReference,
            MerchantReference = merchantReference
        };

    public static AkibaPayWebhookHandleResult Processed(
        Guid paymentId,
        string akibaReference,
        string merchantReference,
        string transactionStatus,
        string? message = null) =>
        new()
        {
            Outcome = "Processed",
            PaymentId = paymentId,
            AkibaReference = akibaReference,
            MerchantReference = merchantReference,
            TransactionStatus = transactionStatus,
            Message = message
        };

    public static AkibaPayWebhookHandleResult Failed(string message, string? merchantReference = null, string? akibaReference = null) =>
        new()
        {
            Outcome = "Failed",
            Message = message,
            MerchantReference = merchantReference,
            AkibaReference = akibaReference
        };

    public static AkibaPayWebhookHandleResult Error(string message) =>
        new() { Outcome = "Error", Message = message };
}
