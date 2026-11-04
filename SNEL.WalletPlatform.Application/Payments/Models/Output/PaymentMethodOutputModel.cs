using System.Text.Json;

namespace SNEL.WalletPlatform.Application.Payments.Models.Output;

public sealed record PaymentMethodCurrencyOutputModel(
    long Id,
    int CurrencyId,
    decimal Min,
    decimal Max,
    int FeesType,
    decimal FeesValue,
    int CostType,
    decimal CostValue,
    bool AutoSettleJournal,
    Guid? BankAccountId,
    decimal VAT,
    int Scale,
    bool Active
);

public sealed record PaymentMethodOutputModel(
    long Id,
    int PaymentCategoryId,
    long PaymentProviderId,
    string Code,
    string Name,
    int PaymentInitMode,
    int ConfirmationMode,
    int CompletionMode,
    int PaymentMethodPurpose,
    int ConfirmationType,
    bool Active,
    JsonElement? Config,
    IReadOnlyCollection<PaymentMethodCurrencyOutputModel> Currencies,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    string CreatedBy,
    string UpdatedBy
);
