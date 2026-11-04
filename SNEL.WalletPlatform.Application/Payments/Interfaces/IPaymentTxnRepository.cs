using SNEL.WalletPlatform.Application.Interfaces.Repositories.Base;
using SNEL.WalletPlatform.Domain.Payments;

namespace SNEL.WalletPlatform.Application.Payments.Interfaces;

public interface IPaymentTxnRepository : IEntityRepository<PaymentTxnEntity, Guid>
{
    Task<IReadOnlyList<PaymentTxnEntity>> GetByPaymentIdAsync(
        Guid paymentId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<int> CountByPaymentIdAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PaymentTxnEntity>> GetByPaymentIdAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default);
}
