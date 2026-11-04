using SNEL.WalletPlatform.Application.Accounting.Models.Output;

namespace SNEL.WalletPlatform.Application.Payments.Models.Output;

public sealed record PaymentProviderOutputModel(
    long Id,
    long PartnerId,
    string Code,
    string Name,
    bool Active,
    IReadOnlyCollection<AccountOutputModel> Accounts,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    string? CreatedBy,
    string? UpdatedBy);
