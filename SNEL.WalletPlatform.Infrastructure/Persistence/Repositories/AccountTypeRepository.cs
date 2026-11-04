using SNEL.WalletPlatform.Application.Accounting.Interfaces;
using SNEL.WalletPlatform.Domain.Accounting;
using SNEL.WalletPlatform.Infrastructure.Persistence.Repositories.Base;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Repositories;

public sealed class AccountTypeRepository(ApplicationDbContext context)
    : LookupRepository<AccountTypeEntity,int>(context), IAccountTypeRepository
{
}

