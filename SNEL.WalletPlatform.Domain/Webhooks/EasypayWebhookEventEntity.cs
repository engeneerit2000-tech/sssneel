using SNEL.WalletPlatform.Domain.Common;

namespace SNEL.WalletPlatform.Domain.Webhooks;

public sealed class EasypayWebhookEventEntity : TrackedEntity<Guid>
{
    private EasypayWebhookEventEntity()
    {
        ReferenceId = string.Empty;
        TransactionId = string.Empty;
        Status = string.Empty;
        Provider = string.Empty;
        Currency = string.Empty;
    }

    private EasypayWebhookEventEntity(
        string referenceId,
        string status,
        string transactionId,
        decimal amount,
        string currency,
        DateTimeOffset? date,
        string? provider,
        string? phone,
        string? telecomId,
        string? type,
        string? reason,
        WebhookEventStatus webhookStatus)
    {
        ReferenceId = referenceId;
        Status = status;
        TransactionId = transactionId;
        Amount = amount;
        Currency = currency;
        Date = date;
        Provider = provider;
        Phone = phone;
        TelecomId = telecomId;
        Type = type;
        Reason = reason;
        WebhookStatus = webhookStatus;
    }

    public string ReferenceId { get; private set; }
    public string Status { get; private set; }
    public string TransactionId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; }
    public string? Provider { get; private set; }
    public string? Phone { get; private set; }
    public string? TelecomId { get; private set; }
    public string? Type { get; private set; }
    public string? Reason { get; private set; }
    public DateTimeOffset? Date { get; private set; }
    public WebhookEventStatus WebhookStatus { get; private set; }

    public static EasypayWebhookEventEntity Create(
        string referenceId,
        string status,
        string transactionId,
        decimal amount,
        string currency,
        DateTimeOffset? date,
        string? provider,
        string? phone,
        string? telecomId,
        string? type,
        string? reason)
    {
        if (string.IsNullOrWhiteSpace(referenceId))
            throw new ArgumentException("ReferenceId is required.", nameof(referenceId));
        if (string.IsNullOrWhiteSpace(status))
            throw new ArgumentException("Status is required.", nameof(status));
        if (string.IsNullOrWhiteSpace(provider))
            throw new ArgumentException("Provider is required.", nameof(status));
        if (string.IsNullOrWhiteSpace(transactionId))
            throw new ArgumentException("TransactionId is required.", nameof(transactionId));
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency is required.", nameof(currency));

        return new EasypayWebhookEventEntity(
            referenceId.Trim(),
            status.Trim(),
            transactionId.Trim(),
            amount,
            currency,
            date,
            provider,
            NormalizeOptional(phone),
            NormalizeOptional(telecomId),
            NormalizeOptional(type),
            NormalizeOptional(reason),
            WebhookEventStatus.Pending);
    }

    public void ChangeStatus(WebhookEventStatus status) => WebhookStatus = status;

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

