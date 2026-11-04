using SNEL.WalletPlatform.Domain.Common;
using SNEL.WalletPlatform.Domain.Lookups;

namespace SNEL.WalletPlatform.Domain.Payments;

public sealed class PaymentTxnEntity : TrackedEntity<Guid>
{
    private PaymentTxnEntity() { }

    private PaymentTxnEntity(
        Guid paymentId,
        TransactionType type,
        TransactionStatus status,
        int currencyId,
        decimal amount,
        string? externalRef,
        string? reqBody,
        string? resBody,
        string? resCode,
        string? resMessage)
    {
        PaymentId = paymentId;
        Type = type;
        Status = status;
        CurrencyId = currencyId;
        Amount = amount;
        ExternalRef = externalRef;
        ReqBody = reqBody;
        ResBody = resBody;
        ResCode = resCode;
        ResMessage = resMessage;
    }

    public Guid PaymentId { get; private set; }
    public TransactionType Type { get; private set; }
    public TransactionStatus Status { get; private set; }
    public int CurrencyId { get; private set; }
    public decimal Amount { get; private set; }
    public string? ExternalRef { get; private set; }
    public string? ReqBody { get; private set; }
    public string? ResBody { get; private set; }
    public string? ResCode { get; private set; }
    public string? ResMessage { get; private set; }

    public PaymentEntity Payment { get; private set; } = null!;
    public CurrencyEntity Currency { get; private set; } = null!;

    public static PaymentTxnEntity Create(
        Guid paymentId,
        TransactionType type,
        TransactionStatus status,
        int currencyId,
        decimal amount,
        string? externalRef = null,
        string? reqBody = null,
        string? resBody = null,
        string? resCode = null,
        string? resMessage = null)
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative", nameof(amount));

        return new PaymentTxnEntity(
            paymentId, type, status, currencyId, amount,
            externalRef, reqBody, resBody, resCode, resMessage);
    }

    public PaymentTxnEntity SetStatus(TransactionStatus newStatus)
    {
        Status = newStatus;
        return this;
    }

    public PaymentTxnEntity SetRequestBody(string requestBody)
    {
        ReqBody = requestBody;
        return this;
    }

    public PaymentTxnEntity SetResponseBody(string responseBody)
    {
        ResBody = responseBody;
        return this;
    }

    public PaymentTxnEntity SetResponseCode(string resCode)
    {
        ResCode = resCode;
        return this;
    }

    public PaymentTxnEntity SetResponseMessage(string resMessage)
    {
        ResMessage = resMessage;
        return this;
    }
}
