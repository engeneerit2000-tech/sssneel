// using SNEL.WalletPlatform.Domain.Common;
// using SNEL.WalletPlatform.Domain.Lookups;
//
// namespace SNEL.WalletPlatform.Domain.Wallets;
//
// public sealed class WalletAccountEntity : TrackedEntity<Guid>
// {
//     private WalletAccountEntity() { }
//
//     private WalletAccountEntity(
//         Guid walletId,
//         int currencyId,
//         string name,
//         WalletAccountStatus status,
//         decimal balance,
//         decimal blockedBalance,
//         int version)
//     {
//         WalletId = walletId;
//         CurrencyId = currencyId;
//         Name = name;
//         Status = status;
//         Balance = balance;
//         BlockedBalance = blockedBalance;
//         Version = version;
//     }
//
//     public Guid WalletId { get; private set; }
//     public WalletEntity Wallet { get; private set; } = null!;
//     public int CurrencyId { get; private set; }
//     public CurrencyEntity Currency { get; private set; } = null!;
//     public string Name { get; private set; } = string.Empty;
//     public WalletAccountStatus Status { get; private set; } = WalletAccountStatus.Active;
//     public decimal Balance { get; private set; }
//     public decimal BlockedBalance { get; private set; }
//     public int Version { get; private set; }
//
//     public static WalletAccountEntity Create(Guid walletId, int currencyId, string name, WalletAccountStatus status = WalletAccountStatus.Active)
//     {
//         return new WalletAccountEntity(
//             walletId,
//             currencyId,
//             name?.Trim() ?? throw new ArgumentNullException(nameof(name)),
//             status,
//             balance: 0,
//             blockedBalance: 0,
//             version: 0);
//     }
//
//     public void UpdateName(string name)
//     {
//         Name = string.IsNullOrWhiteSpace(name) ? Name : name.Trim();
//     }
//
//     public void ChangeStatus(WalletAccountStatus status)
//     {
//         Status = status;
//     }
//
//     public void SetBalance(decimal balance)
//     {
//         Balance = balance;
//     }
//
//     public void SetBlockedBalance(decimal blockedBalance)
//     {
//         BlockedBalance = blockedBalance;
//     }
//
//     public void SetVersion(int version)
//     {
//         Version = version;
//     }
// }
//
