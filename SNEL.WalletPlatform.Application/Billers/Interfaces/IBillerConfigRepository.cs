using SNEL.WalletPlatform.Application.Interfaces.Repositories.Base;
using SNEL.WalletPlatform.Domain.Billing;

namespace SNEL.WalletPlatform.Application.Billers.Interfaces;

public interface IBillerConfigRepository : IEntityRepository<BillerConfigEntity, long>
{
    Task UpsertAsync(
        string billerCode,
        string name,
        string billerName,
        string value,
        CancellationToken cancellationToken = default);

    Task<BillerConfigEntity?> GetByBillerCodeAndNameAsync(
        string billerCode,
        string name,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BillerConfigEntity>> GetByBillerCodeAsync(
        string billerCode,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BillerConfigEntity>> GetByBillerCodeForUpdateAsync(
        string billerCode,
        CancellationToken cancellationToken = default);
}
