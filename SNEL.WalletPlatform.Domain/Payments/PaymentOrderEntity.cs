using SNEL.WalletPlatform.Domain.Common;
using SNEL.WalletPlatform.Domain.Customers;
using SNEL.WalletPlatform.Domain.Lookups;

namespace SNEL.WalletPlatform.Domain.Payments;

public sealed class PaymentOrderEntity : TrackedEntity<Guid>
{
    private PaymentOrderEntity()
    {
        Status = PaymentOrderStatus.Pending;
        Description = string.Empty;
        Items = new List<PaymentOrderItemEntity>();
        Payments = new List<PaymentEntity>();
    }

    public Guid CustomerId { get; private set; }
    public int ChannelId { get; private set; }
    public string Description { get; private set; }
    public PaymentOrderStatus Status { get; private set; }
    public int CurrencyId { get; private set; }
    public decimal Amount { get; private set; }
    
    public decimal TotalFees { get; private set; }
    public decimal TotalFeesTax { get; private set; }
    public decimal TotalCost { get; private set; }
    public decimal TotalCostTax { get; private set; }
    public decimal TotalCommission { get; private set; }
    public decimal TotalCommissionTax { get; private set; }
    public decimal TotalAmount { get; private set; }

    public CustomerEntity? Customer { get; private set; }
    public ChannelEntity? Channel { get; private set; } 
    public CurrencyEntity? Currency { get; private set; }
    public ICollection<PaymentOrderItemEntity> Items { get; private set; }
    public ICollection<PaymentEntity> Payments { get; private set; }

    public static PaymentOrderEntity Create(
        Guid customerId,
        int channelId,
        string? description,
        PaymentOrderStatus status,
        int currencyId
        
        )
    {
        // if (amount < 0 || totalFees < 0 ||totalFeesTax<0 || totalCost<0 || totalCostTax<0 || totalCommission<0 || totalCommissionTax<0)
        //     throw new ArgumentException("Value cannot be negative");
        return new PaymentOrderEntity()
        {
            CustomerId = customerId,
            ChannelId = channelId,
            Description = description ?? string.Empty,
            Status = status,
            CurrencyId = currencyId,
            
        };
    }
    
    public void UpdateAmount(decimal amount, decimal totalFees, decimal totalFeesTax, decimal totalCost, decimal totalCostTax, decimal totalCommission, decimal totalCommissionTax)
    {
        if (amount < 0 || totalFees < 0 ||totalFeesTax<0 || totalCost<0 || totalCostTax<0 || totalCommission<0 || totalCommissionTax<0)
            throw new ArgumentException("Value cannot be negative");
        
        Amount = amount;
        TotalFees = totalFees;
        TotalFeesTax = totalFeesTax;
        TotalCost = totalCost;
        TotalCostTax = totalCostTax;
        TotalCommission = totalCommission;
        TotalCommissionTax = totalCommissionTax;
        TotalAmount = amount + totalFees + totalFeesTax;
    }

    public void ChangeStatus(PaymentOrderStatus status)
    {
        Status = status;
    }

    public PaymentOrderEntity SetCustomer(CustomerEntity customer)
    {
        Customer = customer;
        return this;
    }

    public PaymentOrderEntity SetItems(ICollection<PaymentOrderItemEntity> items)
    {
        Items = items;
        return this;
    }
}
