using SNEL.WalletPlatform.Domain.Common;
using SNEL.WalletPlatform.Domain.Customers;
using SNEL.WalletPlatform.Domain.Lookups;

namespace SNEL.WalletPlatform.Domain.Payments;

public sealed class PaymentEntity : TrackedEntity<Guid>
{
    private PaymentEntity()
    {
        PaymentTxns = new List<PaymentTxnEntity>();
    }

    private PaymentEntity(
        Guid paymentOrderId,
        long paymentMethodId,
        int currencyId,
        decimal amount,
        decimal? tax,
        decimal total,
        string? externalRef,
        string? phoneNumber,
        string? paymentRef,
        DateTimeOffset? statusChanged,
        PaymentStatus status,
        PaymentTypeEnum? paymentType,
        Guid customerId,int channelId,PaymentDirectionEnum direction, bool needsReview = false)
    {
        CustomerId = customerId;
        ChannelId = channelId;
        Direction = direction;
        PaymentOrderId = paymentOrderId;
        PaymentMethodId = paymentMethodId;
        CurrencyId = currencyId;
        Amount = amount;
        Tax = tax;
        Total = total;
        ExternalRef = externalRef;
        PhoneNumber = phoneNumber;
        PaymentRef = paymentRef;
        Status = status;
        PaymentTypeId = paymentType == null ? null : (int)paymentType.Value;
        PaymentTxns = new List<PaymentTxnEntity>();
        NeedsReview = needsReview;
        StatusChanged = statusChanged;
    }

    public Guid PaymentOrderId { get; private set; }
    public long PaymentMethodId { get; private set; }
    public int CurrencyId { get; private set; }
    public decimal Amount { get; private set; }
    public decimal? Tax { get; private set; }
    public decimal Total { get; private set; }
    public string? ExternalRef { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? PaymentRef { get; private set; }
    public PaymentStatus Status { get; private set; }
    public PaymentDirectionEnum? Direction { get; private set; } = default;
    public Guid? CustomerId { get; private set; }
    public int? ChannelId { get; private set; }
    public int? PaymentTypeId { get; private set; }
    public bool NeedsReview { get; private set; } = false;
    public DateTimeOffset? StatusChanged { get; private set; } = null;
    public PaymentOrderEntity? PaymentOrder { get; private set; } 
    public PaymentMethodEntity? PaymentMethod { get; private set; }
    public PaymentTypeEntity? PaymentType { get; private set; }
    public CurrencyEntity? Currency { get; private set; } 
    public ICollection<PaymentTxnEntity> PaymentTxns { get; private set; }

    public CustomerEntity? Customer { get; private set; }
    public ChannelEntity? Channel { get; private set; }

    public static PaymentEntity Create(
        Guid paymentOrderId,
        Guid customerId,
        int channelId,
        PaymentDirectionEnum direction,
        long paymentMethodId,
        int currencyId,
        decimal amount,
        decimal? tax,
        string? externalRef = null,
        string? phoneNumber = null,
        string? paymentRef = null,
        PaymentTypeEnum? paymentType = null,
        PaymentStatus status = PaymentStatus.Pending,
        bool needsReview = false,
        DateTimeOffset? statusChanged = null)
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative", nameof(amount));

        if (tax < 0)
            throw new ArgumentException("Tax cannot be negative", nameof(tax));

        return new PaymentEntity(
            paymentOrderId, paymentMethodId, currencyId, amount, tax, amount + (tax ?? 0),
            externalRef, phoneNumber, paymentRef, statusChanged, status, paymentType, customerId, channelId, direction, needsReview);
    }

    private void ChangeStatus(PaymentStatus newStatus)
    {
        Status = newStatus;
        StatusChanged = DateTimeOffset.UtcNow;
    }

    public void SetExternalRef(string? externalRef)
    {
        ExternalRef = string.IsNullOrWhiteSpace(externalRef)
            ? null
            : externalRef.Trim();
    }

    public void SetPhoneNumber(string? phoneNumber)
    {
        PhoneNumber = string.IsNullOrWhiteSpace(phoneNumber)
            ? null
            : phoneNumber.Trim();
    }

    public void SetNeedsReview(bool needsReview)
    {
        NeedsReview = needsReview;
    }

    public PaymentEntity SetPaymentOrder(PaymentOrderEntity po)
    {
        PaymentOrder = po;
        return this;
    }
    public PaymentEntity SetPaymentMethod(PaymentMethodEntity pm)
    {
        PaymentMethod = pm;
        return this;
    }

    public PaymentEntity ChangePaymentArtifactStatus(PaymentStatus status)
    {
        ChangeStatus(status);
        foreach (var paymentOrderItem in PaymentOrder?.Items ?? Enumerable.Empty<PaymentOrderItemEntity>())
        {
            switch (status)
            {
                case PaymentStatus.Pending:
                case PaymentStatus.Processing:
                    paymentOrderItem.ChangeStatus(PaymentOrderItemStatus.Created);
                    break;
                case PaymentStatus.Completed:
                    paymentOrderItem.ChangeStatus(PaymentOrderItemStatus.Paid);
                    break;
                case PaymentStatus.Failed:
                    paymentOrderItem.ChangeStatus(PaymentOrderItemStatus.Failed);
                    break;
                case PaymentStatus.Cancelled:
                    paymentOrderItem.ChangeStatus(PaymentOrderItemStatus.Cancelled);
                    break;
                case PaymentStatus.Expired:
                    paymentOrderItem.ChangeStatus(PaymentOrderItemStatus.Expired);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }

        return this;
    }
}
