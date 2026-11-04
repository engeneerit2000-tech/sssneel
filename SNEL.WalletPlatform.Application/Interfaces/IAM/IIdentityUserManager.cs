
namespace SNEL.WalletPlatform.Application.Interfaces.IAM;

public interface IIdentityUserManager
{
    Task<IdentityUserModel?> FindByUsernameAsync(string username);
    Task<IdentityUserModel?> FindByEmailAsync(string email);
    Task<IdentityUserModel> CreateUserAsync(CreateIdentityUserRequest request);
    Task AssignRealmRolesAsync(string identityUserId, IReadOnlyCollection<string> roles);
    Task<IReadOnlyCollection<string>> GetRealmRolesAsync(string identityUserId);
    Task ReplaceRealmRolesAsync(string identityUserId, IReadOnlyCollection<string> roles);
    Task DeleteUserAsync(string identityUserId);
    Task SetPasswordAsync(string userId, string password);
    Task UpdateUserAsync(UpdateIdentityUserRequest request);
    Task<bool> ValidatePasswordAsync(string username, string password);
}
