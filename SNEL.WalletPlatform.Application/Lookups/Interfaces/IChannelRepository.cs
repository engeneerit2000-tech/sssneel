using SNEL.WalletPlatform.Domain.Lookups;

namespace SNEL.WalletPlatform.Application.Lookups.Interfaces;

public interface IChannelRepository
{
    Task<ChannelEntity?> GetByCodeAsync(string code);
}
