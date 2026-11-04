namespace SNEL.WalletPlatform.Application.Webhooks.Models;

public sealed class EasypayWebhookHandleResult
{
    public string Outcome { get; init; } = string.Empty;
    public string? ReferenceId { get; init; }
    public string? Status { get; init; }
    public Guid? PaymentId { get; init; }
    public string? Message { get; init; }

    public static EasypayWebhookHandleResult ParseFailed(string message) =>
        new() { Outcome = "ParseFailed", Message = message };

    public static EasypayWebhookHandleResult Duplicate(string ReferenceId) =>
        new()
        {
            Outcome = "Duplicate",
            ReferenceId = ReferenceId
        };

    public static EasypayWebhookHandleResult PaymentNotFound(string ReferenceId) =>
        new()
        {
            Outcome = "PaymentNotFound",
            ReferenceId = ReferenceId
        };

    public static EasypayWebhookHandleResult Processed(
        Guid paymentId,
        string ReferenceId,
        string Status,
        string? message = null) =>
        new()
        {
            Outcome = "Processed",
            PaymentId = paymentId,
            ReferenceId = ReferenceId,
            Status = Status,
            Message = message
        };

    public static EasypayWebhookHandleResult Failed(string message, string? ReferenceId = null) =>
        new()
        {
            Outcome = "Failed",
            Message = message,
            ReferenceId = ReferenceId
        };

    public static EasypayWebhookHandleResult Error(string message) =>
        new() { Outcome = "Error", Message = message };
}
