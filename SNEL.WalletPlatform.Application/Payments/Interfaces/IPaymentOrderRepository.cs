using SNEL.WalletPlatform.Application.Interfaces.Repositories.Base;
using SNEL.WalletPlatform.Domain.Payments;

namespace SNEL.WalletPlatform.Application.Payments.Interfaces;

public interface IPaymentOrderRepository : IEntityRepository<PaymentOrderEntity,Guid>
{
    Task<IReadOnlyList<PaymentOrderEntity>> GetPaymentHistoryByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<PaymentOrderEntity> GetByIdWithItemsAndCustomerAsync(Guid id, CancellationToken cancellationToken = default);
}