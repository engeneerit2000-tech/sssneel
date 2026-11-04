using SNEL.WalletPlatform.Application.Interfaces.Repositories.Base;
using SNEL.WalletPlatform.Domain.Payments;

namespace SNEL.WalletPlatform.Application.Payments.Interfaces;

public interface IPaymentMethodRepository : IEntityRepository<PaymentMethodEntity,long>
{
    Task<IReadOnlyCollection<PaymentMethodEntity>> GetAllWithRelationsScopedByProviderPartnerAsync(
        long? providerPartnerId,
        CancellationToken cancellationToken = default);
    Task<PaymentMethodEntity?> GetByCodeAndPspWithRelationsAsync(string paymentMethodCode, string paymentServiceProviderCode);
    Task<PaymentMethodEntity?> GetByCodeAsync(string paymentMethodCode);
    public Task<PaymentMethodEntity?> GetByIdWithRelationsAsync(long Id);
    Task<IReadOnlyCollection<PaymentMethodEntity>> GetByProviderIdWithRelationsAsync(long Id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<bool> ExistsByCodeExceptIdAsync(string code, long id, CancellationToken cancellationToken = default);
}
