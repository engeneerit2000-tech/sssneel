using SNEL.WalletPlatform.Application.Interfaces.Repositories.Base;
using SNEL.WalletPlatform.Domain.Billing;

namespace SNEL.WalletPlatform.Application.Billers.Interfaces;

public interface IBillerRepository : IEntityRepository<BillerEntity,long>
{
    Task<IReadOnlyCollection<BillerEntity>> GetAllScopedByPartnerAsync(long? partnerId, CancellationToken cancellationToken = default);
    Task<BillerEntity?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<bool> ExistsByCodeAsync(string code);
    Task<bool> ExistsByCodeExceptIdAsync(string code, long billerId, CancellationToken cancellationToken = default);
}
