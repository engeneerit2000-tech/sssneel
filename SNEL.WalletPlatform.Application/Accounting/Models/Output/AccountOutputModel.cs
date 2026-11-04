using SNEL.WalletPlatform.Application.Lookups.Models.Output;

namespace SNEL.WalletPlatform.Application.Accounting.Models.Output;

public sealed class AccountOutputModel
{
    public AccountOutputModel() { }

    public AccountOutputModel(
        Guid id,
        string name,
        int accountTypeId,
        AccountTypeOutputModel? accountType,
        long partnerId,
        int currencyId,
        CurrencyOutputModel? currency,
        int order,
        decimal balance,
        int version,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc,
        string? createdBy,
        string? updatedBy)
    {
        Id = id;
        Name = name;
        AccountTypeId = accountTypeId;
        AccountType = accountType;
        PartnerId = partnerId;
        CurrencyId = currencyId;
        Currency = currency;
        Order = order;
        Balance = balance;
        Version = version;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
        CreatedBy = createdBy;
        UpdatedBy = updatedBy;
    }

    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int AccountTypeId { get; set; }
    public AccountTypeOutputModel? AccountType { get; set; }
    public long PartnerId { get; set; }
    public int CurrencyId { get; set; }
    public CurrencyOutputModel? Currency { get; set; }
    public int Order { get; set; }
    public decimal Balance { get; set; }
    public int Version { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}
