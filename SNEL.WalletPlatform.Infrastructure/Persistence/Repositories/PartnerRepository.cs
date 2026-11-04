using Microsoft.EntityFrameworkCore;
using SNEL.WalletPlatform.Application.Common.Security;
using SNEL.WalletPlatform.Application.Partners.Interfaces;
using SNEL.WalletPlatform.Application.Partners.Models.Input;
using SNEL.WalletPlatform.Domain.Partners;
using SNEL.WalletPlatform.Infrastructure.Persistence.Repositories.Base;
using System.Linq.Expressions;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Repositories;

public sealed class PartnerRepository(ApplicationDbContext context)
    : EntityRepository<PartnerEntity,long>(context), IPartnerRepository
{
    private static readonly Dictionary<string, Expression<Func<PartnerEntity, object>>> AllowedOrderBy =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["createdAtUtc"] = x => x.CreatedAtUtc,
            ["updatedAtUtc"] = x => x.UpdatedAtUtc,
            ["partnerTypeId"] = x => x.PartnerTypeId,
            ["legalName"] = x => x.LegalName,
            ["displayName"] = x => x.DisplayName,
            ["id"] = x => x.Id
        };
    public Task<PartnerEntity?> GetByIdWithContactsAsync(long id)
    {
        return context.Partners
            .Include(x => x.Contacts)
                .ThenInclude(x => x.ContactType)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public Task<bool> ExistsByLegalNameAsync(string legalName)
    {
        var normalized = legalName.Trim().ToLower();

        return context.Partners.AnyAsync(x => x.LegalName.ToLower() == normalized);
    }

    public Task<bool> ExistsByLegalNameExceptIdAsync(string legalName, long partnerId, CancellationToken cancellationToken = default)
    {
        var normalized = legalName.Trim().ToLower();
        return context.Partners.AnyAsync(
            x => x.Id != partnerId && x.LegalName.ToLower() == normalized,
            cancellationToken);
    }

    public Task<bool> HasCustomersAsync(long partnerId, CancellationToken cancellationToken = default)
    {
        return context.Customers.AnyAsync(x => x.PartnerId == partnerId, cancellationToken);
    }

    public Task<bool> HasOtherBillersAsync(long partnerId, long? excludingBillerId = null, CancellationToken cancellationToken = default)
    {
        return context.Billers.AnyAsync(
            x => x.PartnerId == partnerId && (!excludingBillerId.HasValue || x.Id != excludingBillerId.Value),
            cancellationToken);
    }

    public Task<bool> HasOtherPaymentProvidersAsync(long partnerId, long? excludingPaymentProviderId = null, CancellationToken cancellationToken = default)
    {
        return context.PaymentProviders.AnyAsync(
            x => x.PartnerId == partnerId && (!excludingPaymentProviderId.HasValue || x.Id != excludingPaymentProviderId.Value),
            cancellationToken);
    }

    public Task<bool> HasPaymentMethodsAsync(long partnerId, CancellationToken cancellationToken = default)
    {
        return context.PaymentMethods.AnyAsync(
            x => x.PaymentProvider.PartnerId == partnerId,
            cancellationToken);
    }

    public Task<bool> HasPaymentsAsync(long partnerId, CancellationToken cancellationToken = default)
    {
        return context.Payments.AnyAsync(
            x => x.PaymentMethod.PaymentProvider.PartnerId == partnerId,
            cancellationToken);
    }

    public Task<bool> HasJournalLinesOnAccountsAsync(long partnerId, CancellationToken cancellationToken = default)
    {
        var accountIds = context.Accounts
            .Where(x => x.PartnerId == partnerId)
            .Select(x => x.Id);

        return context.JournalLines.AnyAsync(
            x => accountIds.Contains(x.AccountId),
            cancellationToken);
    }

    public Task<bool> HasPartnerContactsAsync(long partnerId, CancellationToken cancellationToken = default)
    {
        return context.PartnerContacts.AnyAsync(x => x.PartnerId == partnerId, cancellationToken);
    }

    public Task<int> CountByVisibilityScopeAsync(
        long actorPartnerId,
        bool canViewAllNonCustomerPartners,
        RelatedPartnersQueryInputModel query)
    {
        var filtered = ApplyVisibilityScopeFilter(actorPartnerId, canViewAllNonCustomerPartners);
        filtered = ApplyRelatedPartnersSearch(filtered, query.Search);
        return filtered.CountAsync();
    }

    public async Task<IReadOnlyCollection<PartnerEntity>> GetPagedByVisibilityScopeAsync(
        long actorPartnerId,
        bool canViewAllNonCustomerPartners,
        RelatedPartnersQueryInputModel query)
    {
        var filtered = ApplyVisibilityScopeFilter(actorPartnerId, canViewAllNonCustomerPartners);
        filtered = ApplyRelatedPartnersSearch(filtered, query.Search);
        filtered = filtered.Include(x => x.PartnerType);

        var orderedQuery = ApplyOrdering(
            filtered,
            query.SortBy,
            query.IsAscending,
            AllowedOrderBy,
            nameof(PartnerEntity.CreatedAtUtc));

        return await orderedQuery
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .AsNoTracking()
            .ToListAsync();
    }

    private IQueryable<PartnerEntity> ApplyVisibilityScopeFilter(
        long actorPartnerId,
        bool canViewAllNonCustomerPartners)
    {
        IQueryable<PartnerEntity> q = context.Partners;

        return canViewAllNonCustomerPartners
            ? q.Where(x => x.PartnerTypeId != PartnerTypeConstants.Customer)
            : q.Where(x => x.Id == actorPartnerId);
    }

    private static IQueryable<PartnerEntity> ApplyRelatedPartnersSearch(IQueryable<PartnerEntity> query, string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return query;
        }

        var term = search.Trim().ToLower();

        return query.Where(x =>
            x.LegalName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            x.DisplayName.Contains(term, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<IReadOnlyCollection<PartnerEntity>> GetPagedAsync(PartnersQueryInputModel query)
    {
        var orderedQuery = ApplyOrdering(
            ApplyFilters(query),
            query.SortBy,
            query.IsAscending,
            AllowedOrderBy,
            nameof(PartnerEntity.CreatedAtUtc));

        return await orderedQuery
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .AsNoTracking()
            .ToListAsync();
    }
    public Task<int> CountAsync(PartnersQueryInputModel model)
    {
        return ApplyFilters(model).CountAsync();
    }
    private IQueryable<PartnerEntity> ApplyFilters(PartnersQueryInputModel model)
    {
        IQueryable<PartnerEntity> query = context.Partners
            .Include(x => x.PartnerType);

        if (model.PartnerTypeId.HasValue)
        {
            query = query.Where(x => x.PartnerTypeId == model.PartnerTypeId.Value);
        }

        if (!string.IsNullOrWhiteSpace(model.Search))
        {
            var term = model.Search.Trim().ToLower();
            query = query.Where(x => x.LegalName.Contains(term, StringComparison.OrdinalIgnoreCase) || x.DisplayName.Contains(term, StringComparison.OrdinalIgnoreCase));    
        }

        return query;
    }
}
