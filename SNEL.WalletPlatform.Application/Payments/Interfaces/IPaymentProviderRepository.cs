using SNEL.WalletPlatform.Application.Interfaces.Repositories.Base;
using SNEL.WalletPlatform.Domain.Payments;

namespace SNEL.WalletPlatform.Application.Payments.Interfaces;

public interface IPaymentProviderRepository : IEntityRepository<PaymentProviderEntity,long>
{
    Task<IReadOnlyCollection<PaymentProviderEntity>> GetAllScopedByPartnerAsync(
        long? partnerId,
        CancellationToken cancellationToken = default);
    Task<PaymentProviderEntity> GetByPartnerId(long partnerId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<bool> ExistsByCodeExceptIdAsync(string code, long paymentProviderId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByPartnerIdAsync(long partnerId, CancellationToken cancellationToken = default);
}