using SNEL.WalletPlatform.Application.Common.Models.Input;
using SNEL.WalletPlatform.Domain.Users;

namespace SNEL.WalletPlatform.Application.Users.Models.Input;

public sealed class UsersQueryInputModel : BaseQueryInputModel
{
    public UserStatus? Status { get; init; }
}
