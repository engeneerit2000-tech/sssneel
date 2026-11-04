using SNEL.WalletPlatform.Domain.Common;
using SNEL.WalletPlatform.Domain.Lookups;

namespace SNEL.WalletPlatform.Domain.Billing;

public sealed class BillingServiceCurrencyEntity : TrackedEntity<long>
{
    private BillingServiceCurrencyEntity()
    {
    }

    public int CurrencyId { get; private set; }
    public CurrencyEntity Currency { get; private set; } = null!;
    public long BillingServiceId { get; private set; }
    public BillingServiceEntity BillingService { get; private set; } = null!;
    public decimal Min { get; private set; }
    public decimal Max { get; private set; }
    public BillingCommissionTypeEnum CommissionType { get; private set; }
    public decimal CommissionValue { get; private set; }
    public BillingFeesTypeEnum FeesType { get; private set; }
    public decimal FeesValue { get; private set; }
    public BillingCostTypeEnum CostType { get; private set; }
    public decimal CostValue { get; private set; }

    public bool AutoSettleJournal { get; private set; }
    public decimal VAT { get; private set; }
    public Guid BankAccountId { get; private set; }
    public int Scale { get; private set; }
    public bool Active { get; private set; }
    public ApplyTypeEnum ApplyType { get; set; }

    public static BillingServiceCurrencyEntity Create(
        int currencyId,
        long billingServiceId,
        decimal min,
        decimal max,
        BillingCommissionTypeEnum commissionType,
        decimal commissionValue,
        BillingFeesTypeEnum feesType,
        decimal feesValue,
        BillingCostTypeEnum costType,
        decimal costValue,
        decimal vat,
        bool autoSettleJournal,
        Guid bankAccountId,
        int scale,
        bool isActive)
    {
        return new BillingServiceCurrencyEntity
        {
            CurrencyId = currencyId,
            BillingServiceId = billingServiceId,
            Min = min,
            Max = max,
            CommissionType = commissionType,
            CommissionValue = commissionValue,
            FeesType = feesType,
            FeesValue = feesValue,
            CostType = costType,
            CostValue = costValue,
            AutoSettleJournal = autoSettleJournal,
            VAT = vat,
            BankAccountId = bankAccountId,
            Scale = scale,
            Active = isActive
        };
    }

    public static BillingServiceCurrencyEntity CreateForBillingService(
        int currencyId,
        BillingServiceEntity billingService,
        decimal min,
        decimal max,
        BillingCommissionTypeEnum commissionType,
        decimal commissionValue,
        BillingFeesTypeEnum feesType,
        decimal feesValue,
        BillingCostTypeEnum costType,
        decimal costValue,
        decimal vat,
        bool autoSettleJournal,
        Guid bankAccountId,
        int scale,
        bool isActive)
    {
        return new BillingServiceCurrencyEntity
        {
            CurrencyId = currencyId,
            BillingService = billingService,
            Min = min,
            Max = max,
            CommissionType = commissionType,
            CommissionValue = commissionValue,
            FeesType = feesType,
            FeesValue = feesValue,
            CostType = costType,
            CostValue = costValue,
            AutoSettleJournal = autoSettleJournal,
            VAT = vat,
            BankAccountId = bankAccountId,
            Scale = scale,
            Active = isActive
        };
    }

    public void Update(
        decimal min,
        decimal max,
        BillingCommissionTypeEnum commissionType,
        decimal commissionValue,
        BillingFeesTypeEnum feesType,
        decimal feesValue,
        BillingCostTypeEnum costType,
        decimal costValue,
        decimal vat,
        bool autoSettleJournal,
        Guid bankAccountId,
        int scale,
        bool isActive,
        ApplyTypeEnum applyType)
    {
        Min = min;
        Max = max;
        CommissionType = commissionType;
        CommissionValue = commissionValue;
        FeesType = feesType;
        FeesValue = feesValue;
        CostType = costType;
        CostValue = costValue;
        AutoSettleJournal = autoSettleJournal;
        VAT = vat;
        BankAccountId = bankAccountId;
        Scale = scale;
        Active = isActive;
        ApplyType = applyType;
    }

    public bool CheckLimits(decimal amount)
    {
        return amount >= Min && amount <= Max;
    }

    public BillingChargesRecord CalculateCharges(decimal amount)
    {
        var feesAmount = FeesType switch
        {
            BillingFeesTypeEnum.FLAT => FeesValue,
            BillingFeesTypeEnum.PERCENTAGE => Math.Round(amount * FeesValue / 100, Scale),
            _ => throw new InvalidOperationException("Invalid fees type")
        };
        var feesTaxAmount = Math.Round(VAT * feesAmount / 100, Scale);
        var costAmount = CostType switch
        {
            BillingCostTypeEnum.FLAT => CostValue,
            BillingCostTypeEnum.PERCENTAGE => Math.Round(amount * CostValue / 100, Scale),
            _ => throw new InvalidOperationException("Invalid cost type")
        };
        var costTaxAmount = Math.Round(VAT * costAmount / 100, Scale);

        var commissionAmount = CommissionType switch
        {
            BillingCommissionTypeEnum.FLAT => CommissionValue,
            BillingCommissionTypeEnum.PERCENTAGE => Math.Round(amount * CommissionValue / 100, Scale),
            _ => throw new InvalidOperationException("Invalid commission type")
        };
        var commissionTaxAmount = Math.Round(VAT * commissionAmount / 100, Scale);

        return new BillingChargesRecord(
            Fees: feesAmount,
            FeesTax: feesTaxAmount,
            Cost: costAmount,
            CostTax: costTaxAmount,
            Commission: commissionAmount,
            CommissionTax: commissionTaxAmount, 
            Amount: amount, 
            TotalAmount: amount + feesAmount + feesTaxAmount
        );
    }
}