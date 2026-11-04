// using SNEL.WalletPlatform.Domain.Common;
//
// namespace SNEL.WalletPlatform.Domain.Wallets;
//
// public sealed class WalletAccountTxnEntity : TrackedEntity<Guid>
// {
//     private WalletAccountTxnEntity() { }
//
//     private WalletAccountTxnEntity(
//         Guid walletAccountId,
//         WalletTransactionType type,
//         decimal debit,
//         decimal credit,
//         decimal balance,
//         string? sourceType,
//         string? sourceRef)
//     {
//         WalletAccountId = walletAccountId;
//         Type = type;
//         Debit = debit;
//         Credit = credit;
//         Balance = balance;
//         SourceType = sourceType;
//         SourceRef = sourceRef;
//     }
//
//     public Guid WalletAccountId { get; private set; }
//     public WalletAccountEntity WalletAccount { get; private set; } = null!;
//     public WalletTransactionType Type { get; private set; }
//     public decimal Debit { get; private set; }
//     public decimal Credit { get; private set; }
//     public decimal Balance { get; private set; }
//     public string? SourceType { get; private set; }
//     public string? SourceRef { get; private set; }
//
//     public static WalletAccountTxnEntity Create(
//         Guid walletAccountId,
//         WalletTransactionType type,
//         decimal debit,
//         decimal credit,
//         decimal balance,
//         string? sourceType = null,
//         string? sourceRef = null)
//     {
//         return new WalletAccountTxnEntity(
//             walletAccountId,
//             type,
//             debit,
//             credit,
//             balance,
//             string.IsNullOrWhiteSpace(sourceType) ? null : sourceType.Trim(),
//             string.IsNullOrWhiteSpace(sourceRef) ? null : sourceRef.Trim());
//     }
// }
//
