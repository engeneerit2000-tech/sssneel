using SNEL.WalletPlatform.Domain.Common;
using System.Text.Json;

namespace SNEL.WalletPlatform.Domain.Webhooks;

public sealed class InterswitchWebhookEventEntity : TrackedEntity<Guid>
{
    private InterswitchWebhookEventEntity()
    {
        Event = string.Empty;
        Uuid = string.Empty;
        BankCode = string.Empty;
        PaymentReference = string.Empty;
        Channel = string.Empty;
        SplitAccounts = string.Empty;
        RetrievalReferenceNumber = string.Empty;
        ResponseCode = string.Empty;
        ResponseDescription = string.Empty;
        MerchantCustomerId = string.Empty;
        MerchantReference = string.Empty;
        CurrencyCode = string.Empty;
        MerchantCustomerName = string.Empty;
        CardNumber = string.Empty;
    }

    private InterswitchWebhookEventEntity(
        string @event,
        string uuid,
        long timestamp,
        decimal remittanceAmount,
        string bankCode,
        decimal amount,
        string paymentReference,
        string channel,
        string splitAccounts,
        string retrievalReferenceNumber,
        long transactionDate,
        string? accountNumber,
        string responseCode,
        string? token,
        string responseDescription,
        long paymentId,
        string merchantCustomerId,
        bool escrow,
        string merchantReference,
        string currencyCode,
        string merchantCustomerName,
        string cardNumber,
        WebhookEventStatus status,
        string? signature)
    {
        Event = @event;
        Uuid = uuid;
        Timestamp = timestamp;
        RemittanceAmount = remittanceAmount;
        BankCode = bankCode;
        Amount = amount;
        PaymentReference = paymentReference;
        Channel = channel;
        SplitAccounts = splitAccounts;
        RetrievalReferenceNumber = retrievalReferenceNumber;
        TransactionDate = transactionDate;
        AccountNumber = accountNumber;
        ResponseCode = responseCode;
        Token = token;
        ResponseDescription = responseDescription;
        PaymentId = paymentId;
        MerchantCustomerId = merchantCustomerId;
        Escrow = escrow;
        MerchantReference = merchantReference;
        CurrencyCode = currencyCode;
        MerchantCustomerName = merchantCustomerName;
        CardNumber = cardNumber;
        Status = status;
        Signature = signature;
    }

    public string Event { get; set; }
    public string Uuid { get; set; }
    public long Timestamp { get; set; }
    public decimal RemittanceAmount { get; set; }
    public string BankCode { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string PaymentReference { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public string SplitAccounts { get; set; } = string.Empty;
    public string RetrievalReferenceNumber { get; set; } = string.Empty;
    public long TransactionDate { get; set; }
    public string? AccountNumber { get; set; }
    public string ResponseCode { get; set; } = string.Empty;
    public string? Token { get; set; }
    public string ResponseDescription { get; set; } = string.Empty;
    public long PaymentId { get; set; }
    public string MerchantCustomerId { get; set; } = string.Empty;
    public bool Escrow { get; set; }
    public string MerchantReference { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = string.Empty;
    public string MerchantCustomerName { get; set; } = string.Empty;
    public string CardNumber { get; set; } = string.Empty;
    public WebhookEventStatus Status { get; private set; }
    public string? Signature { get; set; }


    public static InterswitchWebhookEventEntity Create(
        string @event,
        string uuid,
        long timestamp,
        decimal remittanceAmount,
        string bankCode,
        decimal amount,
        string paymentReference,
        string channel,
        List<object> splitAccounts,
        string retrievalReferenceNumber,
        long transactionDate,
        string? accountNumber,
        string responseCode,
        string? token,
        string responseDescription,
        long paymentId,
        string merchantCustomerId,
        bool escrow,
        string merchantReference,
        string currencyCode,
        string merchantCustomerName,
        string cardNumber,
        string? signature)
    {
        return new InterswitchWebhookEventEntity(
            @event,
            uuid,
            timestamp,
            remittanceAmount,
            bankCode,
            amount,
            paymentReference,
            channel,
            JsonSerializer.Serialize(splitAccounts),
            retrievalReferenceNumber,
            transactionDate,
            accountNumber,
            responseCode,
            token,
            responseDescription,
            paymentId,
            merchantCustomerId,
            escrow,
            merchantReference,
            currencyCode,
            merchantCustomerName,
            cardNumber,
            WebhookEventStatus.Pending,
            signature); 
    }

    public void ChangeStatus(WebhookEventStatus status)
    {
        Status = status;
    }
}

public enum WebhookEventStatus
{
    Pending,
    Processed,
    Failed,
    Duplicate,
    InvalidSignature,
    SignatureMissing
}
