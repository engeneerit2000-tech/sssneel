using SNEL.WalletPlatform.Application.Accounting.Models.Input;
using SNEL.WalletPlatform.Application.Interfaces.Repositories.Base;
using SNEL.WalletPlatform.Domain.Accounting;

namespace SNEL.WalletPlatform.Application.Accounting.Interfaces;

public interface IAccountRepository : IEntityRepository<AccountEntity,Guid>
{
    Task<int> CountAsync(AccountsQueryInputModel query);
    Task<AccountEntity?> GetByIdWithRelationsbyPartnerIdAsync(long partnerId);
    Task<AccountEntity?> GetByPartnerIdAndCurrencyIdAndAccountTypeIdAsync(long partnerId,int currencyId,int accountTypeId);
    Task<IReadOnlyCollection<AccountEntity>> GetByPartnerIdWithRelationsAsync(long partnerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<AccountEntity>> GetAllByPartnerIdAndCurrencyIdAndAccountTypeIdAsync(long partnerId, int currencyId, int accountTypeId);
    Task<IReadOnlyCollection<AccountEntity>> GetPagedAsync(AccountsQueryInputModel query);
}

