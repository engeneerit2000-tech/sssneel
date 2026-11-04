using SNEL.WalletPlatform.Application.Interfaces.Repositories.Base;
using SNEL.WalletPlatform.Domain.Lookups;

namespace SNEL.WalletPlatform.Application.Lookups.Interfaces;

public interface ICenterRepository : ILookupRepository<CenterEntity, int>
{
    Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default);
}
