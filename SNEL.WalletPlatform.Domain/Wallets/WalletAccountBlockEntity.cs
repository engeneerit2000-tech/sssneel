// using SNEL.WalletPlatform.Domain.Common;
//
// namespace SNEL.WalletPlatform.Domain.Wallets;
//
// public sealed class WalletAccountBlockEntity : TrackedEntity<Guid>
// {
//     private WalletAccountBlockEntity() { }
//
//     private WalletAccountBlockEntity(
//         Guid walletAccountId,
//         decimal amount,
//         BlockStatus status,
//         string? sourceType,
//         string? sourceRef)
//     {
//         WalletAccountId = walletAccountId;
//         Amount = amount;
//         Status = status;
//         SourceType = sourceType;
//         SourceRef = sourceRef;
//     }
//
//     public Guid WalletAccountId { get; private set; }
//     public WalletAccountEntity WalletAccount { get; private set; } = null!;
//     public decimal Amount { get; private set; }
//     public BlockStatus Status { get; private set; } = BlockStatus.Active;
//     public string? SourceType { get; private set; }
//     public string? SourceRef { get; private set; }
//
//     public static WalletAccountBlockEntity Create(
//         Guid walletAccountId,
//         decimal amount,
//         BlockStatus status = BlockStatus.Active,
//         string? sourceType = null,
//         string? sourceRef = null)
//     {
//         return new WalletAccountBlockEntity(
//             walletAccountId,
//             amount,
//             status,
//             string.IsNullOrWhiteSpace(sourceType) ? null : sourceType.Trim(),
//             string.IsNullOrWhiteSpace(sourceRef) ? null : sourceRef.Trim());
//     }
//
//     public void ChangeStatus(BlockStatus status)
//     {
//         Status = status;
//     }
// }
//
