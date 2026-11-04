using SNEL.WalletPlatform.Domain.Lookups;

namespace SNEL.WalletPlatform.Application.Lookups.Interfaces;

public interface IPlatformConstantRepository
{
    Task<PlatformConstantEntity?> GetByGroupAndCode(PlatformConstantGroupsEnum group, PlatformConstantCodesEnum code, CancellationToken cancellationToken = default);
    Task<string> GetValueByGroupAndCode(PlatformConstantGroupsEnum group, PlatformConstantCodesEnum code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PlatformConstantEntity>> GetAllWithTranslationsAsync(CancellationToken cancellationToken = default);
}
