using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SNEL.WalletPlatform.Application.Users.Interfaces;
using SNEL.WalletPlatform.Domain.Users;
using SNEL.WalletPlatform.Infrastructure.Persistence.Repositories.Base;

namespace SNEL.WalletPlatform.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(ApplicationDbContext context)
    : EntityRepository<UserEntity, Guid>(context), IUserRepository
{
    private static readonly Dictionary<string, Expression<Func<UserEntity, object>>> AllowedOrderBy =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["createdAtUtc"] = x => x.CreatedAtUtc,
            ["updatedAtUtc"] = x => x.UpdatedAtUtc,
            ["username"] = x => x.Username,
            ["status"] = x => x.Status,
            ["id"] = x => x.Id
        };

    public Task<bool> ExistsByMobileNoAsync(string mobileNo)
    {
        var normalized = mobileNo.Trim().ToLower();
        return context.Users.AnyAsync(x => x.MobileNo != null
            && x.Status != UserStatus.Inactive
            && x.MobileNo.ToLower() == normalized);
    }

    public Task<UserEntity?> GetByMobileNoAsync(string mobileNo)
    {
        var normalized = mobileNo.Trim().ToLower();
        return context.Users.FirstOrDefaultAsync(x => x.MobileNo != null
            && x.Status != UserStatus.Inactive
            && x.MobileNo.ToLower() == normalized);
    }

    public Task<UserEntity?> GetByIamRefAsync(string iamRef)
    {
        return context.Users.FirstOrDefaultAsync(x => x.IamRef != null
            && x.Status != UserStatus.Inactive
            && x.IamRef == iamRef);
    }

    public Task<UserEntity?> GetByIamRefWithCustomerAndSubscriptionsAsync(string iamRef)
    {
        return context.Users
            .FirstOrDefaultAsync(x => x.IamRef != null
            && x.Status == UserStatus.Active
            && x.IamRef == iamRef);
    }

    public Task<UserEntity?> GetByIamRefWithPartnerAsync(string iamRef, CancellationToken cancellationToken = default)
    {
        return context.Users
            .AsNoTracking()
            .Include(u => u.Partner)
                .ThenInclude(p => p!.PartnerType)
            .FirstOrDefaultAsync(x => x.IamRef != null
                                      && x.Status == UserStatus.Active
                                      && x.IamRef == iamRef,
                cancellationToken);
    }


    public Task<bool> ExistsByEmailAsync(string email)
    {
        var normalized = email.Trim().ToLower();
        return context.Users.AnyAsync(x => x.Email != null
            && x.Status != UserStatus.Inactive
            && x.Email.ToLower() == normalized);
    }

    public Task<bool> ExistsByNationalNoAsync(string nationalNo)
    {
        var normalized = nationalNo.Trim().ToLower();
        return context.Users.AnyAsync(x => x.NationalNo != null
            && x.Status != UserStatus.Inactive
            && x.NationalNo.ToLower() == normalized);
    }


    public async Task<List<UserEntity>> GetByUserIdsWithNotificationsDependenciesAsync(List<Guid> userIds)
    {
        var usersWithIncludes = await context.Users
            .Where(u => userIds.Contains(u.Id))
            .Include(u => u.NotificationPreferences)
            .ToListAsync();

        return usersWithIncludes;
    }
    
    public Task<UserEntity?> GetByIamRefWithNotificationsDependenciesAsync(string iamRef)
    {
        return  context.Users
            .Include(u => u.NotificationPreferences)
            .FirstOrDefaultAsync(x => x.IamRef != null
                                      && x.Status == UserStatus.Active
                                      && x.IamRef == iamRef);
        
       
    }

    public async Task<List<UserEntity>> GetByPartnerIdsWithNotificationsDependenciesAsync(List<long> partnerIds)
    {
        var usersWithIncludes = await context.Users
            .Include(u => u.NotificationPreferences)
            .Where(u => u.PartnerId.HasValue && partnerIds.Contains(u.PartnerId.Value))
            .ToListAsync();

        return usersWithIncludes;
    }

    public async Task<IReadOnlyList<UserEntity>> GetActiveBatchByPartnerTypeForNotificationsAsync(
        int partnerTypeId,
        Guid? afterUserId,
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        var query = context.Users
            .AsNoTracking()
            .Where(u =>
                u.Status == UserStatus.Active &&
                u.PartnerId.HasValue &&
                u.Partner!.PartnerTypeId == partnerTypeId);

        if (afterUserId.HasValue)
            query = query.Where(u => u.Id.CompareTo(afterUserId.Value) > 0);

        return await query
            .OrderBy(u => u.Id)
            .Take(batchSize)
            .Include(u => u.NotificationPreferences)
            .ToListAsync(cancellationToken);
    }

    public Task<UserEntity?> GetByIdWithPartnerAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return context.Users
            .Include(x => x.Partner)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<UserEntity?> GetByIdWithDeleteDependenciesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return context.Users
            .Include(x => x.Partner)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<bool> HasRegistrationHistoryAsEmployeeAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return context.CustomerRegistrationHistories.AnyAsync(x => x.EmployeeId == userId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<UserEntity>> GetPagedWithPartnerAsync(
        int pageNumber,
        int pageSize,
        string? search,
        UserStatus? status,
        long? partnerId,
        string? sortBy,
        bool isAscending,
        CancellationToken cancellationToken = default)
    {
        var effectiveAscending = string.IsNullOrWhiteSpace(sortBy) ? false : isAscending;
        var orderedQuery = ApplyOrdering(
            ApplyFilters(search, status, partnerId),
            sortBy,
            effectiveAscending,
            AllowedOrderBy,
            nameof(UserEntity.CreatedAtUtc));

        return await orderedQuery
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Include(x => x.Partner)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountWithPartnerAsync(
        string? search,
        UserStatus? status,
        long? partnerId,
        CancellationToken cancellationToken = default)
    {
        return ApplyFilters(search, status, partnerId).CountAsync(cancellationToken);
    }

    private IQueryable<UserEntity> ApplyFilters(string? search, UserStatus? status, long? partnerId)
    {
        IQueryable<UserEntity> query = context.Users;
        if (!string.IsNullOrWhiteSpace(search))
        {
            var tokens = search
                .Trim()
                .ToLowerInvariant()
                .Split((char[]?)null!, StringSplitOptions.RemoveEmptyEntries)
                .Distinct()
                .ToArray();

            if (tokens.Length == 0)
            {
                tokens = [search.Trim().ToLowerInvariant()];
            }

            // Each token must match somewhere (AND). Supports "Biller Admin" across first/last or partner names;
            // plus concatenated name so phrase substrings match one blob.
            foreach (var token in tokens)
            {
                query = query.Where(x =>
                    x.Username.ToLower().Contains(token)
                    || x.FirstName.ToLower().Contains(token)
                    || (x.SecondName != null && x.SecondName.ToLower().Contains(token))
                    || (x.ThirdName != null && x.ThirdName.ToLower().Contains(token))
                    || x.LastName.ToLower().Contains(token)
                    || (x.MobileNo != null && x.MobileNo.ToLower().Contains(token))
                    || (x.Email != null && x.Email.ToLower().Contains(token))
                    || (x.NationalNo != null && x.NationalNo.ToLower().Contains(token))
                    || (x.Partner != null && x.Partner.LegalName.ToLower().Contains(token))
                    || (x.Partner != null
                        && x.Partner.DisplayName != null
                        && x.Partner.DisplayName.ToLower().Contains(token))
                    || (x.FirstName + " " + (x.SecondName ?? "") + " " + (x.ThirdName ?? "") + " " + x.LastName)
                        .ToLower()
                        .Contains(token));
            }
        }

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        if (partnerId.HasValue)
        {
            query = query.Where(x => x.PartnerId == partnerId.Value);
        }

        return query;
    }
}
