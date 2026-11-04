using Microsoft.EntityFrameworkCore;
using SNEL.WalletPlatform.Application.Accounting.Interfaces;
using SNEL.WalletPlatform.Application.Accounting.Models.Input;
using SNEL.WalletPlatform.Domain.Accounting;
using SNEL.WalletPlatform.Domain.Accounting.Enums;
using SNEL.WalletPlatform.Infrastructure.Persistence.Repositories.Base;
using System.Linq.Expressions;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Repositories;

public sealed class JournalLineRepository(ApplicationDbContext context)
    : EntityRepository<JournalLineEntity, Guid>(context), IJournalLineRepository
{
    private static readonly Dictionary<string, Expression<Func<JournalLineEntity, object>>> AllowedOrderBy =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["createdAtUtc"] = x => x.CreatedAtUtc,
            ["updatedAtUtc"] = x => x.UpdatedAtUtc,
            ["balance"] = x => x.Balance,
            ["id"] = x => x.Id
        };

    public async Task<IReadOnlyCollection<JournalLineEntity>> GetPagedAsync(JournalLinesQueryInputModel model)
    {
        var orderedQuery = ApplyOrdering(
            ApplyFilters(model),
            model.SortBy,
            model.IsAscending,
            AllowedOrderBy,
            nameof(JournalLineEntity.CreatedAtUtc));

        return await orderedQuery
            .Skip((model.PageNumber - 1) * model.PageSize)
            .Take(model.PageSize)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IReadOnlyCollection<JournalLineEntity>> GetJournalLineStatementAsync(JournalLinesStatementInternalModel model)
    {
        var orderedQuery = ApplyOrdering(
            ApplyFiltersForStatement(model),
            model.SortBy,
            model.IsAscending,
            AllowedOrderBy,
            nameof(JournalLineEntity.CreatedAtUtc));

        if (model.AddingFirstRecord && model.PageNumber == 1)
        {
            return await orderedQuery
               .Take(model.PageSize - 1)
               .AsNoTracking()
               .ToListAsync();
        }
        else if (model.AddingFirstRecord)
        {
            return await orderedQuery
               .Skip((model.PageNumber - 1) * model.PageSize - 1)
               .Take(model.PageSize)
               .AsNoTracking()
               .ToListAsync();
        }
        return await orderedQuery
               .Skip((model.PageNumber - 1) * model.PageSize)
               .Take(model.PageSize)
               .AsNoTracking()
               .ToListAsync();
    }

    public Task<int> CountAsync(JournalLinesQueryInputModel model)
    {
        return ApplyFilters(model).CountAsync();
    }
    public Task<int> CountAsync(JournalLinesStatementInternalModel model)
    {
        return ApplyFiltersForStatement(model).CountAsync();
    }

    private IQueryable<JournalLineEntity> ApplyFilters(JournalLinesQueryInputModel model)
    {
        IQueryable<JournalLineEntity> query = context.JournalLines
            //.Include(x => x.Journal)            
            .Include(x => x.JournalLineType)
            .Include(x => x.PlatformInvoice)
            .Where(x => x.AccountId == model.AccountId);

        if (!string.IsNullOrWhiteSpace(model.Search))
        {
            query = query.Where(x => x.Journal.Description.Contains(model.Search, StringComparison.OrdinalIgnoreCase));
        }

        if (model.JournalLineTypeId.HasValue)
        {
            query = query.Where(x => x.JournalLineTypeId == model.JournalLineTypeId.Value);
        }

        return query;
    }

    private IQueryable<JournalLineEntity> ApplyFiltersForStatement(JournalLinesStatementInternalModel model)
    {
        IQueryable<JournalLineEntity> query = context.JournalLines
            .Include(x => x.Journal)
            .Include(x => x.Account)
            .Include(x => x.JournalLineType)
            .Where(x => x.UpdatedAtUtc >= model.FromDate && (model.ToDate == null ? true : x.UpdatedAtUtc <= model.ToDate));        
        if (!string.IsNullOrWhiteSpace(model.Search))
        {
            query.Where(x => x.Account.Name.Contains(model.Search, StringComparison.OrdinalIgnoreCase) ||
            x.Journal.Description.Contains(model.Search, StringComparison.OrdinalIgnoreCase)
            );
        }

        if (model.JournalLineTypeId.HasValue)
        {
            query = query.Where(x => x.JournalLineTypeId == model.JournalLineTypeId.Value);
        }
        if (model.AccountTypeId.HasValue)
        {
            query = query.Where(x => x.Account.AccountTypeId == model.AccountTypeId);
        }
        if (model.PartnerId.HasValue)
        {
            query = query.Where(x => x.Account.PartnerId == model.PartnerId);
        }
        if (model.CurrencyId.HasValue)
        {
            query = query.Where(x => x.Account.CurrencyId == model.CurrencyId);
        }
        return query;
    }
}

