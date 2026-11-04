using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Users.Models.Input;
using SNEL.WalletPlatform.Application.Users.Models.Output;

namespace SNEL.WalletPlatform.Application.Users.Interfaces;

public interface IUserManagementService
{
    Task<Result<PagedResult<ManagedUserOutputModel>>> GetUsersAsync(UsersQueryInputModel model, CancellationToken cancellationToken = default);
    Task<Result<ManagedUserOutputModel>> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<ManagedUserOutputModel>> CreateUserAsync(CreateManagedUserInputModel model, CancellationToken cancellationToken = default);
    Task<Result<ManagedUserOutputModel>> UpdateUserAsync(Guid id, UpdateManagedUserInputModel model, CancellationToken cancellationToken = default);
    Task<Result<ManagedUserOutputModel>> ChangeUserPasswordAsync(
        Guid id,
        ChangeManagedUserPasswordInputModel model,
        CancellationToken cancellationToken = default);
    Task<Result<ManagedUserOutputModel>> ChangeCustomerMobileAsync(
        Guid id,
        ChangeCustomerMobileInputModel model,
        CancellationToken cancellationToken = default);
    Task<Result<bool>> DeleteUserAsync(Guid id, CancellationToken cancellationToken = default);
}
