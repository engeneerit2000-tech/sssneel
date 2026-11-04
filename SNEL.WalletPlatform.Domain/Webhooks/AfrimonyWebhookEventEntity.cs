using SNEL.WalletPlatform.Domain.Common;

namespace SNEL.WalletPlatform.Domain.Webhooks;

public sealed class AfrimonyWebhookEventEntity : TrackedEntity<Guid>
{
    private AfrimonyWebhookEventEntity()
    {
        MmTransactionId = string.Empty;
        ExternalId = string.Empty;
        TxnStatus = string.Empty;
    }

    private AfrimonyWebhookEventEntity(
        string mmTransactionId,
        string externalId,
        string txnStatus,
        decimal amount,
        string? senderMsisdn,
        WebhookEventStatus status)
    {
        MmTransactionId = mmTransactionId;
        ExternalId = externalId;
        TxnStatus = txnStatus;
        Amount = amount;
        SenderMsisdn = senderMsisdn;
        Status = status;
    }

    public string MmTransactionId { get; private set; }
    public string ExternalId { get; private set; }
    public string TxnStatus { get; private set; }
    public decimal Amount { get; private set; }
    public string? SenderMsisdn { get; private set; }
    public WebhookEventStatus Status { get; private set; }

    public static AfrimonyWebhookEventEntity Create(
        string mmTransactionId,
        string externalId,
        string txnStatus,
        decimal amount,
        string? senderMsisdn)
    {
        if (string.IsNullOrWhiteSpace(mmTransactionId))
            throw new ArgumentException("MMTransactionID is required.", nameof(mmTransactionId));
        if (string.IsNullOrWhiteSpace(externalId))
            throw new ArgumentException("externalId is required.", nameof(externalId));
        if (string.IsNullOrWhiteSpace(txnStatus))
            throw new ArgumentException("txnStatus is required.", nameof(txnStatus));

        return new AfrimonyWebhookEventEntity(
            mmTransactionId.Trim(),
            externalId.Trim(),
            txnStatus.Trim(),
            amount,
            senderMsisdn?.Trim(),
            WebhookEventStatus.Pending);
    }

    public void ChangeStatus(WebhookEventStatus status) => Status = status;
}
