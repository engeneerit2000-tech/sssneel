using SNEL.WalletPlatform.Application.Customers.Models.Internal;
using SNEL.WalletPlatform.Application.Interfaces.Repositories.Base;
using SNEL.WalletPlatform.Domain.Customers;

namespace SNEL.WalletPlatform.Application.Customers.Interfaces;

public interface ICustomerSubscriptionRepository : IEntityRepository<CustomerSubscriptionEntity,Guid>
{
    Task<CustomerSubscriptionEntity?> GetByIdWithRelationsAsync(Guid id);
    Task<CustomerSubscriptionEntity?> GetByIdForInquiryAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<CustomerSubscriptionEntity>> GetByIdsWithRelationsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default);
    Task<List<CustomerSubscriptionEntity>> GetByCustomerIdAsync(Guid customerId);
    Task<bool> ExistsAsync(Guid customerId, long billingServiceId, string billingRefValue, Guid[]? excludedIds = null);
    Task<IReadOnlyCollection<CustomerSubscriptionEntity>> GetPagedAsync(CustomerSubscriptionsQueryInternalModel query);
    Task<int> CountAsync(CustomerSubscriptionsQueryInternalModel query);
}
