using Microsoft.EntityFrameworkCore;
using SNEL.WalletPlatform.Application.Accounting.Interfaces;
using SNEL.WalletPlatform.Application.Accounting.Models.Input;
using SNEL.WalletPlatform.Domain.Accounting;
using SNEL.WalletPlatform.Infrastructure.Persistence.Repositories.Base;
using System.Linq.Expressions;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Repositories;

public sealed class AccountRepository(ApplicationDbContext context)
    : EntityRepository<AccountEntity,Guid>(context), IAccountRepository
{
    private static readonly Dictionary<string, Expression<Func<AccountEntity, object>>> AllowedOrderBy =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["createdAtUtc"] = x => x.CreatedAtUtc,
            ["updatedAtUtc"] = x => x.UpdatedAtUtc,
            ["version"] = x => x.Version,
            ["balance"] = x => x.Balance,
            ["name"] = x => x.Name,
            ["order"] = x => x.Order,
            ["id"] = x => x.Id
        };

    public async Task<AccountEntity?> GetByIdWithRelationsbyPartnerIdAsync(long partnerId)
    {
        return await context.Accounts
            .Include(x => x.AccountType)
            .Include(x => x.Partner)
            .Include(x => x.Currency)
            .Where(x => x.PartnerId == partnerId)
            .FirstOrDefaultAsync();
    }

    public async Task<AccountEntity?> GetByPartnerIdAndCurrencyIdAndAccountTypeIdAsync(long partnerId, int currencyId,int accoutTypeId)
    {
        
        return await context.Accounts
            .Include(x => x.Currency).Include(x => x.Partner).Include(x=>x.AccountType)
            .Where(x => x.PartnerId == partnerId && x.CurrencyId==currencyId && x.AccountTypeId==accoutTypeId)
            .FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyCollection<AccountEntity>> GetByPartnerIdWithRelationsAsync(long partnerId, CancellationToken cancellationToken = default)
    {
        return await context.Accounts
            .Include(x => x.AccountType)
            .Include(x => x.Partner)
            .Include(x => x.Currency)
            .Where(x => x.PartnerId == partnerId)
            .ToListAsync(cancellationToken);
    }
    public async Task<IReadOnlyCollection<AccountEntity>> GetAllByPartnerIdAndCurrencyIdAndAccountTypeIdAsync(long partnerId, int currencyId, int accountTypeId)
    {
        return await context.Accounts
           .Include(x => x.AccountType)
           .Where(x => x.PartnerId == partnerId && x.AccountTypeId == accountTypeId && x.CurrencyId == currencyId)
           .ToListAsync();
    }
    public async Task<IReadOnlyCollection<AccountEntity>> GetPagedAsync(AccountsQueryInputModel query)
    {
        var orderedQuery = ApplyOrdering(
            ApplyFilters(query),
            query.SortBy,
            query.IsAscending,
            AllowedOrderBy,
            nameof(AccountEntity.CreatedAtUtc));

        return await orderedQuery
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .AsNoTracking()
            .ToListAsync();
    }

    public Task<int> CountAsync(AccountsQueryInputModel model)
    {
        return ApplyFilters(model).CountAsync();
    }

    private IQueryable<AccountEntity> ApplyFilters(AccountsQueryInputModel model)
    {
        IQueryable<AccountEntity> query = context.Accounts
            .Include(x => x.AccountType)
            .Include(x => x.Partner)
            .Include(x => x.Currency);
        if (model.PartnerId != null)
        {
            query = query.Where(x => x.PartnerId == model.PartnerId);
        }
        if (!string.IsNullOrWhiteSpace(model.Search))
        {
            var term = model.Search.Trim().ToLower();
            query = query.Where(x => x.Name.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        return query;
    }
}
