using SNEL.WalletPlatform.Domain.Billing;

namespace SNEL.WalletPlatform.Application.BillingServices.Models.Output;

public sealed record BillingServiceCurrencyOutputModel(
    long Id,
    int CurrencyId,
    decimal Min,
    decimal Max,
    BillingCommissionTypeEnum CommissionType,
    decimal CommissionValue,
    BillingFeesTypeEnum FeesType,
    decimal FeesValue,
    BillingCostTypeEnum CostType,
    decimal CostValue,
    decimal VAT,
    bool AutoSettleJournal,
    Guid BankAccountId,
    int Scale,
    bool Active,
    ApplyTypeEnum ApplyType
);
