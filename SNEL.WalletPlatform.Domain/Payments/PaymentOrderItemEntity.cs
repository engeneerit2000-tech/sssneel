using SNEL.WalletPlatform.Domain.Billing;
using SNEL.WalletPlatform.Domain.Common;
using SNEL.WalletPlatform.Domain.Customers;
using SNEL.WalletPlatform.Domain.Lookups;

namespace SNEL.WalletPlatform.Domain.Payments;

public class PaymentOrderItemEntity : TrackedEntity<Guid>
{
    public Guid PaymentOrderId { get; private set; }
    public int ChannelId { get; private set; }
    public OrderItemType ItemType { get; private set; }
    public long? BillingServiceId { get; private set; }
    public Guid? CustomerSubscriptionId { get; private set; }
    public string? BillingRefValue { get; private set; }
    public Guid? InvoiceId { get; private set; }
    public Guid WalletAccountId { get; private set; }
    public decimal Amount { get; private set; }
    public decimal PaymentProviderFees { get; private set; }
    public decimal PaymentProviderFeesTax { get; private set; }

    public decimal BillerFees { get; private set; }
    public decimal BillerFeesTax { get; private set; }

    public decimal PaymentProviderCost { get; private set; }
    public decimal PaymentProviderCostTax { get; private set; }

    public decimal BillerCost { get; private set; }
    public decimal BillerCostTax { get; private set; }

    public decimal PaymentProviderCommission { get; private set; }
    public decimal PaymentProviderCommissionTax { get; private set; }

    public decimal BillerCommission { get; private set; }
    public decimal BillerCommissionTax { get; private set; }


    public decimal TotalFees { get; private set; }
    public decimal TotalFeesTax { get; private set; }
    public decimal TotalCost { get; private set; }
    public decimal TotalCostTax { get; private set; }
    public decimal TotalCommission { get; private set; }
    public decimal TotalCommissionTax { get; private set; }
    public decimal TotalAmount { get; private set; }

    public int? CurrencyId { get; private set; }
    public string? ExternalRef { get; private set; }
    public string? WebResponse { get; private set; }
    public PaymentOrderItemStatus Status { get; private set; } = PaymentOrderItemStatus.Created;

    public CustomerSubscriptionEntity? CustomerSubscription { get; private set; }
    public BillingServiceEntity? BillingService { get; private set; }
    public InvoiceEntity? Invoice { get; private set; }
    public PaymentOrderEntity? PaymentOrder { get; private set; }
    public ChannelEntity? Channel { get; private set; }
    public CurrencyEntity? Currency { get; private set; }

    public static PaymentOrderItemEntity Create(
        Guid paymentOrderId,
        int channelId,
        OrderItemType itemType,
        long? billingServiceId,
        Guid? customerSubscriptionId,
        string? billingRefValue,
        Guid? invoiceId,
        Guid walletAccountId,
        int currencyId,
        decimal amount,
        decimal pspFees,
        decimal pspFeesTax,
        decimal billerFees,
        decimal billerFeesTax,
        decimal pspCost,
        decimal pspCostTax,
        decimal billerCost,
        decimal billerCostTax,
        decimal pspCommission,
        decimal pspCommissionTax,
        decimal billerCommission,
        decimal billerCommissionTax)
    {
        if (amount < 0
            || pspFees < 0
            || pspFeesTax < 0
            || billerFees < 0
            || billerFeesTax < 0
            || pspCost < 0
            || pspCostTax < 0
            || billerCost < 0
            || billerCostTax < 0
            || pspCommission < 0
            || pspCommissionTax < 0
            || billerCommission < 0
            || billerCommissionTax < 0
           )
            throw new ArgumentException("Value cannot be negative");

        return new PaymentOrderItemEntity()
        {
            PaymentOrderId = paymentOrderId,
            ChannelId = channelId,
            ItemType = itemType,
            BillingServiceId = billingServiceId,
            CustomerSubscriptionId = customerSubscriptionId,
            BillingRefValue = billingRefValue,
            InvoiceId = invoiceId,
            WalletAccountId = walletAccountId,
            CurrencyId = currencyId,
            Amount = amount,
            PaymentProviderFees = pspFees,
            PaymentProviderFeesTax = pspFeesTax,
            BillerFees = billerFees,
            BillerFeesTax = billerFeesTax,
            PaymentProviderCost = pspCost,
            PaymentProviderCostTax = pspCostTax,
            BillerCost = billerCost,
            BillerCostTax = billerCostTax,
            PaymentProviderCommission = pspCommission,
            PaymentProviderCommissionTax = pspCommissionTax,
            BillerCommission = billerCommission,
            BillerCommissionTax = billerCommissionTax,
            TotalFees = pspFees + billerFees,
            TotalFeesTax = pspFeesTax + billerFeesTax,
            TotalCost = pspCost + billerCost,
            TotalCostTax = pspCostTax + billerCostTax,
            TotalCommission = pspCommission + billerCommission,
            TotalCommissionTax = pspCommissionTax + billerCommissionTax,
            TotalAmount = amount + pspFees + billerFees + pspFeesTax + billerFeesTax
        };
    }

    public void ChangeStatus(PaymentOrderItemStatus status)
    {
        Status = status;
    }

    public void SetExternalRef(string? externalRef)
    {
        ExternalRef = string.IsNullOrWhiteSpace(externalRef)
            ? null
            : externalRef.Trim();
    }

    public void SetWebResponse(string? webResponse)
    {
        WebResponse = string.IsNullOrWhiteSpace(webResponse)
            ? null
            : webResponse.Trim();
    }
    public decimal PSPSettlementAmount => Amount + PaymentProviderFees + PaymentProviderFeesTax - PaymentProviderCost - PaymentProviderCostTax - PaymentProviderCommission-PaymentProviderCommissionTax;
    public decimal BillerSettlementAmount => Amount + TotalCost + TotalCostTax - TotalCommission - TotalCommissionTax;


    #region override equality
    public static bool operator ==(PaymentOrderItemEntity? left, PaymentOrderItemEntity? right)
    {
        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    public static bool operator !=(PaymentOrderItemEntity? left, PaymentOrderItemEntity? right)
        => !(left == right);

    public bool Equals(PaymentOrderItemEntity? other)
    {
        if (other is null)
            return false;

        return ItemType == other.ItemType &&
            BillingServiceId == other.BillingServiceId &&
            CustomerSubscriptionId == other.CustomerSubscriptionId &&
            BillingRefValue == other.BillingRefValue &&
            InvoiceId == other.InvoiceId &&
            WalletAccountId == other.WalletAccountId &&
            Amount == other.Amount &&
            PaymentProviderFees == other.PaymentProviderFees &&
            PaymentProviderFeesTax == other.PaymentProviderFeesTax &&
            BillerFees == other.BillerFees &&
            BillerFeesTax == other.BillerFeesTax &&
            PaymentProviderCost == other.PaymentProviderCost &&
            PaymentProviderCostTax == other.PaymentProviderCostTax &&
            BillerCost == other.BillerCost &&
            BillerCostTax == other.BillerCostTax &&
            PaymentProviderCommission == other.PaymentProviderCommission &&
            PaymentProviderCommissionTax == other.PaymentProviderCommissionTax &&
            BillerCommission == other.BillerCommission &&
            BillerCommissionTax == other.BillerCommissionTax &&
            CurrencyId == other.CurrencyId;
    }

    public override bool Equals(object? obj)
        => obj is PaymentOrderItemEntity other && Equals(other);

    #endregion
}