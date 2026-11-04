using SNEL.WalletPlatform.Application.Common;
using SNEL.WalletPlatform.Application.Users.Models.Internal;
using SNEL.WalletPlatform.Domain.Users;

namespace SNEL.WalletPlatform.Application.Users.Interfaces;

public interface IUserManagementAccessPolicy
{
    Task<Result<ActorScope>> GetActorScopeAsync(CancellationToken cancellationToken = default);
    bool CanAccessUser(ActorScope scope, UserEntity user);
    bool CanManageRole(ActorScope scope, string role);
}
