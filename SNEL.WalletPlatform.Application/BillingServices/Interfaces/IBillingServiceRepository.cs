using SNEL.WalletPlatform.Application.Interfaces.Repositories.Base;
using SNEL.WalletPlatform.Domain.Billing;

namespace SNEL.WalletPlatform.Application.BillingServices.Interfaces;

public interface IBillingServiceRepository : IEntityRepository<BillingServiceEntity,long>
{
    Task<BillingServiceEntity?> GetByIdWithRelationsAsync(long id);
    Task<IReadOnlyCollection<BillingServiceEntity>> GetAllWithRelationsScopedByBillerPartnerAsync(
        long? billerPartnerId,
        bool? isActive = true,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<BillingServiceEntity>> GetAllWithRelationsAsync(CancellationToken cancellationToken = default);
    Task<BillingServiceEntity?> GetByBillerCodeAndBillingServiceCodeWithCurrenciesAsync(string billerCode, string billingServiceCode);
    Task<bool> ExistsByCodeAsync(long billerId, string code);
    Task<bool> ExistsByCodeExceptIdAsync(long billerId, string code, long id);
    Task<bool> HasInvoicesAsync(long billingServiceId, CancellationToken cancellationToken = default);
    Task<bool> HasCustomerSubscriptionsAsync(long billingServiceId, CancellationToken cancellationToken = default);
}
