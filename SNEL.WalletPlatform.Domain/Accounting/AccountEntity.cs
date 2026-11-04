using System.ComponentModel.DataAnnotations.Schema;
using SNEL.WalletPlatform.Domain.Common;
using SNEL.WalletPlatform.Domain.Lookups;
using SNEL.WalletPlatform.Domain.Partners;

namespace SNEL.WalletPlatform.Domain.Accounting;

public sealed class AccountEntity : TrackedEntity<Guid>
{
    private AccountEntity() { }

    private AccountEntity(int accountTypeId, PartnerEntity partner, int currencyId, int order, decimal balance, int version)
    {
        Name = $"ACC-{partner.Id}";
        AccountTypeId = accountTypeId;
        Partner = partner;
        CurrencyId = currencyId;
        Order = order;
        Balance = balance;
        Version = version;
        AccountType = null!;
    }

    public string Name { get; private set; } = string.Empty;
    public int AccountTypeId { get; private set; }
    public AccountTypeEntity AccountType { get; private set; } = null!;
    /// <summary>Optional reference to partner (e.g. customer).</summary>
    public long PartnerId { get; private set; }
    public PartnerEntity? Partner { get; private set; }= null!;
    public int CurrencyId { get; private set; }
    public CurrencyEntity? Currency { get; private set; }
    public int Order { get; private set; }
    public decimal Balance { get; private set; }
    public int Version { get; private set; }

    public static AccountEntity Create(
        int accountTypeId,
        PartnerEntity partner,
        int currencyId,
        int order,
        decimal balance = 0)
    {
        return new AccountEntity(
            accountTypeId,
            partner,
            currencyId,
            order,
            balance,
            version: 0);
    }

    public void UpdateName(string name)
    {
        Name = string.IsNullOrWhiteSpace(name) ? Name : name.Trim();
    }

    public void SetBalance(decimal balance)
    {
        Balance = balance;
    }

    public void SetVersion(int version)
    {
        Version = version;
    }

    public void SetOrder(int order)
    {
        Order = order;
    }
}

