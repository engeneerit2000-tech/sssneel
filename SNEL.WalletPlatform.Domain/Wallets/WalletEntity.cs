// using SNEL.WalletPlatform.Domain.Common;
// using SNEL.WalletPlatform.Domain.Customers;
//
// namespace SNEL.WalletPlatform.Domain.Wallets;
//
// public sealed class WalletEntity : TrackedEntity<Guid>
// {
//     private WalletEntity() { }
//
//     private WalletEntity(Guid customerId, string walletNo, string name, WalletStatus status)
//     {
//         CustomerId = customerId;
//         WalletNo = walletNo;
//         Name = name;
//         Status = status;
//     }
//
//     public Guid CustomerId { get; private set; }
//     public CustomerEntity Customer { get; private set; } = null!;
//     public string WalletNo { get; private set; } = string.Empty;
//     public string Name { get; private set; } = string.Empty;
//     public WalletStatus Status { get; private set; } = WalletStatus.Active;
//
//     public static WalletEntity Create(Guid customerId, string walletNo, string name, WalletStatus status = WalletStatus.Active)
//     {
//         return new WalletEntity(
//             customerId,
//             walletNo?.Trim() ?? throw new ArgumentNullException(nameof(walletNo)),
//             name?.Trim() ?? throw new ArgumentNullException(nameof(name)),
//             status);
//     }
//
//     public void UpdateName(string name)
//     {
//         Name = string.IsNullOrWhiteSpace(name) ? Name : name.Trim();
//     }
//
//     public void ChangeStatus(WalletStatus status)
//     {
//         Status = status;
//     }
// }
//
