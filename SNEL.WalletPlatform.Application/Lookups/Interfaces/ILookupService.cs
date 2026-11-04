using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Lookups.Models.Output;

namespace SNEL.WalletPlatform.Application.Lookups.Interfaces;

public interface ILookupService
{
    Task<Result<AllLookupsOutputModel>> GetAllAsync();
}
