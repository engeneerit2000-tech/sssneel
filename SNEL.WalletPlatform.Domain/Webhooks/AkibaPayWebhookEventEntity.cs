using SNEL.WalletPlatform.Domain.Common;

namespace SNEL.WalletPlatform.Domain.Webhooks;

public sealed class AkibaPayWebhookEventEntity : TrackedEntity<Guid>
{
    private AkibaPayWebhookEventEntity()
    {
        AkibaReference = string.Empty;
        MerchantReference = string.Empty;
        TransactionStatus = string.Empty;
        Currency = string.Empty;
    }

    private AkibaPayWebhookEventEntity(
        string akibaReference,
        string merchantReference,
        string transactionStatus,
        string? toNumberTelecomReference,
        string? fromNumberTelecomReference,
        decimal amount,
        string currency,
        string? toNumber,
        string? fromNumber,
        DateTimeOffset? createdAt,
        DateTimeOffset? updatedAt,
        DateTimeOffset? callbackGeneratedAt,
        WebhookEventStatus status)
    {
        AkibaReference = akibaReference;
        MerchantReference = merchantReference;
        TransactionStatus = transactionStatus;
        ToNumberTelecomReference = toNumberTelecomReference;
        FromNumberTelecomReference = fromNumberTelecomReference;
        Amount = amount;
        Currency = currency;
        ToNumber = toNumber;
        FromNumber = fromNumber;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        CallbackGeneratedAt = callbackGeneratedAt;
        Status = status;
    }

    public string AkibaReference { get; private set; }
    public string MerchantReference { get; private set; }
    public string TransactionStatus { get; private set; }
    public string? ToNumberTelecomReference { get; private set; }
    public string? FromNumberTelecomReference { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; }
    public string? ToNumber { get; private set; }
    public string? FromNumber { get; private set; }
    public DateTimeOffset? CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public DateTimeOffset? CallbackGeneratedAt { get; private set; }
    public WebhookEventStatus Status { get; private set; }

    public static AkibaPayWebhookEventEntity Create(
        string akibaReference,
        string merchantReference,
        string transactionStatus,
        string? toNumberTelecomReference,
        string? fromNumberTelecomReference,
        decimal amount,
        string currency,
        string? toNumber,
        string? fromNumber,
        DateTimeOffset? createdAt,
        DateTimeOffset? updatedAt,
        DateTimeOffset? callbackGeneratedAt)
    {
        if (string.IsNullOrWhiteSpace(akibaReference))
            throw new ArgumentException("AkibaReference is required.", nameof(akibaReference));
        if (string.IsNullOrWhiteSpace(merchantReference))
            throw new ArgumentException("MerchantReference is required.", nameof(merchantReference));
        if (string.IsNullOrWhiteSpace(transactionStatus))
            throw new ArgumentException("TransactionStatus is required.", nameof(transactionStatus));
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency is required.", nameof(currency));

        return new AkibaPayWebhookEventEntity(
            akibaReference.Trim(),
            merchantReference.Trim(),
            transactionStatus.Trim(),
            toNumberTelecomReference,
            fromNumberTelecomReference,
            amount,
            currency.Trim(),
            toNumber,
            fromNumber,
            createdAt,
            updatedAt,
            callbackGeneratedAt,
            WebhookEventStatus.Pending);
    }

    public void ChangeStatus(WebhookEventStatus status) => Status = status;
}

