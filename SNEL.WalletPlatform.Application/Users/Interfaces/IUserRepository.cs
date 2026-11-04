using SNEL.WalletPlatform.Application.Interfaces.Repositories.Base;
using SNEL.WalletPlatform.Domain.Users;

namespace SNEL.WalletPlatform.Application.Users.Interfaces;

public interface IUserRepository : IEntityRepository<UserEntity,Guid>
{
    Task<bool> ExistsByMobileNoAsync(string mobileNo);
    Task<UserEntity?> GetByMobileNoAsync(string mobileNo);
    Task<UserEntity?> GetByIamRefAsync(string iamRef);
    Task<UserEntity?> GetByIamRefWithCustomerAndSubscriptionsAsync(string iamRef);
    Task<UserEntity?> GetByIamRefWithPartnerAsync(string iamRef, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAsync(string email);
    Task<bool> ExistsByNationalNoAsync(string nationalNo);
    
    Task<UserEntity?> GetByIamRefWithNotificationsDependenciesAsync(string iamRef);

    Task<List<UserEntity>> GetByUserIdsWithNotificationsDependenciesAsync(List<Guid> userIds);
    Task<List<UserEntity>> GetByPartnerIdsWithNotificationsDependenciesAsync(List<long> partnerIds);
    Task<IReadOnlyList<UserEntity>> GetActiveBatchByPartnerTypeForNotificationsAsync(
        int partnerTypeId,
        Guid? afterUserId,
        int batchSize,
        CancellationToken cancellationToken = default);
    Task<UserEntity?> GetByIdWithPartnerAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserEntity?> GetByIdWithDeleteDependenciesAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> HasRegistrationHistoryAsEmployeeAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<UserEntity>> GetPagedWithPartnerAsync(
        int pageNumber,
        int pageSize,
        string? search,
        UserStatus? status,
        long? partnerId,
        string? sortBy,
        bool isAscending,
        CancellationToken cancellationToken = default);
    Task<int> CountWithPartnerAsync(
        string? search,
        UserStatus? status,
        long? partnerId,
        CancellationToken cancellationToken = default);
}
