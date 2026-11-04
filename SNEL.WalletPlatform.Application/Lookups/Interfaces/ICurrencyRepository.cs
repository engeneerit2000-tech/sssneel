using SNEL.WalletPlatform.Application.Interfaces.Repositories.Base;
using SNEL.WalletPlatform.Domain.Lookups;

namespace SNEL.WalletPlatform.Application.Lookups.Interfaces;

public interface ICurrencyRepository:ILookupRepository<CurrencyEntity,int>
{
}
