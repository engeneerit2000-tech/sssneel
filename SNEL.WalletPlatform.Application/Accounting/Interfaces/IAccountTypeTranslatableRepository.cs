using SNEL.WalletPlatform.Application.Interfaces.Repositories.Base;
using SNEL.WalletPlatform.Domain.Accounting;

namespace SNEL.WalletPlatform.Application.Accounting.Interfaces;

public interface IAccountTypeTranslatableRepository : ITranslatableLookupRepository<AccountTypeEntity,int>
{
}

