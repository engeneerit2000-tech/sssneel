using SNEL.WalletPlatform.Domain.Common;

namespace SNEL.WalletPlatform.Domain.Webhooks;

public sealed class OrangeWebhookEventEntity : TrackedEntity<Guid>
{
    private OrangeWebhookEventEntity()
    {
        StatusCode = string.Empty;
        TransactionId = string.Empty;
        Currency = string.Empty;
    }

    private OrangeWebhookEventEntity(string statusCode, string? message, string transactionId,
        string? orangeTransactionId, string? type, string? peerId, string? peerIdType,
        decimal amount, string currency, string? posId, DateTimeOffset? executionDate,
        string? country, WebhookEventStatus status)
    {
        StatusCode = statusCode;
        Message = message;
        TransactionId = transactionId;
        OrangeTransactionId = orangeTransactionId;
        Type = type;
        PeerId = peerId;
        PeerIdType = peerIdType;
        Amount = amount;
        Currency = currency;
        PosId = posId;
        ExecutionDate = executionDate;
        Country = country;
        Status = status;
    }

    public string StatusCode { get; private set; }
    public string? Message { get; private set; }
    public string TransactionId { get; private set; }
    public string? OrangeTransactionId { get; private set; }
    public string? Type { get; private set; }
    public string? PeerId { get; private set; }
    public string? PeerIdType { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; }
    public string? PosId { get; private set; }
    public DateTimeOffset? ExecutionDate { get; private set; }
    public string? Country { get; private set; }
    public WebhookEventStatus Status { get; private set; }

    public static OrangeWebhookEventEntity Create(string statusCode, string? message, string transactionId,
        string? orangeTransactionId, string? type, string? peerId, string? peerIdType,
        decimal amount, string currency, string? posId, DateTimeOffset? executionDate, string? country)
    {
        if (string.IsNullOrWhiteSpace(statusCode)) throw new ArgumentException("Status is required.", nameof(statusCode));
        if (string.IsNullOrWhiteSpace(transactionId)) throw new ArgumentException("TransactionId is required.", nameof(transactionId));
        if (string.IsNullOrWhiteSpace(currency)) throw new ArgumentException("Currency is required.", nameof(currency));
        return new OrangeWebhookEventEntity(statusCode.Trim(), message?.Trim(), transactionId.Trim(),
            orangeTransactionId?.Trim(), type?.Trim(), peerId?.Trim(), peerIdType?.Trim(), amount,
            currency.Trim(), posId?.Trim(), executionDate, country?.Trim(), WebhookEventStatus.Pending);
    }

    public void ChangeStatus(WebhookEventStatus status) => Status = status;
}
