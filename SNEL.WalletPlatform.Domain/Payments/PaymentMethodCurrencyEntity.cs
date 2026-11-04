using SNEL.WalletPlatform.Domain.Common;
using SNEL.WalletPlatform.Domain.Lookups;

namespace SNEL.WalletPlatform.Domain.Payments;

public sealed class PaymentMethodCurrencyEntity : TrackedEntity<long>
{
    private PaymentMethodCurrencyEntity() { }

    public int CurrencyId { get; private set; }
    public long PaymentMethodId { get; private set; }
    public decimal Min { get; private set; }  // TODO: is it nullable ?
    public decimal Max { get; private set; } // TODO: is it nullable ?
    public PaymentFeesTypeEnum FeesType { get; private set; } 
    public decimal FeesValue { get; private set; }
    public PaymentCostTypeEnum CostType { get; private set; } 
    public decimal CostValue { get; private set; }
    public bool AutoSettleJournal { get; set; }
    public Guid? BankAccountId { get; set; }
    public decimal VAT { get; set; }
    public int Scale { get; set; }
    public bool Active { get; private set; }

    public int? MinimumAgeMinutes { get; set; }

    public CurrencyEntity? Currency { get; private set; } 
    public PaymentMethodEntity? PaymentMethod { get; private set; } 

    public static PaymentMethodCurrencyEntity Create(
        int currencyId,
        long paymentMethodId,
        decimal min,
        decimal max,
        PaymentFeesTypeEnum feesType ,
        decimal feesValue ,
        PaymentCostTypeEnum costType ,
        decimal costValue ,
        bool autoSettleJournal,
        Guid? bankAccountId,
        decimal vat,
        int scale,
        bool isActive = true,
        int? minimumAgeMinutes = null)
    {
        if (min > max)
            throw new ArgumentException("Min value cannot be greater than Max value");

        return new PaymentMethodCurrencyEntity()
        {
            CurrencyId = currencyId,
            PaymentMethodId = paymentMethodId,
            Min = min,
            Max = max,
            FeesType = feesType,
            FeesValue = feesValue,
            CostType = costType,
            CostValue = costValue,
            AutoSettleJournal = autoSettleJournal,
            Scale = scale,
            VAT = vat,
            BankAccountId = bankAccountId,
            Active = isActive,
            MinimumAgeMinutes = minimumAgeMinutes
        };
    }

    public static PaymentMethodCurrencyEntity CreateForPaymentMethod(
        int currencyId,
        PaymentMethodEntity paymentMethod,
        decimal min,
        decimal max,
        PaymentFeesTypeEnum feesType,
        decimal feesValue,
        PaymentCostTypeEnum costType,
        decimal costValue,
        bool autoSettleJournal,
        Guid? bankAccountId,
        decimal vat,
        int scale,
        bool isActive = true,
        int? minimumAgeMinutes = null)
    {
        if (min > max)
            throw new ArgumentException("Min value cannot be greater than Max value");

        return new PaymentMethodCurrencyEntity
        {
            CurrencyId = currencyId,
            PaymentMethod = paymentMethod,
            Min = min,
            Max = max,
            FeesType = feesType,
            FeesValue = feesValue,
            CostType = costType,
            CostValue = costValue,
            AutoSettleJournal = autoSettleJournal,
            Scale = scale,
            VAT = vat,
            BankAccountId = bankAccountId,
            Active = isActive,
            MinimumAgeMinutes = minimumAgeMinutes
        };
    }

    public void Update(
        decimal min,
        decimal max,
        PaymentFeesTypeEnum feesType,
        decimal feesValue,
        PaymentCostTypeEnum costType,
        decimal costValue,
        bool autoSettleJournal,
        Guid? bankAccountId,
        decimal vat,
        int scale,
        bool isActive,
        int? minimumAgeMinutes)
    {
        if (min > max)
            throw new ArgumentException("Min value cannot be greater than Max value");

        Min = min;
        Max = max;
        FeesType = feesType;
        FeesValue = feesValue;
        CostType = costType;
        CostValue = costValue;
        AutoSettleJournal = autoSettleJournal;
        BankAccountId = bankAccountId;
        VAT = vat;
        Scale = scale;
        Active = isActive;
        MinimumAgeMinutes = minimumAgeMinutes;
    }

    public void Activate() => Active = true;
    public void Deactivate() => Active = false;
    
    public bool CheckLimits(decimal amount)
    {
        return amount >= Min && amount <= Max;
    }

    public PaymentChargesRecord CalculateCharges(decimal amount)
    {
        var feesAmount = FeesType switch
        {
            PaymentFeesTypeEnum.FLAT => FeesValue,
            PaymentFeesTypeEnum.PERCENTAGE => Math.Round(amount * FeesValue / 100, Scale),
            _ => throw new InvalidOperationException("Invalid fees type")
        };
        var feesTaxAmount = Math.Round(VAT * feesAmount / 100, Scale);
        
        var costAmount = CostType switch
        {
            PaymentCostTypeEnum.FLAT => CostValue,
            PaymentCostTypeEnum.PERCENTAGE => Math.Round(amount * CostValue / 100, Scale),
            _ => throw new InvalidOperationException("Invalid cost type")
        };
        var costTaxAmount = Math.Round(VAT * costAmount / 100, Scale);

        return new PaymentChargesRecord(
            Fees: feesAmount,
            FeesTax: feesTaxAmount,
            Cost: costAmount,
            CostTax: costTaxAmount,
            Amount: amount, 
            TotalAmount: amount + feesAmount + feesTaxAmount
        );
    }
}
