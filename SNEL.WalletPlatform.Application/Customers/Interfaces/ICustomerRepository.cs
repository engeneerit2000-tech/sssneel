using SNEL.WalletPlatform.Application.Interfaces.Repositories.Base;
using SNEL.WalletPlatform.Application.Customers.Models.Internal;
using SNEL.WalletPlatform.Domain.Customers;

namespace SNEL.WalletPlatform.Application.Customers.Interfaces;

public interface ICustomerRepository : IEntityRepository<CustomerEntity,Guid>
{
    Task<IReadOnlyCollection<CustomerEntity>> GetPagedAsync(CustomerQueryInternalModel query);
    Task<int> CountAsync(CustomerQueryInternalModel query);
    Task<CustomerEntity?> GetByPartnerIdAsync(long partnerId, CancellationToken cancellationToken = default);
    Task UpdateAsync(CustomerEntity customer);

    Task<CustomerEntity?> GetByIdWithDeleteRelationsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> HasPaymentsAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<bool> HasSubscriptionsAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<bool> HasFavoritesAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CustomerEntity>> GetActiveBatchForNotificationsAsync(
        Guid? afterCustomerId,
        int batchSize,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CustomerEntity>> GetActiveBatchWithMultipleSubscriptionsForNotificationsAsync(
        Guid? afterCustomerId,
        int batchSize,
        CancellationToken cancellationToken = default);
}
