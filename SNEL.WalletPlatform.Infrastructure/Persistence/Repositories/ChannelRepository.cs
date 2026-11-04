using Microsoft.EntityFrameworkCore;
using SNEL.WalletPlatform.Application.Lookups.Interfaces;
using SNEL.WalletPlatform.Domain.Lookups;
using SNEL.WalletPlatform.Infrastructure.Persistence.Repositories.Base;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Repositories;

public sealed class ChannelRepository(ApplicationDbContext context) : LookupRepository<ChannelEntity,int>(context),IChannelRepository
{
    public Task<ChannelEntity?> GetByCodeAsync(string code)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();
        return context.Set<ChannelEntity>()
            .FirstOrDefaultAsync(x => x.Code == normalizedCode);
    }
}
