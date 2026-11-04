using System.Data;
using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Domain.Payments;

namespace SNEL.WalletPlatform.Application.BillingServices.Interfaces;

public interface ISnelOracleApplyCommandBuilder
{
    Result<IDbCommand> Build(
        IDbConnection connection,
        IReadOnlyList<PaymentOrderItemEntity> eligibleItems,
        decimal journalNumber,
        decimal startItemNumber,
        string posCode,
        decimal posId,
        string paymentMode);
}
