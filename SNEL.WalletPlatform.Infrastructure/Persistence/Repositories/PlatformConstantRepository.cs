using Microsoft.EntityFrameworkCore;
using SNEL.WalletPlatform.Application.Lookups.Interfaces;
using SNEL.WalletPlatform.Domain.Lookups;
using SNEL.WalletPlatform.Infrastructure.Persistence.Repositories.Base;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Repositories;

public sealed class PlatformConstantRepository(ApplicationDbContext context)
    : LookupRepository<PlatformConstantEntity, int>(context), IPlatformConstantRepository
{
    public async Task<PlatformConstantEntity?> GetByGroupAndCode(PlatformConstantGroupsEnum group, PlatformConstantCodesEnum code, CancellationToken cancellationToken = default)
    {
        return await Context.PlatformConstants.FirstOrDefaultAsync(x => x.Group == group.ToString() && x.Code == code.ToString());
    }

    public async Task<string> GetValueByGroupAndCode(PlatformConstantGroupsEnum group, PlatformConstantCodesEnum code, CancellationToken cancellationToken = default)
    {
        return (await GetByGroupAndCode(group,code))!.Value;
    }

    public async Task<IReadOnlyList<PlatformConstantEntity>> GetAllWithTranslationsAsync(
        CancellationToken cancellationToken = default)
    {
        return await Context.PlatformConstants
            .AsNoTracking()
            .Include(x => x.Translations)
            .OrderBy(x => x.Group)
            .ThenBy(x => x.Order)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);
    }
}
