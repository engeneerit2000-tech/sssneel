using Microsoft.Extensions.Localization;
using SNEL.WalletPlatform.Application.Interfaces.Repositories.Base;
using SNEL.WalletPlatform.Application.Lookups.Models.Output;
using SNEL.WalletPlatform.Domain.Lookups;

namespace SNEL.WalletPlatform.Application.Lookups.Interfaces;

public interface ILookupRepository
{
    Task<AllLookupsOutputModel> GetAllAsync(IStringLocalizer<SharedResource> localizer);
    Task<List<CurrencyEntity>> GetAllCurrenciesAsync();
}
